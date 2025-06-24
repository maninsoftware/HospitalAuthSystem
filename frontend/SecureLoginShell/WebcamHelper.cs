using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HospitalLoginApp
{
    public class WebcamHelper
    {
        private readonly Image _imageControl;
        private readonly Dispatcher _dispatcher;
        private VideoCapture? _capture;
        private Thread? _previewThread;
        private volatile bool _isPreviewing = false;
        private Mat? _latestFrame;

        public WebcamHelper(Image imageControl, Dispatcher dispatcher)
        {
            _imageControl = imageControl;
            _dispatcher = dispatcher;
        }

        // Start live preview
        public void StartPreview()
        {
            if (_isPreviewing) return;

            _capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
            {
                _capture.Release();
                _capture = new VideoCapture(0, VideoCaptureAPIs.MSMF);
            }

            if (!_capture.IsOpened())
            {
                Console.WriteLine("❌ Webcam not found or can't be opened.");
                return;
            }

            _isPreviewing = true;
            _previewThread = new Thread(() =>
            {
                try
                {
                    _latestFrame = new Mat();
                    while (_isPreviewing)
                    {
                        _capture.Read(_latestFrame);
                        if (_latestFrame.Empty())
                        {
                            Console.WriteLine("⚠️ Frame is empty.");
                            continue;
                        }

                        var bitmap = _latestFrame.ToBitmapSource();
                        bitmap.Freeze();

                        _dispatcher.Invoke(() =>
                        {
                            _imageControl.Source = bitmap;
                        });

                        Thread.Sleep(30); // ~30 FPS
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Webcam Error] {ex.Message}");
                }
            })
            {
                IsBackground = true
            };
            _previewThread.Start();
        }

        // Capture current frame from preview
        public byte[]? CaptureImage()
        {
            if (_latestFrame == null || _latestFrame.Empty())
            {
                Console.WriteLine("❌ No frame available to capture.");
                return null;
            }

            using var matClone = _latestFrame.Clone();
            return matClone.ImEncode(".jpg");
        }

        // Stop preview cleanly
        public void StopPreview()
        {
            _isPreviewing = false;
            _previewThread?.Join();
            _capture?.Release();
            _capture?.Dispose();
            _latestFrame?.Dispose();

            _dispatcher.Invoke(() =>
            {
                _imageControl.Source = null;
            });
        }
    }
}
