using System;
using System.IO;
using SeeShark;

namespace TrackingExample
{
    public static class CameraUtils
    {
        public static void ListCameras()
        {
            try
            {
                CameraManager manager = new CameraManager();
                for (int i = 0; i < manager.Devices.Count; i++)
                {
                    CameraInfo cam = manager.Devices[i];
                    Console.WriteLine($"#{i}: '{cam}");
                }
                manager.Dispose();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
        
        /// <summary>
        /// Tries parsing the camera by index, path or name.
        /// </summary>
        /// <remarks>
        /// Parsing a camera by name is not recommended as the first camera with the name will be returned.
        /// </remarks>
        /// <param name="text">Either the index, the path or the name of an installed camera.</param>
        /// <returns><see cref="Camera"/> if a camera was found, <see langword="null"/> otherwise.</returns>
        public static Camera? ParseCamera(string text)
        {
            CameraManager manager = new CameraManager();
            Camera? camera = null;
            //try by index first
            if (int.TryParse(text, out int index))
            {
                if (manager.Devices.Count > index && index >= 0)
                {
                    camera = manager.GetCamera(manager.Devices[index]);
                    manager.Dispose();
                    return camera;
                }
            }
            
            //try by path
            if (File.Exists(text))
            {
                try
                {
                    camera = manager.GetCamera(text);
                    manager.Dispose();
                    return camera;
                }
                catch (InvalidOperationException)
                {
                }
            }
            
            //try by name
            foreach (CameraInfo info in manager.Devices)
            {
                if (info.Name == text)
                    camera = manager.GetCamera(info);
                    manager.Dispose();
                    return camera;
            }

            manager.Dispose();
            return null;
        }
    }
}