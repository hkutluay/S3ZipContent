using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;

namespace S3ZipContent
{
    public interface IS3ZipContentHelper
    {
        Task<IList<ZipEntry>> GetContents(string bucket, string key);
    }
}
