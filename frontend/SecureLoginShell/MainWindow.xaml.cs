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
            //RegisterTaskInTaskScheduler(); 
            //UnregisterTaskFromTaskScheduler();

        }

        private void RegisterTaskInTaskScheduler()
        {
            try
            {
                string appName = "HospitalLoginApp";
                string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

                if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
                {
                    MessageBox.Show("❌ Failed to locate application executable path.");
                    return;
                }

                using (TaskService ts = new TaskService())
                {
                    var existing = ts.GetTask(appName);
                    if (existing != null)
                        ts.RootFolder.DeleteTask(appName);

                    var trigger = new LogonTrigger
                    {
                        UserId = WindowsIdentity.GetCurrent().Name,
                        Delay = TimeSpan.FromSeconds(0)
                    };

                    var action = new ExecAction(appPath, null, Path.GetDirectoryName(appPath));

                    var td = ts.NewTask();
                    td.RegistrationInfo.Description = "Auto-launch HospitalLoginApp at user logon";
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.Triggers.Add(trigger);
                    td.Actions.Add(action);
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.StartWhenAvailable = true;
                    td.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(5);
                    td.Settings.Priority = ProcessPriorityClass.High;

                    ts.RootFolder.RegisterTaskDefinition(appName, td);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Failed to register task.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnregisterTaskFromTaskScheduler()
        {
            try
            {
                string appName = "HospitalLoginApp";
                using (TaskService ts = new TaskService())
                {
                    var existing = ts.GetTask(appName);
                    if (existing != null)
                    {
                        ts.RootFolder.DeleteTask(appName);
                        MessageBox.Show("🧹 Task unregistered successfully.");
                    }
                    else
                    {
                        MessageBox.Show("ℹ️ No task found to remove.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Failed to unregister task.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCredentialLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new CredentialLoginWindow
            {
                Owner = this
            };

            this.Hide(); // Hide main window before showing popup

            loginWindow.ShowDialog(); // Show modal

            this.Show(); // Show main window again when popup closes
        }


        private void BtnFaceLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new FaceLoginWindow
            {
                Owner = this
            };

            this.Hide(); // Hide main window before showing popup

            loginWindow.ShowDialog(); // Show modal

            this.Show(); // Show main window again when popup closes
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new RegisterWindow
            {
                Owner = this
            };

            this.Hide(); // Hide main window before showing popup

            loginWindow.ShowDialog(); // Show modal

            this.Show(); // Show main window again when popup closes
        }
    }
}
