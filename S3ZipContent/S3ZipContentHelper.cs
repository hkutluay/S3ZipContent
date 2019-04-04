using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace S3ZipContent
{
    public class S3ZipContentHelper : IS3ZipContentHelper
    {
        private readonly IAmazonS3 s3;

        //https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT
        static readonly byte[] eocdHeader = new byte[] { 80, 75, 5, 6 };
        static readonly byte[] zip64EocdHeader = new byte[] { 80, 75, 6, 6 };
        static readonly byte[] zip64EocdLocatorHeader = new byte[] { 80, 75, 6, 7 };

        public S3ZipContentHelper(IAmazonS3 S3)
        {
            s3 = S3;
        }

        public async Task<IList<ZipArchiveEntry>> GetContent(string Bucket, string Key)
        {
            var metadata = await s3.GetObjectMetadataAsync(Bucket, Key);

            var length = metadata.ContentLength;

            var readLength = length > 5012 ? 5012 : length;

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = Bucket,
                Key = Key,
                ByteRange = new ByteRange(length - readLength, length)
            };

            var zipEndingResponse = s3.GetObjectAsync(request);
            var endingBytes = StreamToArray(zipEndingResponse.Result.ResponseStream);

            int pos = Search(endingBytes, eocdHeader);

            var eocdHeaderBytes = endingBytes.Skip(pos).Take(22).ToArray();

            long size = BitConverter.ToUInt32(eocdHeaderBytes.Skip(12).Take(4).ToArray(), 0);
            long start = BitConverter.ToUInt32(eocdHeaderBytes.Skip(16).Take(4).ToArray(), 0);
            int commentLength = BitConverter.ToUInt16(eocdHeaderBytes.Skip(20).Take(2).ToArray(), 0);

            if (commentLength > 0)
            {
                eocdHeaderBytes = endingBytes.Skip(pos).Take(22 + commentLength).ToArray();
            }

            byte[] zip64EocdLocatorHeaderBytes = null, zip64EocdHeaderBytes = null;

            int zip64EocdHeaderPos = Search(endingBytes, zip64EocdHeader);
            int zip64EocdLocatorHeaderPos = Search(endingBytes, zip64EocdLocatorHeader);


            if (zip64EocdHeaderPos != -1 && zip64EocdLocatorHeaderPos != -1) //zip64bit
            {

                zip64EocdHeaderBytes = endingBytes.Skip(zip64EocdHeaderPos).Take(56).ToArray();

                size = BitConverter.ToInt64(zip64EocdHeaderBytes.Skip(40).Take(8).ToArray(), 0);
                start = BitConverter.ToInt64(zip64EocdHeaderBytes.Skip(48).Take(8).ToArray(), 0);

                zip64EocdLocatorHeaderBytes = endingBytes.Skip(zip64EocdLocatorHeaderPos).Take(20).ToArray();
            }

            GetObjectRequest centralDirectoryRequest = new GetObjectRequest
            {
                BucketName = Bucket,
                Key = Key,
                ByteRange = new ByteRange(start, start + size)
            };

            var centralDirectoryResponse = s3.GetObjectAsync(centralDirectoryRequest);
            var centralDirectoryData = StreamToArray(centralDirectoryResponse.Result.ResponseStream);

            for (int i = 0; i < 4; i++)
                eocdHeaderBytes[i + 16] = 0;

            if (zip64EocdLocatorHeaderBytes != null)
            {
                for (int i = 0; i < 8; i++)
                    zip64EocdHeaderBytes[i + 48] = 0;

                byte[] offset = BitConverter.GetBytes(centralDirectoryData.LongLength);

                for (int i = 0; i < 8; i++)
                    zip64EocdLocatorHeaderBytes[i + 8] = offset[i];
            }

            if (zip64EocdHeaderBytes != null)
                centralDirectoryData = centralDirectoryData.Concat(zip64EocdHeaderBytes).ToArray();

            if (zip64EocdLocatorHeaderBytes != null)
                centralDirectoryData = centralDirectoryData.Concat(zip64EocdLocatorHeaderBytes).ToArray();
                       
            var newFile = centralDirectoryData.Concat(eocdHeaderBytes).ToArray();

            using (Stream stream = new MemoryStream(newFile))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                return archive.Entries.ToList();
        }

        private byte[] StreamToArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private int Search(byte[] src, byte[] pattern)
        {
            int c = src.Length - pattern.Length + 1;
            int j;

            for (int i = c; i > -1 ; i--)
            {
                if (src[i] != pattern[0]) continue;
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--) ;
                if (j == 0) return i;
            }

            return -1;
        }
    }
}
