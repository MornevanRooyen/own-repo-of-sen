using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class TutorMatcher
    {
        // Finds eligible tutors for a subject, excluding the requesting tutor if needed
        // UML: FindEligible(int subjectId, int tutorId): List<string>
        public List<string> FindEligible(int subjectId, int tutorId)
        {
            // TODO: Implement eligibility logic
            // Example: query tutors who are active, approved for the subject, and not the requesting tutor

            Console.WriteLine($"Finding eligible tutors for Subject {subjectId}, excluding Tutor {tutorId}");

            // Placeholder return
            return new List<string>
            {
                "TutorA",
                "TutorB",
                "TutorC"
            };
        }
    }
}
