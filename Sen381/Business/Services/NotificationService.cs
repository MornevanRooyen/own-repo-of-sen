using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class NotificationService
    {
        // Notify when a new topic is created
        public void NotifyNewTopic(int topicId)
        {
            // TODO: look up followers/subscribers of the topic/forum and send notifications
            Console.WriteLine($"[NotificationService] New topic created. TopicId={topicId}");
        }

        // Notify when an existing topic is updated (new reply, edit, etc.)
        public void NotifyTopicUpdate(int topicId)
        {
            // TODO: notify followers/subscribers of this topic
            Console.WriteLine($"[NotificationService] Topic updated. TopicId={topicId}");
        }

        // Notify when a tutor responds
        public void NotifyTutorResponse(int tutorId)
        {
            // TODO: notify the relevant student(s) subscribed to/engaged with this tutor
            Console.WriteLine($"[NotificationService] Tutor responded. TutorId={tutorId}");
        }

        // Notify when an FAQ is promoted or highlighted
        public void NotifyFAQPromotion(int faqId)
        {
            // TODO: broadcast to interested users (module subscribers, etc.)
            Console.WriteLine($"[NotificationService] FAQ promoted. FaqId={faqId}");
        }
    }
}
