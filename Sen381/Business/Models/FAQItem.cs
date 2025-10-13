using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class FAQItem
    {
        // ---------- Fields ----------
        private int id;
        private int subjectId;
        private string question;
        private string answer;
        private bool isActive;
        private DateTime updatedAt;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int SubjectId
        {
            get => subjectId;
            set => subjectId = value;
        }

        public string Question
        {
            get => question;
            set => question = value;
        }

        public string Answer
        {
            get => answer;
            set => answer = value;
        }

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        public DateTime UpdatedAt
        {
            get => updatedAt;
            set => updatedAt = value;
        }

        // ---------- Methods ----------
        public void UpdateAnswer(string newAnswer)
        {
            Answer = newAnswer;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
