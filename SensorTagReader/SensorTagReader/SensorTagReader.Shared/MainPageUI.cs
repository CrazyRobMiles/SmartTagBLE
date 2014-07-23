using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using BLEDevices;
using Windows.UI.Popups;

namespace SensorTagReader
{
    public  partial class MainPage : Page
    {

        SensorTag tag = null;

        async void setupDevices()
        {
            if (tag == null)
            {
                tag = new SensorTag();

                // Show a list of all the tags you can find

                List<String> names = await SensorTag.FindAllTagNames();

                DevicesListBox.ItemsSource = names;
            }
        }

        async void displayMessage(string message)
        {
            MessageDialog mess = new MessageDialog(message);
            await mess.ShowAsync();
        }



        async private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sensorName = "";

                if (DevicesListBox.SelectedItem != null)
                    sensorName = DevicesListBox.SelectedItem as string;
                
                if (await tag.BindToAccel(sensorName) != SensorTag.BindState.StartedOK)
                    return;

                tag.GotNewAccelerometer += new NewAccelerometerReading(DisplayAccelerometerReading);
            }
            catch (Exception bindException)
            {
                displayMessage(e.ToString());
            }
        }

        public void DisplayAccelerometerReading(AccelReading a)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                XAccelTextBlock.Text = "X: " + a.X.ToString();
                YAccelTextBlock.Text = "Y: " + a.Y.ToString();
                ZAccelTextBlock.Text = "Z: " + a.Z.ToString();
                TimeTextBlock.Text = a.Time.ToString();
            });
        }
    }
}
