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
        private WebcamHelper? webcamHelper;

        public MainWindow()
        {
            InitializeComponent();
            txtPassword.Password = "Password";
            txtPassword.Tag = "placeholder";
            txtPassword.Foreground = new SolidColorBrush(Colors.Gray);
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

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Please enter credentials.";
                return;
            }

            lblStatus.Text = "Verifying credentials...";
            bool result = await ApiService.VerifyCredentials(username, password);
            if (result)
            {
                lblStatus.Text = $"✅ Welcome, {username}! Loading your Homescreen...";
                await TPL.Task.Delay(2000);
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();
            }
            else
            {
                lblStatus.Text = "❌ Login Failed!";
            }
        }

        private async void BtnFaceVerify_Click(object sender, RoutedEventArgs e)
        {
            if (webcamHelper == null)
            {
                lblStatus.Text = "❌ Webcam not initialized.";
                return;
            }

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

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = regUsername.Text.Trim();
            string password = regPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Please enter username and password.";
                return;
            }

            lblStatus.Text = "📸 Capturing registration photo...";
            webcamHelper ??= new WebcamHelper(regWebcam, Dispatcher);
            byte[]? imageBytes = webcamHelper.CaptureImage();

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
            CredentialPanel.Visibility = Visibility.Collapsed;
            FacePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;

            EnsureWebcamInitialized(regWebcam);
            lblStatus.Text = "📷 Prepare for registration photo.";
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text == "Username")
                tb.Text = "";
        }

        private void RestoreText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Username";
        }

        private void ClearPassword(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb && pb.Tag?.ToString() == "placeholder")
            {
                pb.Clear();
                pb.Tag = null;
                pb.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void RestorePassword(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb && string.IsNullOrWhiteSpace(pb.Password))
            {
                pb.Password = "Password";
                pb.Tag = "placeholder";
                pb.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.Dispose();
            base.OnClosed(e);
        }
    }
}
