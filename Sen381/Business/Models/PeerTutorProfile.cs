using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    // Placeholder for availability slots (replace with real implementation if needed)
    public class AvailabilitySlot
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class PeerTutorProfile : User
    {
        // ---------- Fields ----------
        private int tutorId;
        private bool isActive;
        private int completedSessions;
        private List<int> approvedSubjects = new List<int>();
        private List<AvailabilitySlot> availability = new List<AvailabilitySlot>();

        // ---------- Properties ----------
        public int TutorId
        {
            get => tutorId;
            set => tutorId = value;
        }

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        public List<int> ApprovedSubjects
        {
            get => approvedSubjects;
            set => approvedSubjects = value ?? new List<int>();
        }

        // UML shows MinRequiredGrade
        public int MinRequiredGrade { get; set; }

        public int CompletedSessions
        {
            get => completedSessions;
            set => completedSessions = value;
        }

        public List<AvailabilitySlot> Availability
        {
            get => availability;
            set => availability = value ?? new List<AvailabilitySlot>();
        }

        // ---------- Methods ----------
        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void CreateSession()
        {
            Console.WriteLine("New tutoring session created.");
        }

        public void EndSession()
        {
            CompletedSessions++;
            Console.WriteLine("Tutoring session ended. Total completed: " + CompletedSessions);
        }

        public bool CanTutor()
        {
            return IsActive && ApprovedSubjects.Count > 0;
        }

        public void NotifyTutorEscalation(int alertId)
        {
            Console.WriteLine($"Tutor escalation alert sent. AlertId: {alertId}");
        }

        public void NotifyStudentAcknowledge(int alertId)
        {
            Console.WriteLine($"Student notified of tutor acknowledgement. AlertId: {alertId}");
        }
    }
}
