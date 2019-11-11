using System;
using System.Linq;

using AzureService.Model;


namespace AzureService
{
    class CommandLineHelper
    {
        public CommandLineOptions GetOptions(string[] args)
        {
            var commandLineOptions = new CommandLineOptions
            {
                ProjectName = "",
                TaskName = "",
                TestAgents = null
            };


            var i = 0;
            foreach (var arg in args)
            {
                if (arg.ToLower().Equals("-project"))
                {
                    CheckValueMissing(i, args);
                    commandLineOptions.ProjectName = args[i + 1];
                }

                if (arg.ToLower().Equals("-createvm"))
                {
                    commandLineOptions.TaskName = "CreateVM";
                    CheckValueMissing(i, args);
                    commandLineOptions.TestAgents = args[i + 1].Split(',').ToList();
                }

                if (arg.ToLower().Equals("-deletevm"))
                {
                    commandLineOptions.TaskName = "DeleteVM";
                    CheckValueMissing(i, args);
                    commandLineOptions.TestAgents = args[i + 1].Split(',').ToList();
                }

                i++;
            }

            if (!string.IsNullOrEmpty(commandLineOptions.TaskName) && commandLineOptions.TestAgents != null && !string.IsNullOrEmpty(commandLineOptions.ProjectName))
                return commandLineOptions;

            Console.WriteLine(@"Error: missing Parameters");
            throw new Exception("Invalid Parameters");
        }

        private void CheckValueMissing(int i, string[] args)
        {
            if (i + 1 < args.Length)
            {
                if (!args[i + 1].StartsWith("-")) return;
            }

            Console.WriteLine(@"Error: Value for the argument is missing");
            throw new Exception("Invalid Parameters");
        }
    }
}