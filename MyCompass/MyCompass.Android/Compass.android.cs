using System;
using System.Diagnostics;
using Android.Hardware;
using Android.Runtime;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(MyCompass.Droid.Compass))]
namespace MyCompass.Droid
{
    public static partial class Compass
    {

        static SensorListener listener;
        static Sensor magnetometer;
        static Sensor accelerometer;

        internal static void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
        {
            var delay = SensorDelay.Ui;
            accelerometer = Platform.SensorManager.GetDefaultSensor(SensorType.Accelerometer);
            magnetometer = Platform.SensorManager.GetDefaultSensor(SensorType.MagneticField);
            listener = new SensorListener(accelerometer.Name, magnetometer.Name, delay, applyLowPassFilter);
            Platform.SensorManager.RegisterListener(listener, accelerometer, delay);
            Platform.SensorManager.RegisterListener(listener, magnetometer, delay);
        }

        internal static void PlatformStop()
        {
            if (listener == null)
                return;

            Platform.SensorManager.UnregisterListener(listener, accelerometer);
            Platform.SensorManager.UnregisterListener(listener, magnetometer);
            listener.Dispose();
            listener = null;
        }
    }

    class SensorListener : Java.Lang.Object, ISensorEventListener, IDisposable
    {
        float[] lastAccelerometer = new float[3];
        float[] lastMagnetometer = new float[3];
        bool lastAccelerometerSet;
        bool lastMagnetometerSet;
        float[] r = new float[9];
        float[] orientation = new float[3];

        string magnetometer;
        string accelerometer;
        bool applyLowPassFilter;

        internal SensorListener(string accelerometer, string magnetometer, SensorDelay delay, bool applyLowPassFilter)
        {
            this.magnetometer = magnetometer;
            this.accelerometer = accelerometer;
            this.applyLowPassFilter = applyLowPassFilter;
        }

        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        static float[] MultiplyMatrices(float[] a, float[] b)
        {
            var result = new float[9];
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    result[(i * 3) + j] = 0;
                    for (var k = 0; k < 3; k++)
                    {
                        result[(i * 3) + j] += a[(i * 3) + k] * b[(k * 3) + j];
                    }
                }
            }
            return result;
        }

        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Name == accelerometer && !lastAccelerometerSet)
            {
                // var modifiedValues = new float[3];
                // e.Values.CopyTo(modifiedValues, 0);
                // modifiedValues[2] = Math.Abs(modifiedValues[2]);
                // modifiedValues.CopyTo(lastAccelerometer, 0);
                // Debug.WriteLine($"accelerometer: {e.Values[0]} {e.Values[1]} {e.Values[2]}\n");
                e.Values.CopyTo(lastAccelerometer, 0);
                lastAccelerometerSet = true;
            }
            else if (e.Sensor.Name == magnetometer && !lastMagnetometerSet)
            {
                e.Values.CopyTo(lastMagnetometer, 0);
                lastMagnetometerSet = true;
            }

            if (lastAccelerometerSet && lastMagnetometerSet)
            {
                SensorManager.GetRotationMatrix(r, null, lastAccelerometer, lastMagnetometer);

                float[] p = { 1, 0, 0, 0, 0, 1, 0, -1, 0 };

                // float[] rotate = { 1, 0, 0, 0, (float)(Math.Sqrt(2) / 2), (float)(-Math.Sqrt(2) / 2), 0, (float)(Math.Sqrt(2) / 2), (float)(Math.Sqrt(2) / 2) };

                float[] rotate = { 1, 0, 0, 0, (float)Math.Cos(15 * Math.PI / 180), -(float)Math.Sin(15 * Math.PI / 180), 0, (float)Math.Sin(15 * Math.PI / 180), (float)Math.Cos(15 * Math.PI / 180) };
                var swapR = MultiplyMatrices(r, p);
                var rotateR = MultiplyMatrices(swapR, rotate);

                Console.WriteLine("Changed r");
                for (var i = 0; i < 9; i += 3)
                {
                    for (var j = 0; j < 3; ++j)
                    {
                        Console.Write(rotateR[i + j] + " ");
                    }
                    Console.WriteLine("\n");
                }

                SensorManager.GetOrientation(rotateR, orientation);

                if (orientation.Length <= 0)
                    return;

                var azimuthInRadians = orientation[0];

                var azimuthInDegress = (Java.Lang.Math.ToDegrees(azimuthInRadians) + 360.0) % 360.0;
                // azimuthInDegress = 0;

                var data = new CompassData(azimuthInDegress);
                Compass.OnChanged(data);
                lastMagnetometerSet = false;
                lastAccelerometerSet = false;
            }
        }
    }
}
