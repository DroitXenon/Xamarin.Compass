using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyCompass
{
    public class MyCompassViewModel : MvvmHelpers.BaseViewModel
    {
        public MyCompassViewModel()
        {
            StopCommand = new Command(Stop);
            StartCommand = new Command(Start);
        }
        string headingDisplay;
        public string HeadingDisplay
        {
            get => headingDisplay;
            set => SetProperty(ref headingDisplay, value);
        }

        double pitch = 0;
        public double Pitch
        {
            get => pitch;
            set => SetProperty(ref pitch, value);
        }

        double roll = 0;

        public double Roll
        {
            get => roll;
            set => SetProperty(ref roll, value);
        }

        double azimut = 0;

        public double Azimut
        {
            get => azimut;
            set => SetProperty(ref azimut, value);
        }

        string pitchDisplay;
        public string PitchDisplay
        {
            get => pitchDisplay;
            set => SetProperty(ref pitchDisplay, value);
        }

        string rollDisplay;

        public string RollDisplay
        {
            get => rollDisplay;
            set => SetProperty(ref rollDisplay, value);
        }

        string azimutDisplay;

        public string AzimutDisplay
        {
            get => azimutDisplay;
            set => SetProperty(ref azimutDisplay, value);
        }

        double heading = 0;

        public double Heading
        {
            get => heading;
            set => SetProperty(ref heading, value);
        }

        public Command StopCommand { get; }

        void Stop()
        {
            if (Compass.IsMonitoring)
            {
                Compass.ReadingChanged -= Compass_ReadingChanged;
                Compass.Stop();
            }

            if (Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                Accelerometer.Stop();
            }
        }


        public Command StartCommand { get; }

        void Start()
        {
            if (Compass.IsMonitoring || Accelerometer.IsMonitoring)
                return;
   
            Compass.ReadingChanged += Compass_ReadingChanged;
            Compass.Start(SensorSpeed.UI, true);

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(SensorSpeed.UI);

        }

        double lerp(double a, double b, double t) //Linear Interpolation
        {
            return a + t * (b - a);
        }

        (double x, double y) AngleToVector(double angle)
        {
            double radian = Math.PI * angle / 180.0;
            return (Math.Cos(radian), Math.Sin(radian));
        }

        double VectorToAngle(double x, double y)
        {
            double radian = Math.Atan2(y, x);
            double angle = radian * 180.0 / Math.PI;
            if (angle < 0)
            {
                angle += 360.0;
            }
            return angle;
        }

        Queue<(double x, double y)> lastNVectors = new Queue<(double x, double y)>();
        const int N = 5;

        public delegate void AccelerometerDataChangedHandler(double x);
        public event AccelerometerDataChangedHandler AccelerometerDataChanged;
        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
           
            double newHeading = e.Reading.HeadingMagneticNorth;

            if (pitch > 90)
            {
                newHeading = (newHeading + 180) % 360;
            }

            var oldVector = AngleToVector(Heading);
            var newVector = AngleToVector(newHeading);

            double t = 0.2;
            double lerpedX = oldVector.x + t * (newVector.x - oldVector.x);
            double lerpedY = oldVector.y + t * (newVector.y - oldVector.y);

            Heading = VectorToAngle(lerpedX, lerpedY);

            HeadingDisplay = $"Heading: {e.Reading.HeadingMagneticNorth.ToString()}";
        }

        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            
            var data = e.Reading;
            Pitch = Math.Acos(data.Acceleration.Z / Math.Sqrt(Math.Pow(data.Acceleration.X, 2) + Math.Pow(data.Acceleration.Y, 2) + Math.Pow(data.Acceleration.Z, 2))) * (180.0 / Math.PI);
            Roll = Math.Atan2(-data.Acceleration.X, data.Acceleration.Z) * (180.0 / Math.PI);
            Azimut = Math.Atan2(data.Acceleration.X, data.Acceleration.Y) * (180.0 / Math.PI);
            Debug.WriteLine($"Pitch: {Pitch.ToString()}");
            Debug.WriteLine($"Roll: {Roll.ToString()}");
            Debug.WriteLine($"Azimut: {Azimut.ToString()}");
            PitchDisplay = $"Pitch: {Pitch.ToString()}";
            RollDisplay = $"Roll: {Roll.ToString()}";
            AzimutDisplay = $"Azimut: {Azimut.ToString()}";

        }
    }

}

