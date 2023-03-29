
using System;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Microsoft.Extensions.Primitives;

namespace aws_bucket.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AwsController : ControllerBase
    {
        public AmazonS3Client _s3Client;
        public string _bucketName;
        public readonly RegionEndpoint _region;

        public AwsController()
        {

            string accesKey = "AKIA5N5WQKZMRFLXPL7Y";
            string secretKey = "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";
            _bucketName = "atlantisbucket";
            _region = RegionEndpoint.EUWest2;
            _s3Client = new AmazonS3Client(accesKey, secretKey, _region);
        }

        [HttpPost]
        [Route("DownloadFile")]
        public async Task<bool> DownloadFile([FromForm] string fileName)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
            };     // Issue request and remember to dispose of the response
            using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
            try
            {
                // Save object to local file
                await response.WriteResponseStreamToFileAsync($"C:\\Users\\1\\OneDrive\\Рабочий стол\\Aws\\{fileName}", true, System.Threading.CancellationToken.None);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error saving {fileName}: {ex.Message}");
                return false;
            }
        }
        [HttpPost]
        [Route("DeleteFile")]
        public async Task<bool> DeleteFile([FromForm]  string file)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = file
                };
                _s3Client.DeleteObjectAsync(deleteObjectRequest);
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }
        [HttpPost]
        [Route("UploadFile")]
        public async Task<bool> UploadFile([FromForm] string fileupload)
        {
            

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileupload,
                FilePath = Path.GetFileName(fileupload),
            };
            var response = await _s3Client.PutObjectAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            
        }

        [HttpGet]
        [Route("GetFiles")]
        public async Task<List<AwsModel>> ListBucketContentsAsync()
        {
            List<AwsModel>objects= new List<AwsModel>();
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    MaxKeys = 5,
                }; 
               
               var response = new ListObjectsV2Response(); do
                {
                    response = await _s3Client.ListObjectsV2Async(request); response.S3Objects
                    .ForEach(obj => objects.Add(new AwsModel() { Name=obj.Key,ModificationDate= obj.LastModified , Size= obj.Size }));             // If the response is truncated, set the request ContinuationToken
                                                                                                                                            // from the NextContinuationToken property of the response.
                    request.ContinuationToken = response.NextContinuationToken;
                }
                while (response.IsTruncated); return objects;
            }
            catch (AmazonS3Exception ex)
            {
              
                return null;
            }




        }
        [HttpPost]
        [Route("Recognize")]
        public  string Recognize([FromForm] string pic)
        {
            StringBuilder stringBuilder= new StringBuilder();
            string accesKey = "AKIA5N5WQKZMRFLXPL7Y";
            string secretKey = "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";
            string fileInfo = "";

            //AmazonS3Config config = new AmazonS3Config();
            //config.ServiceURL = "";

            String photo = pic;
          

            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient(
            accesKey,
                    secretKey,
                    Amazon.RegionEndpoint.EUWest2
                    );
            DetectTextRequest detectTextRequest = new DetectTextRequest()
            {
                Image = new Amazon.Rekognition.Model.Image()
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object()
                    {
                        Name = photo,
                        Bucket = _bucketName
                    }
                }
            };

            try
            {
                DetectTextResponse detectTextResponse = rekognitionClient.DetectTextAsync(detectTextRequest).GetAwaiter().GetResult();
               
                foreach (TextDetection text in detectTextResponse.TextDetections)
                {
                    stringBuilder.Append(text.DetectedText);
                    
                }

                return stringBuilder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Console.WriteLine(fileInfo);
            return fileInfo;
        }

        [HttpPost]
        [Route("RecognizePhoto")]
        public string RecognizePhoto([FromForm] string picture)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string accesKey = "AKIA5N5WQKZMRFLXPL7Y";
            string secretKey = "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";
          
            //AmazonS3Config config = new AmazonS3Config();
            //config.ServiceURL = "";

            String photo = picture;


            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient(
            accesKey,
                    secretKey,
                    Amazon.RegionEndpoint.EUWest2
                    );
            DetectLabelsRequest detectlabelsRequest = new DetectLabelsRequest()
            {
                Image = new Amazon.Rekognition.Model.Image()
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object()
                    {
                        Name = photo,
                        Bucket = _bucketName
                    },
                },
                MaxLabels = 10,
                MinConfidence = 75F
            };
            string result = "";
            try
            {
                DetectLabelsResponse detectLabelsResponse = rekognitionClient.DetectLabelsAsync(detectlabelsRequest).GetAwaiter().GetResult();
              
                foreach (Label label in detectLabelsResponse.Labels)
                {
                   
                    result += label.Name + " : " + label.Confidence + "\n";
                }

               

            }
            catch (Exception e)
            {
                result=e.Message;
            }
            return result;
        }

        [HttpPost]
        [Route("RecognizeFace")]
        public string RecognizeFace([FromForm] string pictures)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string accesKey = "AKIA5N5WQKZMRFLXPL7Y";
            string secretKey = "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";

            //AmazonS3Config config = new AmazonS3Config();
            //config.ServiceURL = "";

            String photo = pictures;


            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient(
            accesKey,
                    secretKey,
                    Amazon.RegionEndpoint.EUWest2
                    );
            DetectFacesRequest detectFaceRequest = new DetectFacesRequest()
            {
                Image = new Amazon.Rekognition.Model.Image()
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object()
                    {
                        Name = photo,
                        Bucket = _bucketName
                    }
                }
            };

            try {
                DetectFacesResponse detectFacesResponse = rekognitionClient.DetectFacesAsync(detectFaceRequest).GetAwaiter().GetResult();
                bool hasAll = detectFaceRequest.Attributes.Contains("ALL");
               

               // Console.WriteLine("Найдено лиц: " + detectFacesResponse.FaceDetails.Count);

                // Загружаем изображение в объект Bitmap
                var bitmap = new Bitmap(pictures);

                // Создаем объект Graphics из объекта Bitmap
                var graphics = Graphics.FromImage(bitmap);

                // Создаем красный квадрат и кисть для обводки лица
                var pen = new Pen(Color.Red, 3);

                // Обходим все лица, найденные сервисом Rekognition
                foreach (var faceDetail in detectFacesResponse.FaceDetails)
                {
                    // Получаем координаты лица
                    var boundingBox = faceDetail.BoundingBox;
                    var left = (int)(bitmap.Width * boundingBox.Left);
                    var top = (int)(bitmap.Height * boundingBox.Top);
                    var width = (int)(bitmap.Width * boundingBox.Width);
                    var height = (int)(bitmap.Height * boundingBox.Height);

                    // Рисуем красный квадрат вокруг лица
                    graphics.DrawRectangle(pen, left, top, width, height);
                }

                // Сохраняем изображение с обведенными красными квадратами
                var outputFilename = Path.GetFileNameWithoutExtension(pictures) + "_output.png";
                bitmap.Save(outputFilename);
               
              
                return outputFilename;

            } 
            
            catch (Exception e) { stringBuilder.Append(e.Message); return null; }
        }

    }
}