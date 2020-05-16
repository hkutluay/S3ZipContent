using System.Collections.Generic;
using System.Threading.Tasks;

namespace S3ZipContent
{
    public interface IS3ZipContentHelper
    {
        Task<IList<ZipEntry>> GetContents(string bucket, string key);
    }
}
