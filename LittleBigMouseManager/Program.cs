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
using System.Text.Json;

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
        Screen[] lastScreens = Screen.AllScreens;
        long lastTimeTicks = DateTime.Now.Ticks;
        public static object timeLock = new object();
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
                    new MenuItem("Start LBM", (object sender, EventArgs e) => {manager.Start("--start");}),
                    new MenuItem("Open Settings", OpenSettings),
                    new MenuItem("Reload Settings", ReloadSettings),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        private bool CompareScreens(Screen[] screens1, Screen[] screens2)
        {
            bool sameScreens = true;
            foreach (Screen screen1 in screens1)
            {
                bool sameScreen = false;
                foreach (Screen screen2 in screens2)
                {
                    if (screen1.DeviceName == screen2.DeviceName
                        && screen1.BitsPerPixel == screen2.BitsPerPixel
                        && screen1.Bounds == screen2.Bounds
                        && screen1.Primary == screen2.Primary)
                    {
                        sameScreen = true;
                        break;
                    }
                }
                sameScreens &= sameScreen;
                if (!sameScreens)
                {
                    break;
                }
            }
            return sameScreens;
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

            SystemEvents.DisplaySettingsChanged += new EventHandler(async delegate (object sender, EventArgs e)
            {
                Screen[] screens = Screen.AllScreens;
                long timeTicks = DateTime.Now.Ticks;
                bool screenNotChanged = screens.Length == lastScreens.Length && CompareScreens(screens, lastScreens);
                Console.WriteLine(JsonSerializer.Serialize(Screen.AllScreens));
                lastScreens = screens;
                lock (timeLock)
                {
                    lastTimeTicks = timeTicks;
                }
                if (screenNotChanged)
                {
                    return;
                }
                await Task.Delay(Settings.loadedProperties.DisplayChangeTime);
                lock (timeLock)
                {
                    if (lastTimeTicks != timeTicks)
                    {
                        return;
                    }
                }
                if (Settings.loadedProperties.KillLBM)
                {
                    manager.Restart();
                }
                else
                {
                    manager.RawStart("--stop");
                    await Task.Delay(Settings.loadedProperties.SafetyTime);
                    manager.RawStart("--start");
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
