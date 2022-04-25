
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenner.Comum
{
    public class ReverseProxySettings
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string PathBase { get; set; }
        public bool IsConfigured => !string.IsNullOrEmpty(Scheme) || !string.IsNullOrEmpty(Host) || !string.IsNullOrEmpty(PathBase);
    }
}
