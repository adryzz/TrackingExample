using CommandLine;

namespace TrackingExample
{
    [Verb("track", true, HelpText = "Starts tracking.")]
    public class Options : GlobalOptions
    {
        [Option(longName: "camera", shortName:'c', HelpText = "Selects the camera by index, path or name. If the argument isn't present, the first camera will be used.", Default = "0")]
        public string Camera { get; set; }

        [Option(longName:"graph", shortName:'g', Required = true, HelpText = "The path of the mediapipe graph to use.")]
        public string Graph { get; set; }
        
        [Option(longName:"landmarks-output", shortName:'l', HelpText = "The path where the landmarks will be saved at")]
        public string? LandmarksPath { get; set; }
        
        [Option(longName:"processed-landmarks-output", shortName:'p', HelpText = "The path where the processed landmarks will be saved at", Default = "multi_face_landmarks")]
        public string? ProcessedLandmarksPath { get; set; }
        
        [Option(longName:"output-stream", shortName:'o', HelpText = "The path where the landmarks will be saved at", Default = "multi_face_landmarks")]
        public string OutputStream { get; set; }

        /// <summary>
        /// Ignore this
        /// </summary>
        [Option(longName:"list-cameras", HelpText = "Lists all available cameras")]
        public bool ListCameras { get; set; }
    }
}