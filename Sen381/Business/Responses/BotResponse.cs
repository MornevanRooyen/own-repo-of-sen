using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Responses
{
    public class BotResponse
    {
        // ---------- Fields ----------
        private string text;
        private double confidence;
        private int suggestedFaqId;

        // ---------- Properties ----------
        public string Text
        {
            get => text;
            set => text = value;
        }

        public double Confidence
        {
            get => confidence;
            set => confidence = value;
        }

        public int SuggestedFaqId
        {
            get => suggestedFaqId;
            set => suggestedFaqId = value;
        }

        public List<SourceRef> SourceRefs { get; set; } = new List<SourceRef>();

        public bool ShouldEscalate { get; set; }

        // ---------- Constructors ----------
        public BotResponse() { }

        public BotResponse(string text, double confidence, int suggestedFaqId, bool shouldEscalate = false)
        {
            Text = text;
            Confidence = confidence;
            SuggestedFaqId = suggestedFaqId;
            ShouldEscalate = shouldEscalate;
        }
    }

    // Placeholder for SourceRef (UML shows it as referenced type)
    public class SourceRef
    {
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
