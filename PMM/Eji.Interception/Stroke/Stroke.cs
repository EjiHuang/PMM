using System.Runtime.InteropServices;

namespace Eji.Interception
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Stroke
    {

        [FieldOffset(0)]
        public MouseStroke Mouse;

        [FieldOffset(0)]
        public KeyStroke Key;

    }
}
