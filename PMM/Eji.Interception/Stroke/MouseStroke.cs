using System.Runtime.InteropServices;

namespace Eji.Interception
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseStroke
    {

        public MouseState State;
        public MouseFlags Flags;

        public short Rolling;

        public int X;
        public int Y;

        public uint Information;

    }
}
