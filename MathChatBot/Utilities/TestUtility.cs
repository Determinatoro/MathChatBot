using Effort.DataLoaders;
using MathChatBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathChatBot.Utilities
{
    public class TestUtility
    {
        private static string GetTestDataFolder(string testDataFolder)
        {
            var projectFolder = Utility.GetProjectFolder();
            return Path.Combine(projectFolder, "test_data", testDataFolder);
        }

        public static MathChatBotEntities GetInMemoryContext()
        {
            var test_data_folder = GetTestDataFolder("database_mock_data");
            var dataLoader = new CsvDataLoader(test_data_folder);
            var inMemoryConnection = Effort.EntityConnectionFactory.CreateTransient("name=MathChatBotEntitiesTest", dataLoader);
            var inMemoryContext = new CustomEntity(inMemoryConnection, false);
            return inMemoryContext;
        }

        public static void CreateHelpRequests()
        {
            var @class = DatabaseUtility.Entity.Classes.FirstOrDefault(x => x.Name == "B100");

            var users = DatabaseUtility.GetUsersInClass(@class, new Role.RoleTypes[] { Role.RoleTypes.Student }, false);
            var materials = DatabaseUtility.Entity.Materials.ToList();

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var material = materials[(i + 3) % materials.Count];

                DatabaseUtility.MakeHelpRequest(user.Id, material.TermId, material.Id, null, null);
            }
        }

    }
}
