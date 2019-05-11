using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MathChatBot.Utilities
{
    public static class CSVUtility
    {

        /// <summary>
        /// Parse a CSV file from a given path
        /// </summary>
        /// <param name="path">The path to the CSV file</param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> ParseCSV(string path)
        {
            var list = new List<Dictionary<string, string>>();

            var fileText = File.ReadAllText(path);
            var splitter = GetCSVFileSplitter(fileText);

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(splitter);

                var firstLine = true;
                var headers = new List<string>();

                // Until end of file
                while (!parser.EndOfData)
                {
                    // Read fields for a single row
                    var fields = parser.ReadFields().ToList();

                    // Skip first row as it contains the headers for each column of data
                    if (firstLine)
                    {
                        firstLine = false;
                        headers = fields.ToList();
                    }
                    else
                    {
                        if (fields.Count == headers.Count)
                        {
                            // Create a dictionary for each row
                            var dictionary = new Dictionary<string, string>();
                            for (int i = 0; i < headers.Count; i++)
                                dictionary[headers[i]] = fields[i];
                            
                            list.Add(dictionary);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Set property values for an object with the values from a CSV file
        /// </summary>
        /// <param name="dictionary">Dictionary containing an entry from an CSV file</param>
        /// <param name="obj">The object</param>
        public static void SetObjectValues(Dictionary<string, string> dictionary, object obj)
        {
            foreach (var key in dictionary.Keys)
            {
                try
                {
                    var pair = dictionary.SingleOrDefault(p => p.Key == key);
                    PropertyInfo propertyInfo = obj.GetType().GetProperty(pair.Key);
                    if (propertyInfo == null)
                        continue;
                    propertyInfo.SetValue(obj, Convert.ChangeType(pair.Value, propertyInfo.PropertyType), null);
                } catch { }
            }
        }

        /// <summary>
        /// Get the splitter used in the CSV file
        /// </summary>
        /// <param name="csvFileText">The text in the CSV file</param>
        /// <returns></returns>
        public static string GetCSVFileSplitter(string csvFileText)
        {
            var numberOfCommas = csvFileText.Where(x => x == ',').ToList().Count;
            var numberOfSemiColons = csvFileText.Where(x => x == ';').ToList().Count;

            return numberOfCommas > numberOfSemiColons ? "," : ";";
        }

    }
}
