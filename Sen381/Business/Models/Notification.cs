using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public enum NotificationType
    {
        General,
        Assignment,
        Reminder,
        Alert
    }

    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class Notification
    {
        // ---------- Fields ----------
        private int id;
        private int userId;
        private NotificationType type;
        private int subjectCode;
        private string title;
        private string body;
        private DateTime sentAt;
        private Priority priority;

        private bool isRead;
        private bool isDismissed;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int UserId
        {
            get => userId;
            set => userId = value;
        }

        public NotificationType Type
        {
            get => type;
            set => type = value;
        }

        public int SubjectCode
        {
            get => subjectCode;
            set => subjectCode = value;
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

        public DateTime SentAt
        {
            get => sentAt;
            set => sentAt = value;
        }

        public Priority Priority
        {
            get => priority;
            set => priority = value;
        }

        // Extra helpers
        public bool IsRead => isRead;
        public bool IsDismissed => isDismissed;

        // ---------- Methods ----------
        public void MarkRead()
        {
            isRead = true;
        }

        public void MarkUnread()
        {
            isRead = false;
        }

        public void Dismiss()
        {
            isDismissed = true;
        }
    }
}
