using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class RetrievalService
    {
        // Gets an answer from FAQ given a question.
        // UML: AnswerFromFaq(string question): (string answer, int faqId)
        public (string Answer, int FaqId) AnswerFromFaq(string question)
        {
            // TODO: Search FAQs for the matching question
            Console.WriteLine($"Searching FAQ for question: {question}");

            // Placeholder return
            return ("Sample FAQ answer", 1);
        }

        // Gets a definition for a given term
        // UML: AnswerDefinition(string term): string
        public string AnswerDefinition(string term)
        {
            // TODO: Search glossary/definitions
            Console.WriteLine($"Fetching definition for term: {term}");

            return "Sample definition text";
        }

        // Provides navigation answer for a given question
        // UML: AnswerNavigation(string question): string
        public string AnswerNavigation(string question)
        {
            // TODO: Implement logic for directing to the right resource/page
            Console.WriteLine($"Providing navigation for question: {question}");

            return "Sample navigation result";
        }
    }
}
