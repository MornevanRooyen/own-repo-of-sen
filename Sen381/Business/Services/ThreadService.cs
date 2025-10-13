using Sen381.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class ThreadService
    {
        // Creates a thread from a HelpRequest
        public PrivateThread CreateFromHelpRequest()
        {
            // TODO: Fetch HelpRequest and map it to a PrivateThread
            var thread = new PrivateThread();
            thread.Open();
            Console.WriteLine("PrivateThread created from HelpRequest.");
            return thread;
        }

        // Creates a thread directly with student, tutor, and subject
        public PrivateThread Create(int studentId, int tutorId, int subjectId)
        {
            // TODO: Persist to DB, link subject if needed
            var thread = new PrivateThread
            {
                StudentId = studentId,
                TutorId = tutorId
            };
            thread.Open();

            Console.WriteLine($"PrivateThread created for Student {studentId} and Tutor {tutorId} on Subject {subjectId}.");
            return thread;
        }

        // Sends a message within a thread
        public PrivateMessage SendMessage(int threadId, int senderUserId, string content)
        {
            // TODO: Attach message to thread, persist to DB
            var message = new PrivateMessage
            {
                ThreadId = threadId,
                SenderUserId = senderUserId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            Console.WriteLine($"Message sent in Thread {threadId} by User {senderUserId}.");
            return message;
        }

        // Closes a thread
        public void CloseThread(int threadId)
        {
            // TODO: Lookup thread and close it
            Console.WriteLine($"Thread {threadId} closed.");
        }

        // Reopens a closed thread
        public void ReopenThread(int threadId)
        {
            // TODO: Lookup thread and reopen it
            Console.WriteLine($"Thread {threadId} reopened.");
        }
    }

    // Simple placeholder for messages (your UML already has ChatMessage/PrivateMessage elsewhere)
    public class PrivateMessage
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int SenderUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
