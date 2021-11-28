using CommandLine;

namespace TrackingExample
{
    [Verb("--list-cameras", HelpText = "Lists all the available cameras.")]
    public class ListCamerasOptions : GlobalOptions
    {
        
    }
}