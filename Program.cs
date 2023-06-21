using System;
using System.Collections.Generic;
using System.Management; // need to add System.Management to your project references.
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        
        bool isRunning = true;
        List<string> devices = new List<string>();
        List<string> initDevices = new List<string>();

        var regexString = @"[\\](.+?)[\\]";

        initDevices = GetUSBIDs();

        while (isRunning) {

            var usbDevices = GetUSBDevices();

            // Collect
            foreach (var usbDevice in usbDevices)
            {
                if (!initDevices.Contains(usbDevice.DeviceID) && !devices.Contains(usbDevice.DeviceID))
                {
                    Match id = Regex.Match(usbDevice.DeviceID.ToString(), regexString);
                    
                    if (id.Success)
                    {

                        // Parse ID further
                        string val = id.Value.Replace("&", "\n").Replace("\\", "").Replace('_', ':');
                        devices.Add(usbDevice.DeviceID);
                        Console.WriteLine($"New Device! -> \n{val}\n");
                    }else
                    {
                        Console.WriteLine($"Failed regex on device: \n{usbDevice.DeviceID}\n");
                    }
                }
            }


            Thread.Sleep(1000); // Sleep one second
        

        }
    }

    static List<string> GetUSBIDs()
    {
        var usbDevices = GetUSBDevices();
        var ids = new List<string>();
        // Collect
        foreach (var usbDevice in usbDevices)
        {
            ids.Add(usbDevice.DeviceID);
        }
        return ids;
    }

    static List<USBDeviceInfo> GetUSBDevices()
    {
        List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

        using var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub");
        using ManagementObjectCollection collection = searcher.Get();

        foreach (var device in collection)
        {
            devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
        }
        return devices;
    }
}

class USBDeviceInfo
{
    public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
    {
        this.DeviceID = deviceID;
        this.PnpDeviceID = pnpDeviceID;
        this.Description = description;
    }
    public string DeviceID { get; private set; }
    public string PnpDeviceID { get; private set; }
    public string Description { get; private set; }
}