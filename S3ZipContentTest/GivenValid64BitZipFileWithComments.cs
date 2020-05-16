using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using S3ZipContent;

namespace S3ZipContentTest
{
    [TestClass]
    public class GivenValid64BitZipFileWithComments
    {

        [TestMethod]
        public async Task ExtractedFilesCountShouldMatch()
        {
            var sut = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            var content = await sut.GetContents("Test", "foo64.zip");

            Assert.AreEqual(content.Count, 1);
        }

        [TestMethod]
        public async Task ShouldHaveExpectedFileName()
        {
            var sut = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            var content = await sut.GetContents("Test", "foo64.zip");

            Assert.AreEqual(content[0].FullName, "Documents/foo.txt");
        }
    }
}