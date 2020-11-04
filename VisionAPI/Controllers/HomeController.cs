using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageResizer;
using VisionAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using ImageResizer.Configuration;
using EndgameVision.Models;
using System.Net;

namespace VisionAPI.Controllers
{
    public class HomeController : Controller
    {
        private VisionAPIDbContext db = new VisionAPIDbContext();
        //[OutputCache(Duration = 60)]
        public ActionResult Index()
        {
            // Pass a list of blob URIs in ViewBag
            CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("images");
            List<BlobInfo> blobs = new List<BlobInfo>();
          

            foreach (IListBlobItem item in container.ListBlobs())
            {
                var blob = item as CloudBlockBlob;
                var image = db.Images.Where(x => x.ImageUrl == blob.Uri.AbsoluteUri).FirstOrDefault();

                

                if (blob != null && image != null)
                {

                    blobs.Add(new BlobInfo()
                    {
                        ImageUri = blob.Uri.ToString(),
                        ThumbnailUri = blob.Uri.ToString().Replace("/images/", "/thumbnails/"),
                        Description = image.Description,
                        AdultContent = image.AdultContent,
                        Category = image.Category,
                        Tags = image.Tags
                    });
                }
            }

            ViewBag.Blobs = blobs.ToArray();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Upload(Image image, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {   
                // Make sure the user selected an image file
                if (!file.ContentType.StartsWith("image"))
                {
                    TempData["Message"] = "Only image files may be uploaded";
                }
                else
                {
                    try
                    {
                        // Save the original image in the "photos" container
                        CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                        CloudBlobClient client = account.CreateCloudBlobClient();
                        CloudBlobContainer container = client.GetContainerReference("images");
                        CloudBlockBlob photo = container.GetBlockBlobReference(Path.GetFileName(file.FileName));                      
                        await photo.UploadFromStreamAsync(file.InputStream);

                        string FileAbsoluteUri = photo.Uri.AbsoluteUri;
                        image.ImageUrl = FileAbsoluteUri;
                        AnalyseImage vision = new AnalyseImage();
                        await vision.Initial(FileAbsoluteUri);

                        image.Description = vision.Description;
                        image.AdultContent = vision.Adult;
                        image.Category = vision.Categories;
                        image.Tags = vision.Tags;

                        db.Images.Add(image);
                        db.SaveChanges();
                        

                        // Generate a thumbnail and save it in the "thumbnails" container
                        using (var outputStream = new MemoryStream())
                        {
                            file.InputStream.Seek(0L, SeekOrigin.Begin);
                            var settings = new ResizeSettings { MaxWidth = 192 };
                            ImageBuilder.Current.Build(file.InputStream, outputStream, settings);
                            outputStream.Seek(0L, SeekOrigin.Begin);
                            container = client.GetContainerReference("thumbnails");
                            CloudBlockBlob thumbnail = container.GetBlockBlobReference(Path.GetFileName(file.FileName));
                            await thumbnail.UploadFromStreamAsync(outputStream);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        // In case something goes wrong
                        TempData["Message"] = ex.Message;
                    }
                }
            }

            return RedirectToAction("Index");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        // GET: Images/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);

            //Delete Blob
            
            CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("images");
            container.GetBlockBlobReference(image.ImageUrl).DeleteIfExists();
            //BlobManager BlobManagerObj = new BlobManager("picture");
            //BlobManagerObj.DeleteBlob(image.ImageUrl);

            //Delete record from Db
            db.Images.Remove(image);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}