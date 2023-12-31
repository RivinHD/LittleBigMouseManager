﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;
using System.IO;

namespace OffLine.Installer
{
    // Taken from:http://msdn2.microsoft.com/en-us/library/
    // system.configuration.configurationmanager.aspx
    // Set 'RunInstaller' attribute to true.

    [RunInstaller(true)]
    public class InstallerClass : System.Configuration.Install.Installer
    {
        public InstallerClass() : base()
        {
            this.Committed += new InstallEventHandler(MyInstaller_Committed);
            this.Committing += new InstallEventHandler(MyInstaller_Committing);
            this.AfterUninstall += new InstallEventHandler(MyInstaller_AfterUninstall);
        }

        // Event handler for 'Committing' event.
        private void MyInstaller_Committing(object sender, InstallEventArgs e)
        {
            //Console.WriteLine("");
            //Console.WriteLine("Committing Event occurred.");
            //Console.WriteLine("");
        }

        // Event handler for 'Committed' event.
        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filename = Assembly.GetExecutingAssembly().GetName().Name;
            string path = Path.Combine(directory, $"{filename}.exe");
            try
            {
                Directory.SetCurrentDirectory(directory);
                Process.Start(path);
            }
            catch { }

            Process.Start("C:\\Windows\\System32\\schtasks.exe", $"/create /sc ONLOGON /tn \"{filename}\" /tr \'\"{path}\"\' /rl HIGHEST /it");
        }

        private void MyInstaller_AfterUninstall(object sender, InstallEventArgs e)
        {
            string filename = Assembly.GetExecutingAssembly().GetName().Name;
            
            Process.Start("C:\\Windows\\System32\\schtasks.exe", $"/delete /tn \"{filename}\" /f");
        }

        // Override the 'Install' method.
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
        }

        // Override the 'Commit' method.
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        // Override the 'Rollback' method.
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }
    }
}