using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    // ---------- Enumeration ----------
    public enum ThreadStatus
    {
        Pending,
        Matched,
        Closed
    }

    // ---------- Entity ----------
    public class PrivateThread
    {
        // ---------- Fields ----------
        private int id;
        private int studentId;
        private int tutorId;
        private ThreadStatus threadStatus;

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

        public int TutorId
        {
            get => tutorId;
            set => tutorId = value;
        }

        public ThreadStatus ThreadStatus
        {
            get => threadStatus;
            set => threadStatus = value;
        }

        // ---------- Methods ----------
        public void Open()
        {
            ThreadStatus = ThreadStatus.Pending;
            Console.WriteLine($"PrivateThread {Id} opened (Pending).");
        }

        public void Close()
        {
            ThreadStatus = ThreadStatus.Closed;
            Console.WriteLine($"PrivateThread {Id} closed.");
        }

        public void Reopen()
        {
            if (ThreadStatus == ThreadStatus.Closed)
            {
                ThreadStatus = ThreadStatus.Pending;
                Console.WriteLine($"PrivateThread {Id} reopened (Pending).");
            }
        }
    }
}
