using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.ViewModels
{
    public class BoxSizeViewModel : IBoxSize
    {
        [Required(ErrorMessage = "Asset Pair is required")]        
        public string AssetPair { get; set; }

        [Required(ErrorMessage = "Time To First Box is required")]
        [Range(1000,60000,ErrorMessage ="Must be a value between 1000 and 60000")]
        public double TimeToFirstBox { get; set; }

        [Required(ErrorMessage = "Box Time Size is required")]
        [Range(1000, 10000, ErrorMessage = "Must be a value between 1000 and 10000")]
        public double BoxHeight { get; set; }

        [Required(ErrorMessage = "Box Price Size is required")]
        [Range(0, 1, ErrorMessage = "Must be a value between 0 and 1")]
        public double BoxWidth { get; set; }

        [Required(ErrorMessage = "Boxes/Row is required")]
        [Range(0, 20, ErrorMessage = "Must be a value between 0 and 20")]
        public int BoxesPerRow { get; set; }

        [Required(ErrorMessage = "Save History is required")]
        public bool SaveHistory { get; set; }

        [Required(ErrorMessage = "Allowed In Game is required")]
        public bool GameAllowed { get; set; } 
    }
}
