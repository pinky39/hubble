using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hubble.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            new HubbleServer(args).Run();
        }
    }
}
