Ti SensorTag Interface Demo

A simple project for Windows 8.1 that creates a connection to the Accelerometer element of a Texas Intstruments SensorTag device:

http://www.ti.com/ww/en/wireless_connectivity/sensortag/

It shows how you can create a class that will emumerate the devices and connect to events that the SensorTag produces.

Works as a universial application on both Windows 8.1 and Windows Phone 8.1.

However, not all Windws Phone devices support Bluetooth 4.9 Low Energy (BLE) and you need to make sure that your computer has BLE support. Tested successfully on a Windows 8.1 Surface Pro 2.

For BLE to work with Windows 8.1 you have to pair the SensorTag. Use the key value 0000.

The BLE device must support HCI, note that the BlueGiga BLE112 device does not provide this, and cannot be paired with Windows 8.1.

This is not a complete solution, but hopefully it is a useful step on the road.
