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
using UnmanageUtility;

namespace TrackingExample
{
    public static class Program
    {
        const string kInputStream = "input_video";
        const string kOutputStream = "output_video";
        
        static Camera Camera;

        static string GraphPath;

        static Stream StdOut;

        static CalculatorGraph? Graph;

        static OutputStreamPoller<ImageFrame>? Poller;

        static string? LandmarksOutputPath;

        static string OutputStream;
        
        static FrameConverter? Converter;

        static int PacketTimestamp = 0;
        
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

            Graph.StartRun().AssertOk();
            
            Camera.StartCapture();
            
            for (;;)
            {
                if (Poller != null)
                {
                    var packet = new ImageFramePacket();
                    if (!Poller.Next(packet))
                        break;
                    var frame = packet.Get();
                    byte[] buffer = frame.CopyToByteBuffer(frame.WidthStep() * frame.Height());
                    StdOut.Write(buffer, 0, buffer.Length);
                }
            }
            
            OnExit();
        }

        private static void InitializeMediapipe()
        {
            string graphText = File.ReadAllText(GraphPath);
            Glog.Initialize("stuff", "stuff");
            Graph = new CalculatorGraph(graphText);
            
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
            var frame = e.Frame;
            if (Converter == null)
            {
                Converter = new FrameConverter(frame, PixelFormat.Rgba);
            }

            if (e.Status != DecodeStatus.NewFrame)
            {
                return;
            }
            
            Frame cFrame = Converter.Convert(frame);
            var pixelData = new UnmanagedArray<byte>(cFrame.RawData);

            var inputFrame = new ImageFrame(ImageFormat.Format.Srgba, cFrame.Width, cFrame.Height, cFrame.WidthStep,
                pixelData);
            
            var inputPacket = new ImageFramePacket(inputFrame, new Timestamp(PacketTimestamp++));

            Graph?.AddPacketToInputStream(kInputStream, inputPacket).AssertOk();
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
            Graph?.Dispose();
            Poller?.Dispose();
            Converter?.Dispose();
        }
    }
}