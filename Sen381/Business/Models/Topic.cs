using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class Topic
    {
        // ---------- Fields ----------
        private int id;
        private string createdBy;
        private int subjectId;
        private string title;
        private string body;
        private bool isPinned;
        private bool isLocked;
        private DateTime lastActivityAt;
        private int replyCount;
        private int upvoteCount;

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

        public string CreatedBy
        {
            get => createdBy;
            set => createdBy = value;
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Body
        {
            get => body;
            set => body = value;
        }

        public bool IsPinned
        {
            get => isPinned;
            set => isPinned = value;
        }

        public bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }

        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt
        {
            get => lastActivityAt;
            set => lastActivityAt = value;
        }

        public int ReplyCount
        {
            get => replyCount;
            set => replyCount = value;
        }

        public int UpvoteCount
        {
            get => upvoteCount;
            set => upvoteCount = value;
        }

        // ---------- Methods ----------
        public void Edit() { }

        public void Pin(int id) { }

        public void Lock() { }

        public void Unlock() { }

        public void RegisterReply() { }

        public void RegisterUpvote() { }

        public void BroadCastCreated() { }

        public void BroadCastUpdated() { }
    }
}
