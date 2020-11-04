using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionAPI.Models
{
    public class BlobInfo
    {
        public string ImageUri { get; set; }
        public string ThumbnailUri { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string AdultContent { get; set; }


    }
}