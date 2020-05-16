using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using S3ZipContent;

namespace S3ZipContentTest
{
    [TestClass]
    public class GivenNestedZipFile { 
        
        [TestMethod]
        public async Task ExtractedFilesCountShouldMatch()
        {
            var sut = new S3ZipContentHelper(TestContext.GetAmazonS3Client());

            var content = await sut.GetContents("Test", "nested.zip");

            Assert.AreEqual(content.Count, 1);
        }
    }
}