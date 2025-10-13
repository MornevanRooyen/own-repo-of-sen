using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class Resource
    {
        // ---------- Fields ----------
        private int id;
        private int subjectId;
        private int uploadUserId;
        private string title;
        private string description;
        private double sizeBytes;
        private string storageKey;
        private int folderId;
        private string url;
        private string thumbNailUrl;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int SubjectId
        {
            get => subjectId;
            set => subjectId = value;
        }

        public int UploadUserId
        {
            get => uploadUserId;
            set => uploadUserId = value;
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public double SizeBytes
        {
            get => sizeBytes;
            set => sizeBytes = value;
        }

        public string StorageKey
        {
            get => storageKey;
            set => storageKey = value;
        }

        public int FolderId
        {
            get => folderId;
            set => folderId = value;
        }

        public string Url
        {
            get => url;
            set => url = value;
        }

        public string ThumbNailUrl
        {
            get => thumbNailUrl;
            set => thumbNailUrl = value;
        }

        // ---------- Methods ----------
        public void Rename(string newTitle)
        {
            Title = newTitle;
        }

        public void UpdateDescription(string text)
        {
            Description = text;
        }

        public void MoveToFolder(int newFolderId)
        {
            FolderId = newFolderId;
        }

        public void StartPreview()
        {
            Console.WriteLine($"Preview started for resource: {Title}");
        }

        public string GetDownloadUrl()
        {
            // Could be a signed URL in real scenario
            return Url;
        }

        public string GetPreviewUrl()
        {
            // Could be a different CDN endpoint for preview
            return ThumbNailUrl ?? Url;
        }
    }
}
