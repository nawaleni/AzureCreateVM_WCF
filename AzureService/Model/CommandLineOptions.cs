using System.Collections.Generic;


namespace AzureService.Model
{
    class CommandLineOptions
    {
        public string ProjectName { get; set; }
        public string TaskName { get; set; }
        public List<string> TestAgents { get; set; }
    }
}