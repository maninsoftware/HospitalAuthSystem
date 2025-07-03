using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using HospitalLoginApp.Windows;
using Microsoft.Win32.TaskScheduler;

namespace HospitalLoginApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Optional: RegisterTaskInTaskScheduler(); or UnregisterTaskFromTaskScheduler();
        }

        private void BtnCredentialLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new CredentialLoginWindow { Owner = this };
            this.Hide();
            window.ShowDialog();
            this.Show();
        }

        private void BtnFaceLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new FaceLoginWindow { Owner = this };
            this.Hide();
            window.ShowDialog();
            this.Show();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegisterWindow { Owner = this };
            this.Hide();
            window.ShowDialog();
            this.Show();
        }

        private void RegisterTaskInTaskScheduler()
        {
            try
            {
                string appName = "HospitalLoginApp";
                string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? "";

                if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
                {
                    MessageBox.Show("❌ Could not locate application path.");
                    return;
                }

                using var ts = new TaskService();
                ts.RootFolder.DeleteTask(appName, false);

                var td = ts.NewTask();
                td.RegistrationInfo.Description = "Launch HospitalLoginApp at user logon";
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;

                td.Triggers.Add(new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name });
                td.Actions.Add(new ExecAction(appPath, null, Path.GetDirectoryName(appPath)));

                ts.RootFolder.RegisterTaskDefinition(appName, td);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Task Scheduler Error: {ex.Message}");
            }
        }

        private void UnregisterTaskFromTaskScheduler()
        {
            try
            {
                using var ts = new TaskService();
                ts.RootFolder.DeleteTask("HospitalLoginApp", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to unregister task: {ex.Message}");
            }
        }
    }
}
