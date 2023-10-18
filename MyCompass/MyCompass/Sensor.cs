using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyCompass
{
    public class SensorReadingPage : MvvmHelpers.BaseViewModel
    {
        public SensorReadingPage()
        {
            StopCommand = new Command(Stop);
            StartCommand = new Command(Start);
        }
        public Command StopCommand { get; }

        void Stop()
        {
            if (!Accelerometer.IsMonitoring)
                return;

            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            Accelerometer.Stop();
        }


        public Command StartCommand { get; }

        void Start()
        {
            if (Accelerometer.IsMonitoring)
                return;


            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(SensorSpeed.UI);

        }

        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {

            var data = e.Reading;

            Debug.WriteLine($"Reading: X: {data.Acceleration.X}, Y: {data.Acceleration.Y}, Z: {data.Acceleration.Z}");
        }
    }

}

