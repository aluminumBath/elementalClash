namespace Elementborn.Core
{
    /// <summary>
    /// Pure geometry for the VR "check your wrist" admin gesture: the player is looking roughly toward their left
    /// wrist AND their right hand is touching it. Takes plain component floats so it stays UnityEngine-free and
    /// unit-tested; the runtime feeds it XR device poses each frame.
    /// </summary>
    public static class WristGesture
    {
        public static bool IsActivated(
            float headX, float headY, float headZ,
            float forwardX, float forwardY, float forwardZ,
            float wristX, float wristY, float wristZ,
            float rightX, float rightY, float rightZ,
            float maxGazeAngleDegrees, float touchDistance)
        {
            // Right hand must be touching the wrist.
            double tx = rightX - wristX, ty = rightY - wristY, tz = rightZ - wristZ;
            if (tx * tx + ty * ty + tz * tz > (double)touchDistance * touchDistance) return false;

            // Head must be gazing toward the wrist (angle between forward and head->wrist).
            double gx = wristX - headX, gy = wristY - headY, gz = wristZ - headZ;
            double gLen = System.Math.Sqrt(gx * gx + gy * gy + gz * gz);
            double fLen = System.Math.Sqrt((double)forwardX * forwardX + forwardY * forwardY + forwardZ * forwardZ);
            if (gLen < 1e-6 || fLen < 1e-6) return false;

            double cos = (gx * forwardX + gy * forwardY + gz * forwardZ) / (gLen * fLen);
            if (cos > 1d) cos = 1d; else if (cos < -1d) cos = -1d;
            double angle = System.Math.Acos(cos) * (180d / System.Math.PI);
            return angle <= maxGazeAngleDegrees;
        }
    }
}
