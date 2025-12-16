using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;

namespace Kikuzuki;

public static class CameraEnumerator
{
    public static DeviceInformationCollection Cameras
    {
        get
        {
            var deviceInfo = DeviceInformation.FindAllAsync(MediaDevice.GetVideoCaptureSelector()).AsTask().Result;
            return deviceInfo;
        }
    }
}
