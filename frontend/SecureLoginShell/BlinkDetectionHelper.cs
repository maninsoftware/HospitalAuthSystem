using DlibDotNet;
using System;
using System.Diagnostics;
using System.Linq;

namespace HospitalLoginApp.Helpers
{


    public class BlinkDetectionHelper
    {
        private readonly int[] leftEyeIdx = { 36, 37, 38, 39, 40, 41 };
        private readonly int[] rightEyeIdx = { 42, 43, 44, 45, 46, 47 };

        private const double EAR_THRESHOLD = 0.23;
        private const int MIN_BLINK_DURATION_MS = 80;
        private const int MAX_BLINK_DURATION_MS = 1000;

        private bool isEyeClosed = false;
        private bool blinkOccurred = false;
        private bool wasEyeClosedLastFrame = false;

        private DateTime blinkStartTime;
        public void Reset()
        {
            blinkOccurred = false;
            isEyeClosed = false;
            wasEyeClosedLastFrame = false;
        }
        public void ProcessLandmarks(Point[] landmarks)
        {
            var leftEye = leftEyeIdx.Select(i => landmarks[i]).ToArray();
            var rightEye = rightEyeIdx.Select(i => landmarks[i]).ToArray();

            double leftEAR = ComputeEAR(leftEye);
            double rightEAR = ComputeEAR(rightEye);
            double avgEAR = (leftEAR + rightEAR) / 2.0;

           // Debug.WriteLine($"[DEBUG] EAR: Left={leftEAR:F4}, Right={rightEAR:F4}, Avg={avgEAR:F4}");

            if (avgEAR < EAR_THRESHOLD && !wasEyeClosedLastFrame)
            {
                wasEyeClosedLastFrame = true;
                blinkStartTime = DateTime.Now;
                Debug.WriteLine($"[DEBUG] 👁️ Eye closed at {blinkStartTime:HH:mm:ss.fff}");
            }
            else if (avgEAR >= EAR_THRESHOLD && wasEyeClosedLastFrame)
            {
                wasEyeClosedLastFrame = false;
                var duration = (DateTime.Now - blinkStartTime).TotalMilliseconds;

                if (duration >= MIN_BLINK_DURATION_MS && duration <= MAX_BLINK_DURATION_MS)
                {
                    blinkOccurred = true;
                    Debug.WriteLine($"[DEBUG] ✅ Blink Detected (EAR drop and rise). Duration={duration:F0}ms");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] ❌ Blink too short or too long. Duration={duration:F0}ms");
                }
            }
        }

        private double ComputeEAR(Point[] eye)
        {
            double A = Distance(eye[1], eye[5]);
            double B = Distance(eye[2], eye[4]);
            double C = Distance(eye[0], eye[3]);
            double ear = (A + B) / (2.0 * C);
            //Debug.WriteLine($"[DEBUG] EAR calculation: A={A:F4}, B={B:F4}, C={C:F4}, EAR={ear:F4}");
            return ear;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public bool BlinkOccurred() => blinkOccurred;

       
    }
}
