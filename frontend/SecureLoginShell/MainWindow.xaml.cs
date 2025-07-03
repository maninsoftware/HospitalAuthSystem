using HospitalLoginApp.Helpers;
using HospitalLoginApp.Services;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TPL = System.Threading.Tasks;
using WpfImage = System.Windows.Controls.Image;
using DrawingImage = System.Drawing.Image;
using DrawingRectangle = System.Drawing.Rectangle;


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

                    LogonTrigger trigger = new LogonTrigger
                    {
                        UserId = WindowsIdentity.GetCurrent().Name,
                        Delay = TimeSpan.FromSeconds(0)
                    };

                    ExecAction action = new ExecAction(appPath, null, Path.GetDirectoryName(appPath));

                    TaskDefinition td = ts.NewTask();
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
            lblStatus.Text = "👀 Please blink within 5 seconds...";
            webcamHelper.ResetLiveness();
            webcamHelper.ActivateLivenessCheck();

            await TPL.Task.Delay(5000); // Wait for 5 seconds for the blink to be detected
            await TPL.Task.Delay(300);

            bool isLive = webcamHelper.IsFaceLive();

            Debug.WriteLine($"[DEBUG] Final Liveness Check Result = {isLive}");

            if (!isLive)
            {
                lblStatus.Text = "❌ Face not live or capture failed.";
                return;
            }

            lblStatus.Text = "📸 Capturing image...";
            byte[]? imageBytes = webcamHelper.CaptureImage();

            if (imageBytes == null)
            {
                lblStatus.Text = "❌ Face not live or capture failed.";
                return;
            }

            lblStatus.Text = "🔍 Verifying face...";
            string? username = await ApiService.VerifyFace(imageBytes);

            if (!string.IsNullOrEmpty(username))
            {
                lblStatus.Text = $"✅ Welcome, {username}! Loading your Homescreen...";
                webcamHelper.StopPreview();
                webcamHelper.ResetLiveness();
                await TPL.Task.Delay(2000);
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();
            }
            else
            {
                lblStatus.Text = "❌ Face not recognized. Please try again.";
                webcamHelper.ResetLiveness();
            }
        }



        private void BtnFaceLogin_Click(object sender, RoutedEventArgs e)
            if (imageBytes == null)
            {
                lblStatus.Text = "❌ Face not live or capture failed.";
                return;
            }

            lblStatus.Text = "📝 Registering...";
            string? responseMessage = await ApiService.RegisterUser(username, password, imageBytes);

            if (!string.IsNullOrEmpty(responseMessage))
            {
                lblStatus.Text = $"✅ {responseMessage}";
                await TPL.Task.Delay(2000);
                BtnCredentialMode_Click(this, new RoutedEventArgs());
            }
            else
            {
                lblStatus.Text = "❌ Registration failed.";
            }
        }

        private void EnsureWebcamInitialized(System.Windows.Controls.Image target)
        {
            webcamHelper?.Dispose();
            webcamHelper = new WebcamHelper(target, Dispatcher);
            webcamHelper.StartPreview();
        }

        private void BtnCredentialMode_Click(object sender, RoutedEventArgs e)
        {
            CredentialPanel.Visibility = Visibility.Visible;
            FacePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Collapsed;

            webcamHelper?.StopPreview();
            lblStatus.Text = "";
        }

        private void BtnFaceMode_Click(object sender, RoutedEventArgs e)
        {
            CredentialPanel.Visibility = Visibility.Collapsed;
            FacePanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;

            EnsureWebcamInitialized(imgWebcam);
            lblStatus.Text = "📷 Live preview started.";
        }

        private void BtnRegisterMode_Click(object sender, RoutedEventArgs e)
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
        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.Dispose();
            base.OnClosed(e);
        }
    }
}
