using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    // ---------- Enumeration ----------
    public enum HelpRequestStatus
    {
        Open,
        Assigned,
        Closed
    }

    // ---------- Entity ----------
    public class HelpRequest
    {
        // ---------- Fields ----------
        private int id;
        private int studentId;
        private int subjectId;
        private int matchedTutorId;
        private HelpRequestStatus helpRequestStatus;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int StudentId
        {
            get => studentId;
            set => studentId = value;
        }

        public int SubjectId
        {
            get => subjectId;
            set => subjectId = value;
        }

        public int MatchedTutorId
        {
            get => matchedTutorId;
            set => matchedTutorId = value;
        }

        public HelpRequestStatus HelpRequestStatus
        {
            get => helpRequestStatus;
            set => helpRequestStatus = value;
        }

        // ---------- Methods ----------
        public void AssignTutor(int tutorId)
        {
            MatchedTutorId = tutorId;
            HelpRequestStatus = HelpRequestStatus.Assigned;
            Console.WriteLine($"Tutor {tutorId} assigned to HelpRequest {Id}.");
        }

        public void Close()
        {
            HelpRequestStatus = HelpRequestStatus.Closed;
            Console.WriteLine($"HelpRequest {Id} closed.");
        }
    }
}
