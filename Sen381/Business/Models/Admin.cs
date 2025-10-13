using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class Admin : User
    {
        // <<Fields>>
        private int adminId;

        // <<Properties>>
        public int AdminId
        {
            get => adminId;
            set => adminId = value;
        }

        // Operations (as per UML; parameterless + void)
        public void ApproveTutor()
        {
            // TODO: implement tutor approval workflow (e.g., set status=Approved on a pending tutor profile)
        }

        public void SuspendUser()
        {
            // TODO: implement user suspension workflow (e.g., flag user, set SuspendedUntil, write audit log)
        }

        public void RemoveContent()
        {
            // TODO: implement content removal workflow (e.g., delete/hide a resource/post and record a moderation action)
        }
    }
}
