using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class Session
    {
        // ---------- Fields ----------
        private int id;
        private int userId;
        private DateTime createdAt;
        private string ipAddress;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int UserId
        {
            get => userId;
            set => userId = value;
        }

        public DateTime CreatedAt
        {
            get => createdAt;
            set => createdAt = value;
        }

        public string IpAddress
        {
            get => ipAddress;
            set => ipAddress = value;
        }

        // ---------- Methods ----------
        public void Revoke()
        {
            // TODO: Implement session revocation logic (e.g., mark invalid, delete from DB, etc.)
            Console.WriteLine($"Session {Id} revoked.");
        }
    }
}
