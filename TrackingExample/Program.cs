using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Akihabara.External;
using Akihabara.Framework;
using Akihabara.Framework.ImageFormat;
using Akihabara.Framework.Packet;
using Akihabara.Framework.Port;
using Akihabara.Framework.Protobuf;
using CommandLine;
using FFmpeg.AutoGen;
using SeeShark;
using SeeShark.FFmpeg;

namespace TrackingExample
{
    public static class Program
    {
        const string kInputStream = "input_video";
        const string kOutputStream = "output_video";
        
        static Camera Camera;

        static string GraphPath;

        static Stream StdOut;

        static CalculatorGraph Graph;

        static OutputStreamPoller<ImageFrame> Poller;

        static string? LandmarksOutputPath;

        static string OutputStream;
        
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

            Camera.NewFrameHandler += OnFrame;

            InitializeMediapipe();
            
            OnExit();
        }

        private static void InitializeMediapipe()
        {
            string GraphText = File.ReadAllText(GraphPath);
            Glog.Initialize("stuff", "stuff");
            Graph = new CalculatorGraph(GraphText);
            
            Poller = Graph.AddOutputStreamPoller<ImageFrame>(kOutputStream).Value();
            
            var jserOptions = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            Graph.ObserveOutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(OutputStream, (packet) => {
                var timestamp = packet.Timestamp().Value();
                Glog.Log(Glog.Severity.Info, $"Got landmarks at timestamp {timestamp}");

                var landmarks = packet.Get();

                if (LandmarksOutputPath != null)
                {
                    var jsonLandmarks = JsonSerializer.Serialize(landmarks, jserOptions);
                    File.WriteAllText(LandmarksOutputPath, jsonLandmarks);
                }

                return Status.Ok();
            }, out var callbackHandle).AssertOk();
        }

        private static void OnFrame(object? sender, FrameEventArgs e)
        {
            
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

            GraphPath = options.Graph;

            LandmarksOutputPath = options.LandmarksPath;

            OutputStream = options.OutputStream;

            StdOut = new BufferedStream(Console.OpenStandardOutput());
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
            StdOut?.Dispose();
        }
    }
}