using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Globalization;

namespace ConsoleCanvasTracker
{
    [DataContract(Name = "assignments")]
    class CanvasAssignmentsParse
    {
        [DataMember(Name = "id")]
        public Int64 ID { get; set; }

        [DataMember(Name ="name")]
        public string AssignmentName { get; set; }

        [DataMember(Name = "submission")]
        public string Submission { get; set; }

        [DataMember(Name = "due_at")]
        private string DueDate { get; set; }

        [IgnoreDataMember]
        public DateTime TimeDue
        {
            get
            {
                DateTime TimeResult = new DateTime();
                if(DateTime.TryParseExact(DueDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,DateTimeStyles.None, out TimeResult))
                {
                    return TimeResult;
                }
                else
                {
                    return TimeResult;
                }

            }
        }
    }
}
