using System.Runtime.InteropServices;

namespace Eji.Interception
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyStroke
    {

        public KeyCode Code;
        public KeyState State;

        public uint Information;

    }
}
