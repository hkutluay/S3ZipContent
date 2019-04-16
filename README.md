# S3ZipContent
Lists zip file content on AWS S3 without downloading whole document. Supports both zip and zip64 files.


# Usage

First install S3ZipContent via NuGet console:
```
PM> Install-Package S3ZipContent
```

Sample usage:
```csharp
IAmazonS3 s3 = new AmazonS3Client();

IS3ZipContentHelper content = new S3ZipContentHelper(s3);
var contentList = await content.GetContent("Bucket", "Key");

foreach (var content in contentList)
   Console.WriteLine(item.FullName);
 ```
