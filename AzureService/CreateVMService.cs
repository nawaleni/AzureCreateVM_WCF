using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AzureService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class CreateVMService : ICreateVMService
    {
        public string CreateTestAgents(string[] args)
        {

            //var commandLineHelper = new CommandLineHelper();
            SetupVM setupVM = new SetupVM();
            setupVM.Main(args);

            //var commandLineOptions = commandLineHelper.GetOptions(args);

            return "Task Completed";
        }

      
    }
}
