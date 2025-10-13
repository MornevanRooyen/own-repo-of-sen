using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sen381.Business.Models
{
   public class Student : User
    {
        private string studentNo;
        private string major;
        private int year;

        private StudentPreferences preferences;
        private PeerTutorProfile tutorProfile;

        private List<Subject> subscribedSubjects;
        private List<int> subscribedTutors; // TutorID
        private List<int> subscribedTopics; // TopicID

        private List<int> followingTopicsIds; // Explicit property per UML

        // Properties 
        public string StudentNo
        {
            get => studentNo;
            set => studentNo = value;
        }

        public string Major
        {
            get => major;
            set => major = value;
        }

        public int Year
        {
            get => year;
            set => year = value;
        }

        public StudentPreferences Preferences
        {
            get => preferences;
            set => preferences = value;
        }

        public PeerTutorProfile TutorProfile
        {
            get => tutorProfile;
            set => tutorProfile = value;
        }

        public List<Subject> SubscribedSubjects
        {
            get => subscribedSubjects;
            set => subscribedSubjects = value;
        }

        public List<int> SubscribedTutors
        {
            get => subscribedTutors;
            set => subscribedTutors = value;
        }

        public List<int> SubscribedTopics
        {
            get => subscribedTopics;
            set => subscribedTopics = value;
        }

        public List<int> FollowingTopicIds
        {
            get => followingTopicsIds;
            set => followingTopicsIds = value;
        }

        // ---- Methods ----

        public void UpdateDetails() 
        {

        }

        public void CreateTopic() 
        {

        }

        public void FollowTopic() 
        {

        }

        public void UnfollowTopic() 
        {

        }

        public void ApplyToBeTutor() 
        {

        }

        public void SubscribeSubject() 
        {

        }

        public void UnsubSubject() 
        {

        }

        public void SubscribeTutor() 
        {

        }

        public void UnsubscribeTutor() 
        {

        }


        // ---- Placeholder -----
        public class StudentPreferences 
        {

        }
        public class PeerTutorProfile 
        { 

        }
        public class Subject
        {

        }
    }
}
