using MathChatBot.Utilities;
using System;
using static MathChatBot.Models.Role;

namespace MathChatBot.Models
{
    public partial class Role
    {
        public enum RoleTypes
        {
            Administrator,
            Teacher,
            Student,
            None
        }

        public RoleTypes RoleType
        {
            get
            {
                if (Name == null)
                    return RoleTypes.None;

                return Utility.ParseEnum<RoleTypes>(Name);
            }
        }

        public bool IsAssigned
        {
            get; set;
        }
    }
}
