using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathChatBot.Models
{
    public static class ModelUtility
    {
        public static void SetStringRolesForUsers(this List<User> list)
        {
            foreach (var user in list)
            {
                var roles = user.UserRoleRelations.Select(x => x.Role).ToList();

                if (roles == null)
                {
                    user.Roles = "";
                    continue;
                }

                var names = roles.OrderBy(x => x.Name).Select(x => x.Name).ToList();

                var strRoles = string.Join(", ", names);
                user.Roles = strRoles;
            }
        }
    }
}
