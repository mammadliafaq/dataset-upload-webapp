using System;
using System.Collections.Generic;

namespace UploadWebApp.Models
{
    public class CommonViewModel
    {
        public FileData FileData { get; set; }
        public IEnumerable<FileData> DataSet { get; set; }
    }
}
