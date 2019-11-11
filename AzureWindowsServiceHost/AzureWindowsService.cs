using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace AzureWindowsServiceHost
{
    public partial class AzureWindowsService : ServiceBase
    {
        ServiceHost host;
        
        public AzureWindowsService()
        {
            InitializeComponent();
            ServiceName = "Azure.Service";
        }

        //public static void Main()
        //{
        //    ServiceBase.Run(new AzureWindowsService());
        //}
        protected override void OnStart(string[] args)
        {
            host = new ServiceHost(typeof(AzureService.CreateVMService));
            host.Open();
        }

        protected override void OnStop()
        {
            host.Close();
        }
    }
}
