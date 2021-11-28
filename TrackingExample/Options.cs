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
        
        [Option(longName:"list-cameras", HelpText = "Lists all available cameras")]
        public bool ListCameras { get; set; }
    }
}