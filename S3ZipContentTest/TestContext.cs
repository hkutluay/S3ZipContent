using Amazon.S3;
using Amazon.S3.Model;

using Moq;

using System;
using System.IO;
using System.Net;
using System.Threading;

namespace S3ZipContentTest
{
    public static class TestContext
    {
        public static IAmazonS3 GetAmazonS3Client()
        {
            var s3ClientMock = new Mock<IAmazonS3>();

            s3ClientMock.Setup(x => x.GetObjectMetadataAsync(
                             It.IsAny<string>(),
                             It.IsAny<string>(),
                             It.IsAny<CancellationToken>()))
                           .ReturnsAsync((string bucket, string key, CancellationToken ct) =>
                           {

                               long length = 0;

                               using (var docStream = new FileInfo($"ZipFiles/{key}").OpenRead())
                               {
                                   length = docStream.Length;
                               }

                               return new GetObjectMetadataResponse
                               {
                                   HttpStatusCode = HttpStatusCode.OK,
                                   ContentLength = length
                               };
                           });

            s3ClientMock.Setup(x => x.GetObjectAsync(
                           It.IsAny<GetObjectRequest>(),
                           It.IsAny<CancellationToken>()))
                        .ReturnsAsync(
                           (GetObjectRequest request, CancellationToken ct) =>
                           {
                               var docStream = new FileInfo($"ZipFiles/{request.Key}").OpenRead();
                               var br = new BinaryReader(docStream);

                               byte[] dataArray = new byte[Convert.ToInt32(request.ByteRange.End - request.ByteRange.Start)];
                               docStream.Seek(Convert.ToInt32(request.ByteRange.Start), SeekOrigin.Begin);

                               var dataRange = br.ReadBytes(Convert.ToInt32(request.ByteRange.End - request.ByteRange.Start));

                               var ms = new MemoryStream(dataRange);

                               return new GetObjectResponse
                               {
                                   BucketName = request.BucketName,
                                   Key = request.Key,
                                   HttpStatusCode = HttpStatusCode.OK,
                                   ResponseStream = ms
                               };
                           });
            return s3ClientMock.Object;
        }
    }
}
