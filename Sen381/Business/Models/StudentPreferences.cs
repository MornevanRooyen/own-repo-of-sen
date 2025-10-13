using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class StudentPreferences
    {
        // ---------- Fields ----------
        private bool notifyByEmail;
        private bool notifyBySMS;
        private bool notifyByWhatsapp;

        // ---------- Properties ----------
        public bool NotifyByEmail
        {
            get => notifyByEmail;
            set => notifyByEmail = value;
        }

        public bool NotifyBySMS
        {
            get => notifyBySMS;
            set => notifyBySMS = value;
        }

        public bool NotifyByWhatsapp
        {
            get => notifyByWhatsapp;
            set => notifyByWhatsapp = value;
        }

        // ---------- Constructor ----------
        public StudentPreferences()
        {
            SetDefaults();
        }

        // ---------- Methods ----------
        public void SetDefaults()
        {
            notifyByEmail = true;      // default: email notifications ON
            notifyBySMS = false;       // default: SMS OFF
            notifyByWhatsapp = false;  // default: WhatsApp OFF
        }
    }
}
