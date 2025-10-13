using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class StudentTopicFollow
    {
        // ---------- Fields ----------
        private int studentNo;
        private int topicId;
        private DateTime createdAt;
        private DateTime mutedUntil;

        // ---------- Properties ----------
        public int StudentNo
        {
            get => studentNo;
            set => studentNo = value;
        }

        public int TopicId
        {
            get => topicId;
            set => topicId = value;
        }

        public DateTime CreatedAt
        {
            get => createdAt;
            set => createdAt = value;
        }

        public DateTime MutedUntil
        {
            get => mutedUntil;
            set => mutedUntil = value;
        }

        // ---------- Constructor ----------
        public StudentTopicFollow(int studentNo, int topicId)
        {
            this.studentNo = studentNo;
            this.topicId = topicId;
            createdAt = DateTime.Now;
            mutedUntil = DateTime.MinValue; // default: not muted
        }

        // ---------- Methods ----------
        public void Mute(TimeSpan duration)
        {
            mutedUntil = DateTime.Now.Add(duration);
        }

        public void Unmute()
        {
            mutedUntil = DateTime.MinValue;
        }
    }
}
