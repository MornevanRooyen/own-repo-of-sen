using Sen381.Business.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    
    public class ChatMessage
    {
        // ---------- Fields ----------
        private int id;
        private int sessionId;
        private string text;

        private List<SourceRef> sourceRefs = new List<SourceRef>();

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int SessionId
        {
            get => sessionId;
            set => sessionId = value;
        }

        public string Text
        {
            get => text;
            set => text = value;
        }

        public List<SourceRef> SourceRefs
        {
            get => sourceRefs;
            set => sourceRefs = value ?? new List<SourceRef>();
        }

        // ---------- Methods ----------
        public void Edit(string newText)
        {
            Text = newText;
        }
    }
}
