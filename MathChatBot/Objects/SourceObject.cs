using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathChatBot.Objects
{
    public enum SourceTypes
    {
        NotDefined,
        Definition,
        Example,
        Assignment
    }

    public class SourceObject
    {
        public int SourceRequests { get; set; }
        public string SourceType { get; set; }
        public string Source { get; set; }

        public SourceObject(SourceTypes sourceType, string source)
        {
            SourceType = sourceType.GetName();
            Source = source;
        }

        public SourceObject(int sourceRequests, string materialSource, string exampleSource, string assignmentSource)
        {
            SourceRequests = sourceRequests;

            if (materialSource != null)
            {
                Source = materialSource;
                SourceType = SourceTypes.Definition.GetName();
            }
            else if (exampleSource != null)
            {
                Source = exampleSource;
                SourceType = SourceTypes.Example.GetName();
            }
            else if (assignmentSource != null)
            {
                Source = assignmentSource;
                SourceType = SourceTypes.Assignment.GetName();
            }
        }
    }
}
