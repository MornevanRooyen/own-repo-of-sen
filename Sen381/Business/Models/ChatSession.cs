using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    // Placeholder enum for ChatStatus (replace with your real one)
    public enum ChatStatus
    {
        Open,
        Escalated,
        Resolved
    }

    public class ChatSession
    {
        // ---------- Fields ----------
        private int id;
        private int studentId;
        private int subjectId;
        private ChatStatus chatStatus;
        private int escalationId;

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

        public ChatStatus ChatStatus
        {
            get => chatStatus;
            set => chatStatus = value;
        }

        public int EscalationId
        {
            get => escalationId;
            set => escalationId = value;
        }

        // ---------- Methods ----------
        public void MarkEscalated(int escalationId)
        {
            this.escalationId = escalationId;
            chatStatus = ChatStatus.Escalated;
        }

        public void Resolve()
        {
            chatStatus = ChatStatus.Resolved;
        }
    }
}
