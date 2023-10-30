using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MyEssentials;
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

        double heading = 0;

        public double Heading
        {
            get => heading;
            set => SetProperty(ref heading, value);
        }

        public Command StopCommand { get; }

        void Stop()
        {
            if (MyEssentials.Compass.IsMonitoring)
            {
                MyEssentials.Compass.ReadingChanged -= Compass_ReadingChanged;
                MyEssentials.Compass.Stop();
            }
        }

        public Command StartCommand { get; }

        void Start()
        {
            if (MyEssentials.Compass.IsMonitoring)
                return;

            MyEssentials.Compass.ReadingChanged += Compass_ReadingChanged;
            MyEssentials.Compass.Start(Xamarin.Essentials.SensorSpeed.UI, true);

        }

        void Compass_ReadingChanged(object sender, MyEssentials.CompassChangedEventArgs e)
        {
           
            Heading = e.Reading.HeadingMagneticNorth;
            HeadingDisplay = $"Heading: {e.Reading.HeadingMagneticNorth}";
        }
    }
}

