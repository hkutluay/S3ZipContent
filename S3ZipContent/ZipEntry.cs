using System;
using System.Collections.Generic;
using System.Text;

namespace S3ZipContent
{
    public class ZipEntry
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTimeOffset LastWriteTime { get; set; }
    }
}
