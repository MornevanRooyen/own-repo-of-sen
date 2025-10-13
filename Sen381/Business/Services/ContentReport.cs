using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    // ---------- Enumeration ----------
    public enum Reason
    {
        Spam,
        Abuse,
        OffTopic,
        Harassment,
        InappropriateContent,
        Other
    }

    // ---------- Entity ----------
    public class ContentReport
    {
        // ---------- Fields ----------
        private int id;
        private int reporterId;
        private int contentId;
        private string details;
        private Reason reportedReason;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int ReporterId
        {
            get => reporterId;
            set => reporterId = value;
        }

        public int ContentId
        {
            get => contentId;
            set => contentId = value;
        }

        public string Details
        {
            get => details;
            set => details = value;
        }

        public Reason ReportedReason
        {
            get => reportedReason;
            set => reportedReason = value;
        }

        // ---------- Methods ----------
        public void MarkActioned(int adminId)
        {
            // TODO: Record that an admin has taken action
            Console.WriteLine($"Content report {Id} actioned by Admin {adminId}.");
        }

        public void MarkDismissed(int adminId)
        {
            // TODO: Record that an admin dismissed the report
            Console.WriteLine($"Content report {Id} dismissed by Admin {adminId}.");
        }
    }
}
