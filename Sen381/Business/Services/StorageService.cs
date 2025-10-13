using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class StorageService
    {
        // Uploads a file and returns a Resource object
        //public Resource UploadFile(int uploaderId, string fileName, byte[] fileData)
        //{
        //    // TODO: implement file upload logic
        //    // Example: save file to disk/cloud, create Resource record in DB, etc.

        //    //var resource = new Resource
        //    //{
        //    //    UploaderId = uploaderId,
        //    //    FileName = fileName,
        //    //    FileSize = fileData.Length,
        //    //    CreatedAt = DateTime.UtcNow
        //    //};

        //    Console.WriteLine($"File '{fileName}' uploaded by User {uploaderId}.");

        //   // return resource;
        //}

        // Generates a download URL for a stored resource
        public string GenerateDownloadUrl(int resourceId)
        {
            // TODO: implement real URL generation (e.g., signed URL, API endpoint, etc.)
            string url = $"https://example.com/download/{resourceId}";
            Console.WriteLine($"Generated download URL for Resource {resourceId}: {url}");
            return url;
        }

        // Deletes a stored resource
        public void DeleteResource(int resourceId)
        {
            // TODO: implement deletion logic (e.g., remove from storage & DB)
            Console.WriteLine($"Resource {resourceId} deleted.");
        }
    }
}
