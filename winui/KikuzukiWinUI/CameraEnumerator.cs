using System;
using System.Linq;
using Windows.Devices.Enumeration;

namespace Kikuzuki;

public static class CameraEnumerator
{
    public struct CameraDevice(int ID, string Name, Guid Identity = new Guid())
    {
        public int deviceID = ID;
        public string deviceName = Name;
        public Guid identifier = Identity;
    }

    public static CameraDevice[] Cameras
    {
        get
        {
            var deviceInfo = DeviceInformation.FindAllAsync(DeviceClass.VideoCapture).AsTask().Result;
            return [.. deviceInfo.Select((device, index) => new CameraDevice(index, device.Name, Guid.TryParse(device.Id, out var guid) ? guid : Guid.Empty))];
        }
    }

    public static string[] CameraNames
    {
        get
        {
            return Cameras.Select(cam => cam.deviceName).ToArray();
        }
    }

    public static string[] CameraNamesLong
    {
        get
        {
            return Cameras.Select(cam => $"[{cam.deviceID}] {cam.deviceName}: {cam.identifier}").ToArray();
        }
    }

    public static string[] CameraNamesShort
    {
        get
        {
            return Cameras.Select(cam => $"[{cam.deviceID}] {cam.deviceName}").ToArray();
        }
    }
}
