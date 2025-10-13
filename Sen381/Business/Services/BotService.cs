using Sen381.Business.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class BotService
    {
        // Handle a user asking something in a session
        // UML: Ask(int sessionId, string messageText): BotResponse
        public BotResponse Ask(int sessionId, string messageText)
        {
            // TODO: NLP or FAQ lookup logic here
            Console.WriteLine($"Bot received message in session {sessionId}: {messageText}");

            return new BotResponse(messageText, 0.85, 1); // sample response
        }

        // Suggests closest FAQ given a question
        // UML: SuggestClosestFaq(string messageText, int subjectId): (int FaqId)
        public int SuggestClosestFaq(string messageText, int subjectId)
        {
            // TODO: Search FAQ database using subject context
            Console.WriteLine($"Suggesting closest FAQ for subject {subjectId}, query: {messageText}");

            return 1; // placeholder FAQ ID
        }

        // Detect intent of a message
        // UML: DetectIntent(string text): BotIntent
        public BotIntent DetectIntent(string text)
        {
            // TODO: Run intent classifier
            Console.WriteLine($"Detecting intent for text: {text}");

            return BotIntent.GeneralQuestion;
        }

        // Escalates to tutors if bot cannot resolve
        // UML: EscalateToTutors(int sessionId, string reason): TutorEscalation
        public TutorEscalation EscalateToTutors(int sessionId, string reason)
        {
            var escalation = new TutorEscalation
            {
                SessionId = sessionId,
                EscalationStatus = EscalationStatus.Pending
            };

            Console.WriteLine($"Escalation created for session {sessionId}, reason: {reason}");
            return escalation;
        }
    }

    // Placeholder for BotIntent (since UML refers to it)
    public enum BotIntent
    {
        GeneralQuestion,
        FAQLookup,
        Escalation
    }
}
