using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Transfer;
using System.IO;
using System.Threading;

namespace IDSAuthority.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        S3Functions S3F = new S3Functions();
        [Route("/CheckAuthed")]
        public IActionResult Index()
        {
            User.FindFirst("sub").Value.ToString();
            if ("sub" != "")
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(401);
            }
            return StatusCode(500);

        }
        [HttpPost("upload", Name = "upload")]
        [Route("/UploadUserFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadUserFile(
             [FromForm(Name = "upload")] IFormFile file,
             CancellationToken cancellationToken)
        {
            try
            {
                User.FindFirst("sub").Value.ToString();
                if (CheckIfUserFile(file))
                {
                    await S3F.UploadFileS3(file);
                }
                else
                {
                    return BadRequest(new { message = "Invalid file extension" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(401);
            }
        }
        private bool CheckIfUserFile(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".txt"); // Change the extension based on your need
        }
    }

    public class S3Functions
    {
        public async Task UploadFileS3(IFormFile file)
        {
            var credentials = new BasicAWSCredentials("Redacted", "Redacted");
            //Console.WriteLine("credentials written");
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest1
            };
            using var client = new AmazonS3Client(credentials, config);
            await using var newMemoryStream = new MemoryStream();
            file.CopyTo(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = file.FileName,
                BucketName = "Redacted",
                CannedACL = S3CannedACL.BucketOwnerRead
            };

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
    }
}
