using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using AzureService.Model;
using System.Xml.Linq;

namespace AzureService
{
    public class SetupVM
    {
        public static Region region = Region.USEast;

        public void Main(string[] args)
        {
            var commandLineHelper = new CommandLineHelper();
            var commandLineOptions = commandLineHelper.GetOptions(args);

            if (commandLineOptions.TaskName.ToLower().Equals("createvm"))
            {
                ConfigureResources(commandLineOptions.ProjectName, commandLineOptions.TestAgents);
            }

            if (commandLineOptions.TaskName.ToLower().Equals("deletevm"))
            {
                DeleteResources(commandLineOptions.ProjectName, commandLineOptions.TestAgents);
            }
        }

        private static void DeleteResources(string projectName, List<string> testAgents)
        {
            Configurations configurations = GetProjectConfigurations(projectName);
            try
            {
                AzureCredentials credentials = SdkContext.AzureCredentialsFactory
                    .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

                IAzure azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

                foreach (var testAgent in testAgents)
                {
                    string testAgentFullName = configurations.BaseUriVM + testAgent;
                    var testAgentname = azure.VirtualMachines.GetById(testAgentFullName);
                    if (testAgentname != null)
                    {
                        azure.VirtualMachines.DeleteById(testAgentFullName);
                        Console.WriteLine("Deleted VM: " + testAgent);
                        //delete the disks
                        Console.WriteLine("Deleting disks of VM: " + testAgent);
                        azure.Disks.DeleteById(configurations.BaseUriDisks + testAgent + "_OSDisk");
                        azure.Disks.DeleteById(configurations.BaseUriDisks + testAgent + "_DataDisk1");
                        azure.Disks.DeleteById(configurations.BaseUriDisks + testAgent + "_DataDisk2");
                        Console.WriteLine("Deleted disks of VM: " + testAgent);
                    }
                    else
                    {
                        Console.WriteLine("VM: " + testAgent + " does not exists. Deleting other resources");
                        Console.WriteLine("Deleting disks of VM: " + testAgent);
                        azure.Disks.DeleteById(configurations.BaseUriDisks + testAgent + "_OSDisk");
                        azure.Disks.DeleteById(configurations.BaseUriDisks + testAgent + "_DataDisk1");
                        azure.Disks.DeleteById(configurations.BaseUriDisks + testAgent + "_DataDisk2");
                        Console.WriteLine("Deleted disks of VM: " + testAgent);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void ConfigureResources(string projectName, List<string> testAgents)
        {
            Configurations configurations = GetProjectConfigurations(projectName);
            try
            {
                AzureCredentials credentials = SdkContext.AzureCredentialsFactory
                    .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

                IAzure azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();


                foreach (var testAgent in testAgents)
                {
                    Console.WriteLine("Creating Disks for VM: " + testAgent);

                    var osDiskName = testAgent + "_OSDisk";
                    var osDisk = azure.Disks.Define(osDiskName)
                        .WithRegion(region)
                        .WithExistingResourceGroup(configurations.ResourceGroup)
                        .WithWindowsFromSnapshot(configurations.OSDisksnapshot)
                        .WithSizeInGB(127)
                        .WithSku(DiskSkuTypes.PremiumLRS)
                        .Create();
                    Console.WriteLine("OS Disk created for VM: " + testAgent);

                    var dataDisk1Name = testAgent + "_DataDisk1";
                    var dataDisk1 = azure.Disks.Define(dataDisk1Name)
                        .WithRegion(region)
                        .WithExistingResourceGroup(configurations.ResourceGroup)
                        .WithData()
                        .FromSnapshot(configurations.DataDisk1Snapshot)
                        .WithSku(DiskSkuTypes.PremiumLRS)
                        .Create();


                    var dataDisk2Name = testAgent + "_DataDisk2";
                    var dataDisk2 = azure.Disks.Define(dataDisk2Name)
                        .WithRegion(region)
                        .WithExistingResourceGroup(configurations.ResourceGroup)
                        .WithData()
                        .FromSnapshot(configurations.DataDisk2Snapshot)
                        .WithSku(DiskSkuTypes.PremiumLRS)
                        .Create();
                    Console.WriteLine("Data disks created for VM: " + testAgent);

                    var networkInterface = CreateNic(azure, testAgent, configurations);

                    var virtualMachine = azure.VirtualMachines.Define(testAgent)
                        .WithRegion(region)
                        .WithExistingResourceGroup(configurations.ResourceGroup)
                        .WithExistingPrimaryNetworkInterface(networkInterface)
                        .WithSpecializedOSDisk(osDisk, OperatingSystemTypes.Windows)
                        .WithExistingDataDisk(dataDisk1, 0, CachingTypes.ReadWrite)
                        .WithExistingDataDisk(dataDisk2, 1, CachingTypes.ReadWrite)
                        .WithSize(VirtualMachineSizeTypes.StandardD8sV3)
                        .Create();
                    Console.WriteLine("Created VM: " + testAgent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public static INetworkInterface CreateNic(IAzure azure, string vmName, Configurations configurations)
        {
            var fullnamebase = configurations.NetworkInterface;


            //e.g TestAgent99_nic

            var res = azure.NetworkInterfaces.GetById(fullnamebase + vmName + "_nic");


            if (res == null)
            {
                var publicIpAddressName = vmName + "_ip";
                var publicIpAddress = azure.PublicIPAddresses.Define(publicIpAddressName)
                    .WithRegion(region)
                    .WithExistingResourceGroup(configurations.ResourceGroup)
                    .WithStaticIP()
                    .Create();

                INetwork vNetwork = azure.Networks.GetById(configurations.VirtualNetwork);
                var networkInterfaceName = vmName + "_nic";
                var networkInterface = azure.NetworkInterfaces.Define(networkInterfaceName)
                    .WithRegion(region)
                    .WithExistingResourceGroup(configurations.ResourceGroup)
                    .WithExistingPrimaryNetwork(vNetwork)
                    .WithSubnet(configurations.SubnetMask)
                    .WithPrimaryPrivateIPAddressDynamic()
                    .WithExistingPrimaryPublicIPAddress(publicIpAddress)
                    .Create();

                return networkInterface;
            }

            return res;
        }

        private static Configurations GetProjectConfigurations(string projectName)
        {
            string path = Environment.CurrentDirectory;

            XDocument xmlDocument = XDocument.Load(@"..\..\ConfigurationSetting.xml");
            Configurations configurations = new Configurations();

            foreach (var node in xmlDocument.Descendants())
            {
                if (node.Name.ToString() == projectName + "ResourceGroupName")
                    configurations.ResourceGroup = node.Value.ToString();
                if (node.Name.ToString() == projectName + "OSDisksnapshotName")
                    configurations.OSDisksnapshot = node.Value.ToString();
                if (node.Name.ToString() == projectName + "DataDisk1SnapshotName")
                    configurations.DataDisk1Snapshot = node.Value.ToString();
                if (node.Name.ToString() == projectName + "DataDisk2SnapshotName")
                    configurations.DataDisk2Snapshot = node.Value.ToString();
                if (node.Name.ToString() == projectName + "NetworkInterface")
                    configurations.NetworkInterface = node.Value.ToString();
                if (node.Name.ToString() == projectName + "SubnetMask")
                    configurations.SubnetMask = node.Value.ToString();
                if (node.Name.ToString() == projectName + "VirtualNetwork")
                    configurations.VirtualNetwork = node.Value.ToString();
                if (node.Name.ToString() == projectName + "BaseUriVM")
                    configurations.BaseUriVM = node.Value.ToString();
                if (node.Name.ToString() == projectName + "BaseUriDisks")
                    configurations.BaseUriDisks = node.Value.ToString();
            }
            

            return configurations;
        }
    }
}
