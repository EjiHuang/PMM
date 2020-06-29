using Eji.Interception.Properties;
using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.Principal;
using System.IO;
using System.Collections.Generic;
using Context = System.IntPtr;
using Device = System.Int32;
using Filter = System.UInt16;
using Precedence = System.Int32;
using System.Runtime.InteropServices;

/// <summary>
/// Fork from 
/// </summary>

namespace Eji.Interception
{
    public static class InputInterceptor
    {
        /// <summary>
        /// Wrapper from dll
        /// </summary>
        private static DllWrapper dllWrapper;

        #region public field

        /// <summary>
        /// Check if the exe is running under administrator rights
        /// </summary>
        public static bool IsAdministratorRights =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        /// <summary>
        /// Check is need dispose
        /// </summary>
        public static bool NeedDispose { get; private set; }

        #region dll wrapper

        public static Context CreateContext() => dllWrapper.CreateContext();
        public static void DestroyContext(Context context) => dllWrapper.DestroyContext(context);
        public static Precedence GetPrecedence(Context context, Device device) => dllWrapper.GetPrecedence(context, device);
        public static void SetPrecedence(Context context, Device device, Precedence precedence) => dllWrapper.SetPrecedence(context, device, precedence);
        public static Filter GetFilter(Context context, Device device) => dllWrapper.GetFilter(context, device);
        public static void SetFilter(Context context, Predicate interception_predicate, KeyboardFilter filter) => dllWrapper.SetFilter(context, interception_predicate, (Filter)filter);
        public static void SetFilter(Context context, Predicate interception_predicate, MouseFilter filter) => dllWrapper.SetFilter(context, interception_predicate, (Filter)filter);
        public static void SetFilter(Context context, Predicate interception_predicate, Filter filter) => dllWrapper.SetFilter(context, interception_predicate, filter);
        public static Device Wait(Context context) => dllWrapper.Wait(context);
        public static Device WaitWithTimeout(Context context, UInt64 milliseconds) => dllWrapper.WaitWithTimeout(context, milliseconds);
        public static Int32 Send(Context context, Device device, ref Stroke stroke, UInt32 nstroke) => dllWrapper.Send(context, device, ref stroke, nstroke);
        public static Int32 Receive(Context context, Device device, ref Stroke stroke, UInt32 nstroke) => dllWrapper.Receive(context, device, ref stroke, nstroke);
        public static UInt32 GetHardwareId(Context context, Device device, IntPtr hardware_id_buffer, UInt32 buffer_size) => dllWrapper.GetHardwareId(context, device, hardware_id_buffer, buffer_size);
        public static Boolean IsInvalid(Device device) => dllWrapper.IsInvalid(device) != 0;
        public static Boolean IsKeyboard(Device device) => dllWrapper.IsKeyboard(device) != 0;
        public static Boolean IsMouse(Device device) => dllWrapper.IsMouse(device) != 0;

        #endregion

        #endregion

        #region ctor

        static InputInterceptor()
        {
            dllWrapper = null;
            NeedDispose = false;
        }

        #endregion

        /// <summary>
        /// InputInterceptor class init
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            try
            {
                var dllBytes = Environment.Is64BitProcess ? Resources.interception_x64 : Resources.interception_x86;
                dllWrapper = new DllWrapper(dllBytes);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Dispose resource
        /// </summary>
        /// <returns></returns>
        public static bool Dispose()
        {
            try
            {
                dllWrapper.Dispose();
                NeedDispose = false;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;

            }
        }

        /// <summary>
        /// Check interception driver is installed
        /// </summary>
        /// <returns></returns>
        public static bool CheckDriverInstalled()
        {
            var baseRegistryKey = Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Services");
            var keyboardRegistryKey = baseRegistryKey.OpenSubKey("keyboard");
            var mouseRegistryKey = baseRegistryKey.OpenSubKey("mouse");

            if (keyboardRegistryKey == null || mouseRegistryKey == null)
                return false;
            if ((string)keyboardRegistryKey.GetValue("DisplayName", string.Empty) != "Keyboard Upper Filter Driver")
                return false;
            if ((string)mouseRegistryKey.GetValue("DisplayName", string.Empty) != "Mouse Upper Filter Driver")
                return false;

            return true;
        }

        /// <summary>
        /// Install interception driver
        /// </summary>
        /// <returns></returns>
        public static bool InstallInterception()
        {
            if (IsAdministratorRights && !CheckDriverInstalled())
            {
                var tempFileName = Path.GetTempFileName();
                File.WriteAllBytes(tempFileName, Resources.install_interception);

                try
                {
                    var installer = new Process
                    {
                        StartInfo =
                        {
                            FileName = tempFileName,
                            Arguments = "/install",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };

                    installer.Start();
                    installer.WaitForExit();
                    Debug.WriteLine($"Installer terminated, exit code: {installer.ExitCode}, exitTime: {installer.ExitTime}");

                    return installer.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                try
                {
                    File.Delete(tempFileName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Get device list
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<DeviceData> GetDeviceList(Predicate predicate = null)
        {
            Context context = CreateContext();
            List<DeviceData> devices = GetDeviceList(context, predicate);
            DestroyContext(context);

            return devices;
        }

        /// <summary>
        /// Get device list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<DeviceData> GetDeviceList(Context context, Predicate predicate = null)
        {
            List<DeviceData> result = new List<DeviceData>();
            char[] buffer = new char[1024];
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            for (Device device = 1; device <= 20; device++)
            {
                if (predicate == null ? IsInvalid(device) == false : predicate(device))
                {
                    uint length = GetHardwareId(context, device, gcHandle.AddrOfPinnedObject(), (uint)buffer.Length);
                    if (length > 0) result.Add(new DeviceData(device, new string(buffer, 0, (int)length)));
                }
            }
            gcHandle.Free();
            return result;
        }
    }
}
