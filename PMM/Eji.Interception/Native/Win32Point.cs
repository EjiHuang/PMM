using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Eji.Interception.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public class Win32Point
    {

        public int X;
        public int Y;

        public Win32Point()
        {
            this.X = 0;
            this.Y = 0;
        }

        public Win32Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static implicit operator Point(Win32Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static implicit operator Win32Point(Point point)
        {
            return new Win32Point(point.X, point.Y);
        }

        public override string ToString()
        {
            return this.X.ToString() + "," + this.Y.ToString();
        }

    }
}
