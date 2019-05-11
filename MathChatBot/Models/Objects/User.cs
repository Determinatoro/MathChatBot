using System;
using System.ComponentModel;
using System.Linq;
using static MathChatBot.Models.Role;

namespace MathChatBot.Models
{
    public partial class User
    {
        public string Roles { get; set; }

        public string Name { get { return FirstName + " " + LastName; } }
    }
}

