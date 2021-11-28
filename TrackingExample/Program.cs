using System;
using System.IO;
using System.Linq;
using CommandLine;
using FFmpeg.AutoGen;
using SeeShark;
using SeeShark.FFmpeg;

namespace TrackingExample
{
    public static class Program
    {
        public static Camera Camera;

        public static string Graph;
        
        public static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options, ListCamerasOptions>(args);
            
            if (result.Errors.Any())
                return;

            //set global options and initialize ffmpeg
            SetGlobalOptions((GlobalOptions)result.Value);

            if (result.Value is ListCamerasOptions)
            {
                CameraUtils.ListCameras();
                return;
            }

            Console.CancelKeyPress += (object? _, ConsoleCancelEventArgs _) => OnExit();
            
            //set global options like graph path and camera
            SetOptions((Options)result.Value);
            
            OnExit();
        }

        static void SetOptions(Options options)
        {
            if (options.ListCameras)
            {
                Console.WriteLine("Can't list cameras and track at the same time.");
                return;
            }
            
            Camera? camera = CameraUtils.ParseCamera(options.Camera);

            if (camera == null)
            {
                Console.WriteLine("Couldn't find the specified camera.");
                return;
            }

            Camera = camera;

            if (!File.Exists(options.Graph))
            {
                Console.WriteLine("The specified graph does not exist.");
                OnExit();
            }
        }
        
        static void SetGlobalOptions(GlobalOptions goptions)
        {
            FFmpegManager.SetupFFmpeg(goptions.FFmpegPath);
            
            if (goptions.Quiet)
            {
                Console.SetError(TextWriter.Null);
                Console.SetOut(TextWriter.Null);
            }
            
            if (goptions.UseStdErr)
            {
                Console.SetOut(Console.Error);
            }
        }
        
        static void OnExit()
        {
            Camera?.Dispose();
        }
    }
}