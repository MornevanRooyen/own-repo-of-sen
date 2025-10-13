using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Policies
{
    public class BotPolicy
    {
        // ---------- Fields ----------
        private double lowConfidenceThreshold;
        private bool escalationRequired;

        // ---------- Properties ----------
        public double LowConfidenceThreshold
        {
            get => lowConfidenceThreshold;
            set => lowConfidenceThreshold = value;
        }

        public bool EscalationRequired
        {
            get => escalationRequired;
            set => escalationRequired = value;
        }

        // ---------- Constructors ----------
        public BotPolicy() { }

        public BotPolicy(double threshold, bool escalationRequired)
        {
            LowConfidenceThreshold = threshold;
            EscalationRequired = escalationRequired;
        }
    }
}
