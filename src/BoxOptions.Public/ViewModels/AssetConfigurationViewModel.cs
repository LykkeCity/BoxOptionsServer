using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.ViewModels
{
    public class AssetConfigurationViewModel
    {
        public string SaveInformation { get; set; }
        public List<BoxSizeViewModel> BoxConfiguration { get; set; }
    }
}
