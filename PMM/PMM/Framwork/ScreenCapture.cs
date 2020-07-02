using System;
using System.Drawing;
using System.Runtime.InteropServices;
// using WpfScreenHelper;

namespace PMM.Framwork
{
    public enum EnmScreenCaptureMode
    {
        Screen,
        Window
    }

    //public static class ScreenCapturer
    //{
    //    [DllImport("user32.dll")]
    //    private static extern IntPtr GetForegroundWindow();

    //    [DllImport("user32.dll")]
    //    private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    //    [StructLayout(LayoutKind.Sequential)]
    //    private struct Rect
    //    {
    //        public int Left;
    //        public int Top;
    //        public int Right;
    //        public int Bottom;
    //    }

    //    public static Bitmap Capture(EnmScreenCaptureMode screenCaptureMode = EnmScreenCaptureMode.Window)
    //    {
    //        System.Windows.Rect bounds;

    //        if (screenCaptureMode == EnmScreenCaptureMode.Screen)
    //        {
    //            bounds = Screen.PrimaryScreen.Bounds;
    //            CursorPosition = MouseHelper.MousePosition;
    //        }
    //        else
    //        {
    //            var foregroundWindowsHandle = GetForegroundWindow();
    //            var rect = new Rect();
    //            GetWindowRect(foregroundWindowsHandle, ref rect);
    //            bounds = new System.Windows.Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    //            CursorPosition = new System.Windows.Point((int)(MouseHelper.MousePosition.X - rect.Left), (int)(MouseHelper.MousePosition.Y - rect.Top));
    //        }

    //        var result = new Bitmap((int)bounds.Width, (int)bounds.Height);

    //        using (var g = Graphics.FromImage(result))
    //        {
    //            g.CopyFromScreen(new Point((int)bounds.Left, (int)bounds.Top), Point.Empty, new Size((int)bounds.Size.Width, (int)bounds.Size.Height));
    //        }

    //        return result;
    //    }

    //    public static System.Windows.Point CursorPosition { get; private set; }
    //}

    public class ScreenShotMaker
    {
        public static Bitmap CaptureScreen(double width, double height, double x = 0, double y = 0)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);
            if (iw <= 0) iw = 1;
            if (ih <= 0) ih = 1;
            Bitmap image = new Bitmap(iw, ih, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(image);
            graphics.CopyFromScreen(ix, iy, 0, 0, new System.Drawing.Size(iw, ih), CopyPixelOperation.SourceCopy);
            return image;
        }

        public static Bitmap SaveScreen(double width, double height, double x = 0, double y = 0)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);
            if (iw <= 0) iw = 1;
            if (ih <= 0) ih = 1;
            Bitmap myImage = new Bitmap(iw, ih);
            try
            {

                Graphics gr1 = Graphics.FromImage(myImage);
                IntPtr dc1 = gr1.GetHdc();
                IntPtr dc2 = NativeMethods.GetWindowDC(NativeMethods.GetForegroundWindow());
                NativeMethods.BitBlt(dc1, ix, iy, iw, ih, dc2, ix, iy, 13369376);
                gr1.ReleaseHdc(dc1);
            }
            catch { }
            return myImage;
        }

        internal class NativeMethods
        {

            [DllImport("user32.dll")]
            public extern static IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hwnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetForegroundWindow();
            [DllImport("gdi32.dll")]
            public static extern UInt64 BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, System.Int32 dwRop);

        }
    }
}
