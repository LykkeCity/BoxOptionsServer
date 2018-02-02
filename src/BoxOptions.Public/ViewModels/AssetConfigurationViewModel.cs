using BoxOptions.Public.Models;
using System.Collections.Generic;

namespace BoxOptions.Public.ViewModels
{
    public class AssetConfigurationViewModel
    {
        public string SaveInformation { get; set; }
        public List<BoxSizeModel> BoxConfiguration { get; set; }
    }
}
