using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class Forum
    {
        // ---------- Fields ----------
        private int id;
        private string name;
        private string description;
        private int subjectCode;
        private DateTime updatedAt;

        private List<int> pinnedTopics = new List<int>();
        private List<int> topicCount = new List<int>();

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public int SubjectCode
        {
            get => subjectCode;
            set => subjectCode = value;
        }

        public DateTime UpdatedAt
        {
            get => updatedAt;
            set => updatedAt = value;
        }

        public List<int> PinnedTopics
        {
            get => pinnedTopics;
            set => pinnedTopics = value ?? new List<int>();
        }

        public List<int> TopicCount
        {
            get => topicCount;
            set => topicCount = value ?? new List<int>();
        }

        // ---------- Methods ----------
        public Topic CreateTopic(int creatorId, string title, string body)
        {
            var topic = new Topic
            {
                Id = new Random().Next(1000, 9999), // Placeholder ID generator
                Title = title,
                Body = body,
                SubjectId = subjectCode,
                CreatedAt = DateTime.Now,
                LastActivityAt = DateTime.Now,
                CreatedBy = creatorId.ToString()
            };

            topicCount.Add(topic.Id);
            updatedAt = DateTime.Now;

            return topic;
        }

        public void Archive()
        {
            Console.WriteLine("Forum archived.");
        }

        public void Unarchive()
        {
            Console.WriteLine("Forum unarchived.");
        }

        public void AddModerator(int id)
        {
            Console.WriteLine($"Moderator {id} added to forum.");
        }

        public void RemoveModerator(int id)
        {
            Console.WriteLine($"Moderator {id} removed from forum.");
        }

        public bool PinTopic(int topicId)
        {
            if (!pinnedTopics.Contains(topicId))
            {
                pinnedTopics.Add(topicId);
                return true;
            }
            return false;
        }

        public bool LockTopic(int topicId)
        {
            Console.WriteLine($"Topic {topicId} locked.");
            return true;
        }

        public void MoveTopic(int topicId, int locationId)
        {
            Console.WriteLine($"Topic {topicId} moved to location {locationId}.");
        }
    }
}
