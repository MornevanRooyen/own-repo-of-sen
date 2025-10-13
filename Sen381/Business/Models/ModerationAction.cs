using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    // ---------- Enumeration ----------
    public enum Action
    {
        HidePost,
        UnhidePost,
        LockTopic,
        UnlockTopic
    }

    // ---------- Entity ----------
    public class ModerationAction
    {
        // ---------- Fields ----------
        private int id;
        private int adminId;
        private int targetId;
        private Action modAction;
        private string notes;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int AdminId
        {
            get => adminId;
            set => adminId = value;
        }

        public int TargetId
        {
            get => targetId;
            set => targetId = value;
        }

        public Action ModAction
        {
            get => modAction;
            set => modAction = value;
        }

        public string Notes
        {
            get => notes;
            set => notes = value;
        }

        // ---------- Methods ----------
        public void Method(Action type)
        {
            // TODO: Implement moderation logic based on action type
            ModAction = type;

            switch (type)
            {
                case Action.HidePost:
                    Console.WriteLine($"Post {TargetId} hidden by Admin {AdminId}.");
                    break;

                case Action.UnhidePost:
                    Console.WriteLine($"Post {TargetId} unhidden by Admin {AdminId}.");
                    break;

                case Action.LockTopic:
                    Console.WriteLine($"Topic {TargetId} locked by Admin {AdminId}.");
                    break;

                case Action.UnlockTopic:
                    Console.WriteLine($"Topic {TargetId} unlocked by Admin {AdminId}.");
                    break;
            }
        }
    }
}
