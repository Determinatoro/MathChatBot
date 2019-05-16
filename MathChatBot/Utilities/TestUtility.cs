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
    }
}
