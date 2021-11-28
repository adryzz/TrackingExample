using System;
using SeeShark;

namespace TrackingExample
{
    public static class Utils
    {
        public static void ListCameras()
        {
            try
            {
                CameraManager manager = new CameraManager();
                for (int i = 0; i < manager.Devices.Count; i++)
                {
                    CameraInfo cam = manager.Devices[i];
                    Console.WriteLine($"#{i}: '{cam.Name}' {cam.Path}");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
        
        public 
    }
}