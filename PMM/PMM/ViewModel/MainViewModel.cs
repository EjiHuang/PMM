using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using InputInterceptorNS;
using PMM.Model;
using PMM.View;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace PMM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public enum Status_Flag
        {
            info = 1,
            err = 0
        }

        #region ctor

        public MainViewModel()
        {
            Init();
        }

        #endregion

        #region public

        /// <summary>
        /// 委托用于启动
        /// </summary>
        public delegate void StartPMM(System.Windows.Controls.Primitives.ToggleButton btn);
        public StartPMM StartDel;

        /// <summary>
        /// 进程信息集合
        /// </summary>
        public ObservableCollection<ProcessModel> ProcessInfoes { get; set; }

        /// <summary>
        /// 进程索引
        /// </summary>
        public int ProcessIndex
        {
            get => GetProperty(() => ProcessIndex);
            set => SetProperty(() => ProcessIndex, value);
        }

        /// <summary>
        /// 状态栏标志
        /// </summary>
        public string StatusFlag
        {
            get => GetProperty(() => StatusFlag);
            set => SetProperty(() => StatusFlag, value);
        }

        /// <summary>
        /// 状态栏文本
        /// </summary>
        public string StatusText
        {
            get => GetProperty(() => StatusText);
            set => SetProperty(() => StatusText, value);
        }

        /// <summary>
        /// 驱动状态
        /// </summary>
        public string DriverStatus
        {
            get => GetProperty(() => DriverStatus);
            set => SetProperty(() => DriverStatus, value);
        }

        /// <summary>
        /// 是否停止状态
        /// </summary>
        public bool IsStopping
        {
            get => GetProperty(() => IsStopping);
            set => SetProperty(() => IsStopping, value);
        }

        /// <summary>
        /// 程序窗口是否为激活状态
        /// </summary>
        public bool IsWinActive = true;

        /// <summary>
        /// 是否开启图像识别功能
        /// </summary>
        public bool IsEnableImageRecognition
        {
            get => GetProperty(() => IsEnableImageRecognition);
            set => SetProperty(() => IsEnableImageRecognition, value);
        }

        /// <summary>
        /// 目标进程
        /// </summary>
        public Process Process_Target;

        /// <summary>
        /// 程序窗口位图
        /// </summary>
        public Bitmap Process_Window_Bitmap;

        /// <summary>
        /// 随机数生成器
        /// </summary>
        public readonly Random RND = new Random();

        /// <summary>
        /// 拦截器上下文
        /// </summary>
        public IntPtr Interception_Context;

        /// <summary>
        /// 用于拦截器的设备号
        /// </summary>
        public int Keyboard_Device;

        /// <summary>
        /// 截图遮罩窗口
        /// </summary>
        public CaptureMarkView Capture_Mark_View;

        #endregion

        #region private

        #endregion

        #region public method

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            // InputInterceptor.Dispose();
        }

        /// <summary>
        /// 设置状态栏信息
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="text"></param>
        public void SetStatusInfo(Status_Flag flag, string text)
        {
            if (flag == Status_Flag.err)
            {
                StatusFlag = "Error";
                StatusText = text;
            }
            else
            {
                StatusFlag = "Info";
                StatusText = text;
            }
        }

        #endregion

        #region private method

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            // 获取高权限令牌
            using Process p = Process.GetCurrentProcess();
            p.PriorityClass = ProcessPriorityClass.High;

            // 初始化进程列表
            ProcessInfoes = new ObservableCollection<ProcessModel>();
            var process_list = Process.GetProcesses().OrderBy(p => p.ProcessName, System.ComponentModel.ListSortDirection.Ascending);
            foreach (var prc in process_list)
            {
                ProcessInfoes.Add(new ProcessModel { ProcessName = prc.ProcessName, Pid = prc.Id });
            }

            // 初始化拦截器
            if (CheckInterceptorDriverInstalled())
            {
                if (InputInterceptor.Initialize())
                {
                    // 获取拦截器上下文及键盘设备
                    Interception_Context = InputInterceptor.CreateContext();
                    var lt = InputInterceptor.GetDeviceList(Interception_Context);
                    Keyboard_Device = lt.Where(o => { return InputInterceptor.IsKeyboard(o.Device); }).FirstOrDefault().Device;
                }
            }
            else
            {
                SetStatusInfo(Status_Flag.err, "Input interceptor initialization failed.");
                MessageBox.Show("Input interceptor initialization failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否安装拦截器驱动
        /// </summary>
        private bool CheckInterceptorDriverInstalled()
        {
            if (InputInterceptor.CheckDriverInstalled())
            {
                SetStatusInfo(Status_Flag.info, "Input interceptor is initialized.");
                DriverStatus = "Installed";
                return true;
            }
            else
            {
                DriverStatus = "Not installed";
                SetStatusInfo(Status_Flag.info, "Input interceptor not installed.");
                var msgRet = MessageBox.Show("Input interceptor not installed. Do you want to install it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgRet == MessageBoxResult.Yes)
                {
                    // 安装驱动
                    SetStatusInfo(Status_Flag.info, "Installing...");
                    if (InputInterceptor.InstallDriver())
                    {
                        SetStatusInfo(Status_Flag.info, "Done! Please restart your computer.");
                        MessageBox.Show("Done! Please restart your computer.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return true;
                    }
                    else
                    {
                        SetStatusInfo(Status_Flag.err, "Unknown exception, installation failed.");
                        MessageBox.Show("Unknown exception, installation failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region command

        [Command]
        public void StartCommand(object obj)
        {
            var btn = obj as System.Windows.Controls.Primitives.ToggleButton;

            StartDel?.Invoke(btn);
        }

        #endregion

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);
    }
}
