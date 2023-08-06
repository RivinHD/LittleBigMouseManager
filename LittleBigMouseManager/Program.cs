﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using LittleBigMouseManager.Properties;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Management.Instrumentation;

namespace LittleBigMouseManager
{
    internal class Program
    {
        static readonly Mutex mutex = new Mutex(true, Assembly.GetEntryAssembly().GetCustomAttribute<GuidAttribute>().Value);

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new TrayManager());
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                CustomMessageBox.Show(
                    "Application is already running",
                    Assembly.GetEntryAssembly().GetName().Name,
                    CustomMessageBox.eDialogButtons.OK,
                    Resources.AppIcon);
            }
        }
    }

    public class TrayManager : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;
        private DateTime lastTime = DateTime.Now;
        private Settings.Properties properties;
        private ProcessManager manager;
        Task restart_application;
        private readonly SemaphoreSlim globalLock = new SemaphoreSlim(1);

        public TrayManager()
        {
            AttachEvents();
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Open Settings", OpenSettings),
                    new MenuItem("Reload Settings", ReloadSettings),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }


        private void AttachEvents()
        {
            Settings.Read();
            properties = Settings.loadedProperties;
            manager = new ProcessManager(properties.ProcessName, properties.ProcessPath);
            if (!manager.ProcessExists())
            {
                Application.Exit(); 
                CustomMessageBox.Show(
                    $"Could not find {properties.ProcessName}",
                    Assembly.GetEntryAssembly().GetName().Name,
                    CustomMessageBox.eDialogButtons.OK,
                    Resources.AppIcon);
            }
            manager.Start("--start");
            if (properties.ProcessPath != manager.processPath)
            {
                properties.ProcessPath = manager.processPath;
                Settings.Write(properties);
            }
            

            SystemEvents.DisplaySettingsChanged += new EventHandler(delegate (object sender, EventArgs e)
            {
                DateTime oldLastTime;
                globalLock.Wait();
                try
                {
                    oldLastTime = lastTime;
                    lastTime = DateTime.Now.AddMilliseconds(Settings.loadedProperties.SafetyTime);
                }
                finally
                {
                    globalLock.Release();
                }
                if (DateTime.Now < oldLastTime || (restart_application != null && !restart_application.IsCompleted))
                {
                    return;
                }

                restart_application = Task.Run(delegate ()
                {
                    DateTime lastTime;
                    globalLock.Wait();
                    try
                    {
                        lastTime = this.lastTime;
                    }
                    finally
                    {
                        globalLock.Release();
                    }
                    while (DateTime.Now < lastTime)
                    {
                        Task.Delay(lastTime - DateTime.Now).Wait();
                        globalLock.Wait();
                        try
                        {
                            lastTime = this.lastTime;
                        }
                        finally
                        {
                            globalLock.Release();
                        }
                    }

                    if (Settings.loadedProperties.KillLBM)
                    {
                        manager.Restart();
                    }
                    else
                    {
                        manager.RawStart("--stop");
                        Task.Delay(Settings.loadedProperties.SafetyTime).Wait();
                        manager.RawStart("--start");

                    }
                });
            });

            bool success = manager.ProcessExitedAttach(delegate (object sender, EventArgs e)
            {
                if (manager.onRestart)
                {
                    return;
                }
                try
                {
                    if (properties.RestartOnClose || (properties.RestartOnUnwantedClose && ((Process)sender).ExitCode != 0))
                    {
                        manager.Start("--start");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }

        void OpenSettings(object sender, EventArgs e)
        {
            Process.Start(Settings.fileName);
        }

        void ReloadSettings(object sender, EventArgs e)
        {
            Settings.Read();
            properties = Settings.loadedProperties;
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            Settings.Write(properties);
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
