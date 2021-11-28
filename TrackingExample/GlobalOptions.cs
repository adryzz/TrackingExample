using CommandLine;

namespace TrackingExample
{
    public class GlobalOptions
    {
        [Option(longName:"quiet", shortName: 'q', SetName = "quiet", HelpText = "Shuts the fuck up.")]
        public bool Quiet { get; set; }
        
        [Option(longName:"stderr", SetName = "stderr", HelpText = "Outputs all the info on stderr instead of stdout.")]
        public bool UseStdErr { get; set; }
        
        [Option(longName:"ffmpeg-path", HelpText = "Specifies the ffmpeg path.", Default = "/usr/lib")]
        public string FFmpegPath { get; set; }
    }
}