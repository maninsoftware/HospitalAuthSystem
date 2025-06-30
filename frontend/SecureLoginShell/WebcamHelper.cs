using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HospitalLoginApp
{
    public class WebcamHelper : IDisposable
    {
        private readonly Image _imageControl;
        private readonly Dispatcher _dispatcher;
        private VideoCapture? _capture;
        private Thread? _previewThread;
        private volatile bool _isPreviewing = false;
        private Mat? _latestFrame;
        private bool _disposed = false;

        public WebcamHelper(Image imageControl, Dispatcher dispatcher)
        {
            _imageControl = imageControl;
            _dispatcher = dispatcher;
        }

        public void StartPreview()
        {
            if (_isPreviewing || _disposed) return;

            try
            {
                _capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
                if (!_capture.IsOpened())
                {
                    _capture.Release();
                    _capture = new VideoCapture(0, VideoCaptureAPIs.MSMF);
                }

                if (!_capture.IsOpened())
                {
                    Console.WriteLine("❌ Webcam not available.");
                    return;
                }

                _isPreviewing = true;
                _latestFrame = new Mat();

                _previewThread = new Thread(() =>
                {
                    try
                    {
                        while (_isPreviewing)
                        {
                            _capture.Read(_latestFrame);
                            if (_latestFrame.Empty()) continue;

                            var bitmap = _latestFrame.ToBitmapSource();
                            bitmap.Freeze();

                            _dispatcher.Invoke(() =>
                            {
                                _imageControl.Source = bitmap;
                            });

                            Thread.Sleep(33);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[Preview Error] " + ex.Message);
                    }
                })
                {
                    IsBackground = true
                };
                _previewThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[StartPreview Error] " + ex.Message);
            }
        }

        public byte[]? CaptureImage()
        {
            if (_disposed || _latestFrame == null || _latestFrame.Empty())
            {
                Console.WriteLine("❌ No frame to capture.");
                return null;
            }

            using var matClone = _latestFrame.Clone();
            return matClone.ImEncode(".jpg");
        }

        public void StopPreview()
        {
            if (_disposed) return;

            _isPreviewing = false;

            try
            {
                _previewThread?.Join();
                _previewThread = null;

                _capture?.Release();
                _capture?.Dispose();
                _capture = null;

                _latestFrame?.Dispose();
                _latestFrame = null;

                _dispatcher.Invoke(() =>
                {
                    _imageControl.Source = null;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Stopping webcam: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            StopPreview();
            _disposed = true;
        }
    }
}
