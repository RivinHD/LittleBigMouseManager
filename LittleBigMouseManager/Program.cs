using Microsoft.Win32;
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

namespace LittleBigMouseManager
{
    internal class Program
    {
        static Mutex mutex = new Mutex(true, Assembly.GetEntryAssembly().GetCustomAttribute<GuidAttribute>().Value);

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
        private NotifyIcon trayIcon;
        private DateTime lastTime = DateTime.Now;
        private Settings.Properties properties;
        private ProcessManager manager;

        public TrayManager()
        {
            AttachEvents();
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
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
            manager.Start("--start");
            if (properties.ProcessPath != manager.processPath)
            {
                properties.ProcessPath = manager.processPath;
                Settings.Write(properties);
            }
            SystemEvents.DisplaySettingsChanged += new EventHandler(delegate (object sender, EventArgs e)
            {
                if (DateTime.Now < lastTime)
                {
                    return;
                }
                lastTime = DateTime.Now.AddMilliseconds(Settings.loadedProperties.SafetyTime);
                if (Settings.loadedProperties.KillLBM)
                {
                    manager.Restart();
                }
                else
                {
                    manager.RawStart("--start");
                    manager.RawStart("--stop");

                }
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
