using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.ViewModels
{
    public class ClientLogsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string  Client { get; set; }
        public string[] ClientList { get; set; }
    }
}
