using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    // ---------- Enumeration ----------
    public enum EscalationStatus
    {
        Pending,
        Notified,
        Acknowledged,
        Expired,
        Resolved
    }

    // ---------- Entity ----------
    public class TutorEscalation
    {
        // ---------- Fields ----------
        private int id;
        private int sessionId;
        private int studentId;
        private int acknowledgedTutorId;
        private List<int> notifiedTutors = new List<int>();
        private EscalationStatus escalationStatus;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int SessionId
        {
            get => sessionId;
            set => sessionId = value;
        }

        public int StudentId
        {
            get => studentId;
            set => studentId = value;
        }

        public int AcknowledgedTutorId
        {
            get => acknowledgedTutorId;
            set => acknowledgedTutorId = value;
        }

        public List<int> NotifiedTutors
        {
            get => notifiedTutors;
            set => notifiedTutors = value ?? new List<int>();
        }

        public EscalationStatus EscalationStatus
        {
            get => escalationStatus;
            set => escalationStatus = value;
        }

        // ---------- Methods ----------
        public void MarkAcknowledged(int tutorId)
        {
            AcknowledgedTutorId = tutorId;
            EscalationStatus = EscalationStatus.Acknowledged;
            Console.WriteLine($"Escalation acknowledged by Tutor {tutorId}.");
        }

        public void MarkResolved()
        {
            EscalationStatus = EscalationStatus.Resolved;
            Console.WriteLine("Escalation resolved.");
        }

        public void Expire()
        {
            EscalationStatus = EscalationStatus.Expired;
            Console.WriteLine("Escalation expired.");
        }
    }
}
