using DlibDotNet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HospitalLoginApp.Helpers;
using Point = DlibDotNet.Point;
using WpfImage = System.Windows.Controls.Image;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingRectangle = System.Drawing.Rectangle;

namespace HospitalLoginApp.Services
{
    public class WebcamHelper : IDisposable
    {
        private readonly WpfImage imageControl;
        private readonly Dispatcher dispatcher;
        private VideoCapture? capture;
        private Thread? cameraThread;
        private bool isRunning = false;

        private readonly BlinkDetectionHelper blinkHelper = new();
        private bool isLivenessActive = false;
        private DateTime livenessStartTime;
        private readonly TimeSpan livenessDuration = TimeSpan.FromSeconds(5);
        private bool livenessConfirmed = false;

        private readonly FrontalFaceDetector faceDetector;
        private readonly ShapePredictor shapePredictor;

        public WebcamHelper(WpfImage target, Dispatcher dispatcher)
        {
            imageControl = target;
            this.dispatcher = dispatcher;

            faceDetector = Dlib.GetFrontalFaceDetector();
            shapePredictor = ShapePredictor.Deserialize("Assets/Models/shape_predictor_68_face_landmarks.dat");
        }

        public void StartPreview()
        {
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
                throw new Exception("Cannot access webcam.");

            isRunning = true;
            cameraThread = new Thread(CameraLoop);
            cameraThread.Start();
        }

        public void StopPreview()
        {
            isRunning = false;
            cameraThread?.Join();

            capture?.Release();
            capture?.Dispose();
            capture = null;
        }

        private void CameraLoop()
        {
            while (isRunning)
            {
                using var frameMat = new Mat();
                if (capture == null || !capture.Read(frameMat) || frameMat.Empty())
                    continue;

                using var bitmap = BitmapConverter.ToBitmap(frameMat);
                using var cloned = CloneBitmap(bitmap);

                dispatcher.Invoke(() =>
                {
                    var bmpSource = ConvertToBitmapSource(cloned);
                    imageControl.Source = bmpSource;
                });

                if (isLivenessActive && (DateTime.Now - livenessStartTime) <= livenessDuration)
                {
                    using var dlibImage = LoadBitmapToDlibImage(cloned);
                    var faces = faceDetector.Operator(dlibImage);
                    //Debug.WriteLine($"[DEBUG] Number of faces detected = {faces.Length}");

                    if (faces.Length > 0)
                    {
                        var shape = shapePredictor.Detect(dlibImage, faces[0]);

                        var landmarks = new Point[68];
                        for (int i = 0; i < 68; i++)
                            landmarks[i] = shape.GetPart((uint)i);

                        blinkHelper.ProcessLandmarks(landmarks);
                    }
                }
                else if (isLivenessActive)
                {
                    // Liveness window ended
                    isLivenessActive = false;
                    livenessConfirmed = blinkHelper.BlinkOccurred();
                    Debug.WriteLine($"[DEBUG] ⏱️ Liveness window ended. BlinkOccurred={livenessConfirmed}");
                }

                Thread.Sleep(30); // ~30 fps
            }
        }

        public void ActivateLivenessCheck()
        {
            blinkHelper.Reset();
            livenessConfirmed = false;
            isLivenessActive = true;
            livenessStartTime = DateTime.Now;
            Debug.WriteLine($"[DEBUG] Liveness check ACTIVATED at {livenessStartTime:HH:mm:ss.fff}");
        }

        public void ResetLiveness()
        {
            isLivenessActive = false;
            livenessConfirmed = false;
            blinkHelper.Reset();
        }

        public bool BlinkOccurred => blinkHelper.BlinkOccurred();
        public bool IsFaceLive()
        {
            bool result = livenessConfirmed;
            Debug.WriteLine($"[DEBUG] Final Liveness Check Result = {result} (BlinkOccurred={blinkHelper.BlinkOccurred()}, LivenessActive={isLivenessActive})");
            return result;
        }

        public byte[]? CaptureImage(bool forceCapture = false)
        {
            if (!forceCapture && !IsFaceLive())
                return null;

            if (capture == null)
                return null;

            using var frame = new Mat();
            if (!capture.Read(frame) || frame.Empty())
                return null;

            using var bitmap = BitmapConverter.ToBitmap(frame);
            using var clone = CloneBitmap(bitmap);

            using var ms = new MemoryStream();
            clone.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        private static DrawingBitmap CloneBitmap(DrawingBitmap source)
        {
            return source.Clone(new DrawingRectangle(0, 0, source.Width, source.Height), source.PixelFormat);
        }

        private static Array2D<RgbPixel> LoadBitmapToDlibImage(DrawingBitmap bitmap)
        {
            var array = new Array2D<RgbPixel>(bitmap.Height, bitmap.Width);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    DrawingColor color = bitmap.GetPixel(x, y);
                    array[y][x] = new RgbPixel
                    {
                        Red = color.R,
                        Green = color.G,
                        Blue = color.B
                    };
                }
            }

            return array;
        }

        private static BitmapSource ConvertToBitmapSource(DrawingBitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            return source;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public void Dispose()
        {
            StopPreview();
            shapePredictor.Dispose();
            faceDetector.Dispose();
        }
    }
}
