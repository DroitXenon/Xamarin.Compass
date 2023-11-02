using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace MyCompass
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Compass.ReadingChanged += Compass_ReadingChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Compass.Start(SensorSpeed.UI);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Compass.Stop();
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            // Assuming HeadingMagneticNorth is in degrees
            headingLabel.Text = $"Heading: {e.Reading.HeadingMagneticNorth}°";
        }
    }
}
