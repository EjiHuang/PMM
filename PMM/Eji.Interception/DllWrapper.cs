using Eji.Interception.Native;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Eji.Interception
{
    internal class DllWrapper
    {
        private readonly string dllTempName;
        private readonly IntPtr dllPointer;

        public readonly InterceptionMethods.CreateContext CreateContext;
        public readonly InterceptionMethods.DestroyContext DestroyContext;
        public readonly InterceptionMethods.GetPrecedence GetPrecedence;
        public readonly InterceptionMethods.SetPrecedence SetPrecedence;
        public readonly InterceptionMethods.GetFilter GetFilter;
        public readonly InterceptionMethods.SetFilter SetFilter;
        public readonly InterceptionMethods.Wait Wait;
        public readonly InterceptionMethods.WaitWithTimeout WaitWithTimeout;
        public readonly InterceptionMethods.Send Send;
        public readonly InterceptionMethods.Receive Receive;
        public readonly InterceptionMethods.GetHardwareId GetHardwareId;
        public readonly InterceptionMethods.IsInvalid IsInvalid;
        public readonly InterceptionMethods.IsKeyboard IsKeyboard;
        public readonly InterceptionMethods.IsMouse IsMouse;

        public DllWrapper(byte[] dllBytes)
        {
            dllTempName = Path.GetTempFileName();
            File.WriteAllBytes(dllTempName, dllBytes);
            dllPointer = NativeMethods.LoadLibrary(dllTempName);

            CreateContext = GetFunction<InterceptionMethods.CreateContext>("interception_create_context");
            DestroyContext = GetFunction<InterceptionMethods.DestroyContext>("interception_destroy_context");
            GetPrecedence = GetFunction<InterceptionMethods.GetPrecedence>("interception_get_precedence");
            SetPrecedence = GetFunction<InterceptionMethods.SetPrecedence>("interception_set_precedence");
            GetFilter = GetFunction<InterceptionMethods.GetFilter>("interception_get_filter");
            SetFilter = GetFunction<InterceptionMethods.SetFilter>("interception_set_filter");
            Wait = GetFunction<InterceptionMethods.Wait>("interception_wait");
            WaitWithTimeout = GetFunction<InterceptionMethods.WaitWithTimeout>("interception_wait_with_timeout");
            Send = GetFunction<InterceptionMethods.Send>("interception_send");
            Receive = GetFunction<InterceptionMethods.Receive>("interception_receive");
            GetHardwareId = GetFunction<InterceptionMethods.GetHardwareId>("interception_get_hardware_id");
            IsInvalid = GetFunction<InterceptionMethods.IsInvalid>("interception_is_invalid");
            IsKeyboard = GetFunction<InterceptionMethods.IsKeyboard>("interception_is_keyboard");
            IsMouse = GetFunction<InterceptionMethods.IsMouse>("interception_is_mouse");
        }

        public void Dispose()
        {
            NativeMethods.FreeLibrary(dllPointer);
            File.Delete(dllTempName);
        }

        private TDelegate GetFunction<TDelegate>(string procedureName) where TDelegate : Delegate
        {
            IntPtr procedureAddress = NativeMethods.GetProcAddress(dllPointer, procedureName);
            return (TDelegate)Marshal.GetDelegateForFunctionPointer(procedureAddress, typeof(TDelegate));
        }
    }
}
