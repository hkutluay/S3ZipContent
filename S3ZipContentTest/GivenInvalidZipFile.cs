using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using S3ZipContent;

namespace S3ZipContentTest
{
    [TestClass]
    public class GivenInvalidZipFile
    {
        [TestMethod]
        [ExpectedException(typeof(FileIsNotaZipException))]
        public async Task ShouldThrowExceptionForEmptyZipFile()
        {
            var s3ZipContentHelper = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            await s3ZipContentHelper.GetContents("Test", "zero-file.zip");
        }


        [TestMethod]
        [ExpectedException(typeof(FileIsNotaZipException))]
        public async Task ShouldThrowExceptionForNonZipFile()
        {
            var s3ZipContentHelper = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            await s3ZipContentHelper.GetContents("Test", "not-a-zip.zip");
        }

        [TestMethod]
        [ExpectedException(typeof(FileIsNotaZipException))]
        public async Task ShouldThroExceptionForZeroByteZipFile()
        {
            var s3ZipContentHelper = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            await s3ZipContentHelper.GetContents("Test", "zero-byte.zip");
        }
    }
}