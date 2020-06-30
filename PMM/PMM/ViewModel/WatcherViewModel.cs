using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using ImageProcessor;
using InputInterceptorNS;
using PMM.Framwork;
using PMM.View;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TOS_TW_TOOL.Models;
using static PMM.ViewModel.MainViewModel;

namespace PMM.ViewModel
{
    public class WatcherViewModel : ViewModelBase
    {
        #region ctor

        public WatcherViewModel(MainViewModel mainVM)
        {
            // 传递主视图模型
            MainVM = mainVM;
            // 初始化
            Init();
        }

        #endregion

        #region public

        public MainViewModel MainVM { get; set; }

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
        /// 截图图像
        /// </summary>
        public BitmapSource CaptureImage
        {
            get => GetProperty(() => CaptureImage);
            set => SetProperty(() => CaptureImage, value);
        }

        #endregion

        #region private

        /// <summary>
        /// 后台执行定时器
        /// </summary>
        private readonly DispatcherTimer timer_auto_run = new DispatcherTimer();

        /// <summary>
        /// 当前截图对象
        /// </summary>
        private Bitmap curr_caputure;

        /// <summary>
        /// 冲程
        /// </summary>
        private Stroke stroke = new Stroke();

        /// <summary>
        /// 用于记录触发时间
        /// </summary>
        private DateTime invoke_time = DateTime.Now;

        #endregion

        #region private method

        private void Init()
        {
            if (File.Exists("caputure.bmp"))
            {
                curr_caputure = new Bitmap("caputure.bmp");
                CaptureImage = BitmapProcessor.BitmapToImageSource(curr_caputure);
            }
            else
            {
                DrawingImage svgCapture = Application.Current.Resources["svg_capture"] as DrawingImage;
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(svgCapture, new Rect(new System.Windows.Point(0, 0), new System.Windows.Size(svgCapture.Width, svgCapture.Height)));
                drawingContext.Close();

                RenderTargetBitmap bmp = new RenderTargetBitmap((int)svgCapture.Width, (int)svgCapture.Height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);

                CaptureImage = bmp;
            }

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
                if (MainVM.IsWinActive)
                {
                    if (HotKey.HotKeyTextBoxFocusable4Skill)
                    {
                        HotKey.Key4Skill = keyStroke.Code.ToString();
                        stroke.Key.Code = keyStroke.Code;
                        // HotKey.HotKeyTextBoxFocusable4Skill = false;
                    }
                }
            });

            timer_auto_run.Tick += Timer_auto_run_Tick;
            timer_auto_run.Interval = TimeSpan.FromSeconds(1);

            MainVM.IsEnableImageRecognition = true;
            MainVM.IsStopping = true;

            MainVM.StartDel += new StartPMM(Start);
        }

        private void Start(System.Windows.Controls.Primitives.ToggleButton btn)
        {
            if (string.IsNullOrWhiteSpace(HotKey.Key4Skill) || !(CD > 0))
            {
                btn.IsChecked = false;
                MessageBox.Show("请检查是否输入了快捷键和冷却时间。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (timer_auto_run.IsEnabled)
            {
                timer_auto_run.Stop();
                MainVM.SetStatusInfo(Status_Flag.info, "Stopping...");
                MainVM.IsStopping = true;
            }
            else
            {
                MainVM.Process_Target = Process.GetProcessesByName(MainVM.ProcessInfoes[MainVM.ProcessIndex].ProcessName)[0];
                SetForegroundWindow(MainVM.Process_Target.MainWindowHandle);

                // 延迟
                Thread.Sleep(TimeSpan.FromMilliseconds(500));

                stroke.Key.State = KeyState.Down;
                InputInterceptor.Send(MainVM.Interception_Context, MainVM.Keyboard_Device, ref stroke, 1);
                Thread.Sleep(MainVM.RND.Next(1, 50));
                stroke.Key.State = KeyState.Up;
                InputInterceptor.Send(MainVM.Interception_Context, MainVM.Keyboard_Device, ref stroke, 1);

                invoke_time = DateTime.Now;
                timer_auto_run.Start();
                //btn.Content = "启动中...";
                //btn.Foreground = Brushes.Green;
                //btn.Foreground =  Brushes.Green;
                MainVM.SetStatusInfo(Status_Flag.info, "Running...");
                MainVM.IsStopping = false;
            }
        }

        private void Timer_auto_run_Tick(object sender, EventArgs e)
        {
            if (HotKey.Key4SkillEnable && (!string.IsNullOrWhiteSpace(HotKey.Key4Skill)) && CD > 0)
            {
                Thread.Sleep(MainVM.RND.Next(1, 50));
                if ((DateTime.Now - invoke_time).TotalSeconds > CD)
                {
                    if (GetForegroundWindow() == MainVM.Process_Target.MainWindowHandle)
                    {
                        if (MainVM.IsEnableImageRecognition)
                        {
                            GetWindowRect(MainVM.Process_Target.MainWindowHandle, out Rectangle rect);

                            try
                            {
                                MainVM.Process_Window_Bitmap = new Bitmap(rect.Width - rect.X, rect.Height - rect.Y);
                                using Graphics g = Graphics.FromImage(MainVM.Process_Window_Bitmap);
                                g.CopyFromScreen(rect.X, rect.Y, 0, 0, MainVM.Process_Window_Bitmap.Size);

                                if (curr_caputure != null)
                                {
                                    // var image_checker = new ImageChecker(process_window_bitmap, curr_caputure);
                                    DateTime begin = DateTime.Now;

                                    var pos = ImageRecognition.GetSubPositionsOpenCV(MainVM.Process_Window_Bitmap, curr_caputure);

                                    var time = (DateTime.Now - begin).TotalMilliseconds;

                                    if (pos.Count == 1)
                                    {
                                        Debug.WriteLine($"[{pos[0].X},{pos[0].Y}]");

                                        stroke.Key.State = KeyState.Down;
                                        InputInterceptor.Send(MainVM.Interception_Context, MainVM.Keyboard_Device, ref stroke, 1);
                                        Thread.Sleep(MainVM.RND.Next(1, 50));
                                        stroke.Key.State = KeyState.Up;
                                        InputInterceptor.Send(MainVM.Interception_Context, MainVM.Keyboard_Device, ref stroke, 1);
                                    }
                                    else
                                    {
                                        Debug.WriteLine("NG");
                                    }

                                    // 重置
                                    invoke_time = DateTime.Now;
                                    MainVM.SetStatusInfo(Status_Flag.info, $"Running... matched[{pos.Count}]: {time} ms");
                                }
                                else
                                {
                                    MainVM.SetStatusInfo(Status_Flag.err, $"Capture bitmap is null.");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                                throw;
                            }
                        }
                        else
                        {
                            stroke.Key.State = KeyState.Down;
                            InputInterceptor.Send(MainVM.Interception_Context, MainVM.Keyboard_Device, ref stroke, 1);
                            Thread.Sleep(MainVM.RND.Next(1, 50));
                            stroke.Key.State = KeyState.Up;
                            InputInterceptor.Send(MainVM.Interception_Context, MainVM.Keyboard_Device, ref stroke, 1);

                            // 重置
                            invoke_time = DateTime.Now;
                            MainVM.SetStatusInfo(Status_Flag.info, "Running...");
                        }
                    }
                    else
                    {
                        MainVM.SetStatusInfo(Status_Flag.err, $"Foreground window isnt {MainVM.Process_Target.ProcessName}.");
                    }
                }
            }
        }

        #endregion

        #region command

        [Command]
        public void CaptureCommand(object obj)
        {
            // System.Windows.Controls.Image image = obj as System.Windows.Controls.Image;
            MainVM.Capture_Mark_View = new CaptureMarkView();
            MainVM.Capture_Mark_View.Closing += (sender, e) =>
            {
                curr_caputure?.Dispose();
                curr_caputure = MainVM.Capture_Mark_View.bitmap;
                curr_caputure.Save("caputure.bmp", ImageFormat.Bmp);
                CaptureImage = BitmapProcessor.BitmapToImageSource(curr_caputure);
            };
            MainVM.Capture_Mark_View.Show();
        }

        #endregion
    }
}
