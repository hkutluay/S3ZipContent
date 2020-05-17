using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using S3ZipContent;

namespace S3ZipContentTest
{
    [TestClass]
    public class GivenValidZipFile
    {
        [TestMethod]
        public async Task ExtractedFileCountShouldMatch()
        {
            var sut = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            var content = await sut.GetContents("Test", "foo.zip");

            Assert.AreEqual(content.Count, 1);
        }

        [TestMethod]
        public async Task ShouldHaveExpectedFileName()
        {
            var sut = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            var content = await sut.GetContents("Test", "foo.zip");

            Assert.AreEqual(content[0].FullName, "foo.txt");
        }
    }
}