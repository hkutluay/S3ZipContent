using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using S3ZipContent;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace S3ZipContentTest
{
    [TestClass]
    public class S3ContentHelperTest
    {

        private Mock<IAmazonS3> s3ClientMock;
        private IS3ZipContentHelper s3ZipContentHelper;

        public S3ContentHelperTest()
        {
            //  var docStream = new FileInfo("ZipFiles/foo.zip").OpenRead();
            //   var br = new BinaryReader(docStream);

            s3ClientMock = new Mock<IAmazonS3>();

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


        }

        [TestMethod]
        public async Task LengthTest()
        {
            var zipHelper = new S3ZipContentHelper(s3ClientMock.Object);

            var content = await zipHelper.GetContent("Test", "foo.zip");

            Assert.AreEqual(content.Count, 1);

        }

        [TestMethod]
        public async Task LengthTest64Bit()
        {
            var zipHelper = new S3ZipContentHelper(s3ClientMock.Object);

            var content = await zipHelper.GetContent("Test", "foo64.zip");

            Assert.AreEqual(content.Count, 1);

        }


        [TestMethod]
        public async Task ContentTest()
        {
            var zipHelper = new S3ZipContentHelper(s3ClientMock.Object);

            var content = await zipHelper.GetContent("Test", "foo.zip");

            Assert.AreEqual(content[0].FullName, "foo.txt");

        }

        [TestMethod]
        public async Task ContentTest64Bit()
        {
            var zipHelper = new S3ZipContentHelper(s3ClientMock.Object);

            var content = await zipHelper.GetContent("Test", "foo64.zip");

            Assert.AreEqual(content[0].FullName, "Documents");

        }
    }
}
