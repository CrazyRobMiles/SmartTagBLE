using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BLEDevices
{

    public struct AccelReading
    {
        public byte X;
        public byte Y;
        public byte Z;

        public DateTimeOffset Time;

        public AccelReading(byte x, byte y, byte z, DateTimeOffset time)
        {
            X = x;
            Y = y;
            Z = z;
            Time = time;
        }
    }


    public delegate void NewAccelerometerReading(AccelReading a);

    public class SensorTag
    {

        async static Task<string> GetDeviceName(DeviceInformation device)
        {
            // Get the GATT (Generic Attribute profile) service from the device
            // We can ask this to tell use the characteristics that the device supports
            GattDeviceService service = await GattDeviceService.FromIdAsync(device.Id);

            // No GATT service, give up on this device
            if (service == null)
                return "";

            // Obtain the characteristic we want to interact with  - 0x2A00 is the device name
            // https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicViewer.aspx?u=org.bluetooth.characteristic.gap.device_name.xml

            // This returns a vector of characteristics, pull off the one at the base 
            GattCharacteristic characteristic = service.GetCharacteristics(GattCharacteristic.ConvertShortIdToUuid(0x2A00))[0];

            //Use the characteristic to read the name bytes from the device
            byte[] deviceNameBytes = (await characteristic.ReadValueAsync()).Value.ToArray();

            //Convert to string  
            string deviceName = Encoding.UTF8.GetString(deviceNameBytes, 0, deviceNameBytes.Length);

            return deviceName;
        }

        async static public Task<List<String>> FindAllTagNames()
        {
            List<string> result = new List<string>();

            // Get all the devices that expose generic access
            DeviceInformationCollection devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.GenericAccess));

            // Return an empty list if there are no devices
            if (devices.Count == 0)
                return result;

            foreach (DeviceInformation device in devices)
            {
                // Get the name of the device
                string deviceName = await GetDeviceName(device);
                // put the name in the list
                result.Add(deviceName);
            }

            return result;
        }

        async static public Task<List<String>> FindAllTagIDs()
        {
            List<string> result = new List<string>();

            // Get all the devices that expose generic access
            DeviceInformationCollection devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.GenericAccess));

            // Return an empty list if there are no devices
            if (devices.Count == 0)
                return result;

            foreach (DeviceInformation device in devices)
            {
                // put the ID in the list
                result.Add(device.Id);
            }

            return result;
        }

        async static public Task<String> FindDeviceIdForName(string target)
        {
            string result = null;

            // Get all the devices that expose generic access
            DeviceInformationCollection devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.GenericAccess));

            // Return an empty list if there are no devices
            if (devices.Count == 0)
                return result;

            // If the target string is empty return the id of the first device found
            if (target.Length == 0)
                return devices[0].Id;

            // Search for the matchin name. 
            foreach (DeviceInformation device in devices)
            {
                string name = await GetDeviceName(device);
                if (name.ToLower() == target.ToLower())
                    return device.Id;
            }

            return result;
        }

        public NewAccelerometerReading GotNewAccelerometer;

        public enum BindState
        {
            NoDevicesFound,
            DeviceNameNotFound,
            DeviceHasNoAccelerometer,
            StartedOK
        }

        /// <summary>
        /// Binds to the given BLE accelerometer device.
        /// </summary>
        /// <param name="deviceName">Name of the device to use. If blank use the first one you find.</param>
        /// <returns>Status of the bind request</returns>
        async public Task<BindState> BindToAccel(string deviceName)
        {
            // Got a device with that name - now try to bind to the accelerometer
            
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(new Guid("F000AA10-0451-4000-B000-000000000000")));

            if (devices.Count == 0)
                return BindState.NoDevicesFound;

            foreach (var device in devices)
            {
                if (device.Name.ToLower() != deviceName.ToLower() && deviceName.Length>0)
                    continue;

                //Connect to the service  
                var accService = await GattDeviceService.FromIdAsync(device.Id);

                if (accService == null)
                    return BindState.DeviceHasNoAccelerometer;

                //Get the accelerometer data characteristic  
                var accData = accService.GetCharacteristics(new Guid("F000AA11-0451-4000-B000-000000000000"))[0];
                //Subcribe value changed  
                accData.ValueChanged += accData_ValueChanged;
                //Set configuration to notify  
                await accData.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                //Get the accelerometer configuration characteristic  
                var accConfig = accService.GetCharacteristics(new Guid("F000AA12-0451-4000-B000-000000000000"))[0];
                //Write 1 to start accelerometer sensor  
                await accConfig.WriteValueAsync((new byte[] { 1 }).AsBuffer());
                return BindState.StartedOK;

            }
            return BindState.DeviceNameNotFound;
        }

        async void accData_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var values = (await sender.ReadValueAsync()).Value.ToArray();
            var x = values[0];
            var y = values[1];
            var z = values[2];

            if (GotNewAccelerometer != null)
            {
                AccelReading r = new AccelReading(x, y, z, args.Timestamp);
                GotNewAccelerometer(r);
            }
        }
    }
}
