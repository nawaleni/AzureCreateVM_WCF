using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureService.Model
{
    public class Configurations
    {
        public string ResourceGroup { get; set; }
        public string OSDisksnapshot { get; set; }
        public string DataDisk1Snapshot { get; set; }
        public string DataDisk2Snapshot { get; set; }
        public string VirtualNetwork { get; set; }
        public string SubnetMask { get; set; }
        public string NetworkInterface { get; set; }
        public string BaseUriVM { get; set; }
        public string BaseUriDisks { get; set; }

    }
}

