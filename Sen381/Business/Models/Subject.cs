using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class Subject
    {
        // ---------- Fields ----------
        private int id;
        private string subjectCode;
        private string name;
        private string description;
        private int year;
        private bool isActive;

        private List<int> prerequisiteSubjects = new List<int>();

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public string SubjectCode
        {
            get => subjectCode;
            set => subjectCode = value;
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

        public int Year
        {
            get => year;
            set => year = value;
        }

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        public List<int> PrerequisiteSubjects
        {
            get => prerequisiteSubjects;
            set => prerequisiteSubjects = value ?? new List<int>();
        }

        // ---------- Methods ----------
        public void AddPrerequisite(int subjectId)
        {
            if (!prerequisiteSubjects.Contains(subjectId))
            {
                prerequisiteSubjects.Add(subjectId);
            }
        }

        public void RemovePrerequisite(int subjectId)
        {
            prerequisiteSubjects.Remove(subjectId);
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
