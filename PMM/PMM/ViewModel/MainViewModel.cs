using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using InputInterceptorNS;
using PMM.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TOS_TW_TOOL.Models;

namespace TosAutoSkill.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        enum Status_Flag
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
        /// 快捷键
        /// </summary>
        public HotKeyModel HotKey
        {
            get => GetProperty(() => HotKey);
            set => SetProperty(() => HotKey, value);
        }

        /// <summary>
        /// 技能CD
        /// </summary>
        public int CD
        {
            get => GetProperty(() => CD);
            set => SetProperty(() => CD, value);
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

        #endregion

        #region private

        /// <summary>
        /// 随机数生成器
        /// </summary>
        private readonly Random rnd = new Random();

        /// <summary>
        /// 后台执行定时器
        /// </summary>
        private readonly DispatcherTimer timer_auto_skill = new DispatcherTimer();

        /// <summary>
        /// 拦截器上下文
        /// </summary>
        private IntPtr interception_context;

        /// <summary>
        /// 用于拦截器的设备号
        /// </summary>
        private int keyboard_device;

        /// <summary>
        /// SP药水键冲程
        /// </summary>
        private Stroke stroke_skill = new Stroke();

        /// <summary>
        /// 用于记录技能触发时间
        /// </summary>
        private DateTime skill_invoke_time = DateTime.Now;

        /// <summary>
        /// 目标进程
        /// </summary>
        private Process process_target;

        #endregion

        #region public method

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            // InputInterceptor.Dispose();
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
                    interception_context = InputInterceptor.CreateContext();
                    var lt = InputInterceptor.GetDeviceList(interception_context);
                    keyboard_device = lt.Where(o => { return InputInterceptor.IsKeyboard(o.Device); }).FirstOrDefault().Device;

                    // 初始化成员对象
                    HotKey = new HotKeyModel
                    {
                        // 初始化热键设置
                        Key4SkillEnable = true,
                        HotKeyTextBoxFocusable4Skill = false
                    };

                    // 键盘Hook，用于获取热键设置
                    KeyboardHook keyboardHook = new KeyboardHook((ref KeyStroke keyStroke) =>
                    {
                        if (IsWinActive)
                        {
                            if (HotKey.HotKeyTextBoxFocusable4Skill)
                            {
                                HotKey.Key4Skill = keyStroke.Code.ToString();
                                stroke_skill.Key.Code = keyStroke.Code;
                                // HotKey.HotKeyTextBoxFocusable4Skill = false;
                            }
                        }
                    });
                }
            }
            else
            {
                SetStatusInfo(Status_Flag.err, "Input interceptor initialization failed.");
                MessageBox.Show("Input interceptor initialization failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            timer_auto_skill.Tick += Timer_auto_skill_Tick;
            timer_auto_skill.Interval = TimeSpan.FromSeconds(1);

            IsStopping = true;
        }

        private void Timer_auto_skill_Tick(object sender, EventArgs e)
        {
            if (HotKey.Key4SkillEnable && (!string.IsNullOrWhiteSpace(HotKey.Key4Skill)) && CD > 0)
            {
                Thread.Sleep(rnd.Next(1, 50));
                if ((DateTime.Now - skill_invoke_time).TotalSeconds > CD)
                {
                    if (GetForegroundWindow() == process_target.MainWindowHandle)
                    {
                        stroke_skill.Key.State = KeyState.Down;
                        InputInterceptor.Send(interception_context, keyboard_device, ref stroke_skill, 1);
                        Thread.Sleep(rnd.Next(1, 50));
                        stroke_skill.Key.State = KeyState.Up;
                        InputInterceptor.Send(interception_context, keyboard_device, ref stroke_skill, 1);

                        // 重置
                        skill_invoke_time = DateTime.Now;
                        SetStatusInfo(Status_Flag.info, "Running...");
                    }
                    else
                    {
                        SetStatusInfo(Status_Flag.err, "Foreground window isnt tos.");
                    }
                }
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

        /// <summary>
        /// 设置状态栏信息
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="text"></param>
        private void SetStatusInfo(Status_Flag flag, string text)
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

        #region command

        [Command]
        public void StartCommand(object obj)
        {
            var btn = obj as System.Windows.Controls.Primitives.ToggleButton;

            if (string.IsNullOrWhiteSpace(HotKey.Key4Skill) || !(CD > 0))
            {
                btn.IsChecked = false;
                MessageBox.Show("请检查是否输入了快捷键和技能时间。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (timer_auto_skill.IsEnabled)
            {
                timer_auto_skill.Stop();
                //btn.Content = "停止中...";
                //btn.Foreground = Brushes.Black;
                SetStatusInfo(Status_Flag.info, "Stopping...");
                IsStopping = true;
            }
            else
            {
                process_target = Process.GetProcessesByName(ProcessInfoes[ProcessIndex].ProcessName)[0];
                SetForegroundWindow(process_target.MainWindowHandle);

                // 延迟
                Thread.Sleep(TimeSpan.FromMilliseconds(500));

                stroke_skill.Key.State = KeyState.Down;
                InputInterceptor.Send(interception_context, keyboard_device, ref stroke_skill, 1);
                Thread.Sleep(rnd.Next(1, 50));
                stroke_skill.Key.State = KeyState.Up;
                InputInterceptor.Send(interception_context, keyboard_device, ref stroke_skill, 1);

                skill_invoke_time = DateTime.Now;
                timer_auto_skill.Start();
                //btn.Content = "启动中...";
                //btn.Foreground = Brushes.Green;
                SetStatusInfo(Status_Flag.info, "Running...");
                IsStopping = false;
            }
        }

        #endregion

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
    }
}
