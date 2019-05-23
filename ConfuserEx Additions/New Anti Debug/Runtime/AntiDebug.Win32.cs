using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Confuser.Runtime
{
    internal static class AntiDebugWin32
    {
        static void Initialize()
        {
            string x = "COR";
            var env = typeof(Environment);
            var method = env.GetMethod("GetEnvironmentVariable", new[] { typeof(string) });

            Process here = GetParentProcess();
            if (here.ProcessName.ToLower().Contains("dnSpy"))
            {
                CrossAppDomainSerializer("START CMD /C \"ECHO dnSpy Detected ! && PAUSE\" ");
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + Application.ExecutablePath;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Process.GetCurrentProcess().Kill();
            }

            Process nhere = GetParentProcess();
            if (nhere.ProcessName.ToLower().Contains("-x86"))
            {
                CrossAppDomainSerializer("START CMD /C \"ECHO dnSpy Detected ! && PAUSE\" ");
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + Application.ExecutablePath;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Process.GetCurrentProcess().Kill();
            }

            if (method != null &&
                "1".Equals(method.Invoke(null, new object[] { x + "_ENABLE_PROFILING" })))
            Environment.FailFast(null);

            var thread = new Thread(Worker);
            thread.IsBackground = true;
            thread.Start(null);
        }

        internal static void CrossAppDomainSerializer(string A_0)
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + A_0)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        private static ParentProcessUtilities PPU;
        public static Process GetParentProcess()
        {
            return PPU.GetParentProcess();
        }

        [StructLayout(LayoutKind.Sequential)]

        struct ParentProcessUtilities
        {
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

            internal Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            public static Process GetParentProcess(int id)
            {
                Process process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            public static Process GetParentProcess(IntPtr handle)
            {
                ParentProcessUtilities pbi = new ParentProcessUtilities();
                int returnLength;
                int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                if (status != 0)
                    throw new System.ComponentModel.Win32Exception(status);

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }

        static void Worker(object thread)
        {
            var th = thread as Thread;
            if (th == null)
            {
                th = new Thread(Worker);
                th.IsBackground = true;
                th.Start(Thread.CurrentThread);
                Thread.Sleep(500);
            }
            while (true)
            {
                if (Debugger.IsAttached || Debugger.IsLogging())
                Environment.FailFast(null);

                if (!th.IsAlive)
                Environment.FailFast(null);

                Thread.Sleep(1000);
            }
        }
    }
}