using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LittleBigMouseManager
{
    internal class ProcessManager
    {
        public readonly string processName;
        public readonly string processPath;
        private bool _onRestart = false;
        public bool onRestart { get => _onRestart; }
        private Process process;
        public ProcessManager(string processName, string processFallbackPath)
        {
            this.processName = processName;

            process = GetProcess();
            if (process == null
                || process.MainModule == null
                || process.MainModule.FileName == null)
            {
                processPath = processFallbackPath;
                return;
            }
            processPath = process.MainModule.FileName;
        }

        private void SetProcessSettings(Process process)
        {
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = process.MainModule.FileName;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = "--start";
        }

        private Process GetProcess()
        {
            Process[] processlist = Process.GetProcessesByName(processName);
            foreach (Process process in processlist)
            {
                if (process.ProcessName == processName)
                {
                    SetProcessSettings(process);
                    return process;
                }
            }
            return null;
        }
        public bool ProcessExists()
        {
            return File.Exists(processPath);
        }

        public bool IsRunning()
        {
            if (process == null)
            {
                process = GetProcess();
            }
            return !(process == null || process.HasExited);
        }

        public bool Start(string arguments = "")
        {
            if (_onRestart)
            {
                return false;
            }
            if (IsRunning())
            {
                return false;
            }
            if (process != null)
            {
                process.Start();
                return true;
            }

            process = Process.Start(processPath, arguments);
            if (process == null)
            {
                return false;
            }
            SetProcessSettings(process);
            if (process.HasExited)
            {
                process = GetProcess();
                return false;
            }
            return true;

        }

        public bool RawStart(string arguments)
        {
            Process argumentProcess = Process.Start(processPath, arguments);
            if (argumentProcess == null)
            {
                return false;
            }
            return argumentProcess.HasExited;
        }

        public void Restart()
        {
            _onRestart = true;
            if (process != null || !process.HasExited)
            {
                process.Kill();
                process.WaitForExit();
            }
            _onRestart = false;
            Start("--start");
        }

        public bool ProcessExitedAttach(EventHandler func)
        {
            if (process == null)
            {
                return false;
            }
            process.Exited += func;
            return true;
        }

    }
}
