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

                while (!parser.EndOfData)
                {
                    //Process row
                    var fields = parser.ReadFields().ToList();

                    if (firstLine)
                    {
                        firstLine = false;
                        headers = fields.ToList();
                    }
                    else
                    {
                        if (fields.Count == headers.Count)
                        {
                            var dictionary = new Dictionary<string, string>();

                            for (int i = 0; i < headers.Count; i++)
                            {
                                dictionary[headers[i]] = fields[i];
                            }

                            list.Add(dictionary);
                        }
                    }
                }
            }

            return list;
        }

        public static void SetObjectValues(Dictionary<string, string> dictionary, object obj)
        {
            foreach (var key in dictionary.Keys)
            {
                try
                {
                    var pair = dictionary.SingleOrDefault(p => p.Key == key);
                    PropertyInfo propertyInfo = obj.GetType().GetProperty(pair.Key);
                    propertyInfo.SetValue(obj, Convert.ChangeType(pair.Value, propertyInfo.PropertyType), null);
                } catch { }
            }
        }

        public static string GetCSVFileSplitter(string csvFileText)
        {
            var numberOfCommas = csvFileText.Where(x => x == ',').ToList().Count;
            var numberOfSemiColons = csvFileText.Where(x => x == ';').ToList().Count;

            return numberOfCommas > numberOfSemiColons ? "," : ";";
        }
    }
}
