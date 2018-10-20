using System;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

/* Program written by Bryce Palmer to go recieve due assignments and remind
    users of due assignments and how long they have to do said assignment */

namespace ConsoleCanvasTracker
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();
        private static string AccessToken;

        static void Main(string[] args)
        {
            try
            {
                //Just make a nice little load up
                Console.WriteLine("Canvas Tracker Console Version");
                Console.WriteLine("-------------------------------------------------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine("Please type your access token here");
                AccessToken = Console.ReadLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();


                ProcessResults().Wait();
                Console.ReadKey();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static async Task ProcessResults()
        {


            //Set up the serializer
            var CourseSerializer = new DataContractJsonSerializer(typeof(List<CanvasCourseParse>));
            var AssignmentSerializer = new DataContractJsonSerializer(typeof(List<CanvasAssignmentsParse>));

            //Clear the request headers
            Client.DefaultRequestHeaders.Accept.Clear();

            //Set the first call url
            var CoursesStreamTask = Client.GetStreamAsync(String.Format("https://canvas.instructure.com/api/v1/courses?access_token={0}", AccessToken));

            //Set the stream into a value of lists
            var CoursesResults = CourseSerializer.ReadObject(await CoursesStreamTask) as List<CanvasCourseParse>;

            foreach(var Course in CoursesResults)
            {
                Console.WriteLine(Course.CourseName);
                Console.WriteLine();
                Console.WriteLine("Assignments");
                Console.WriteLine("--------------------------------------------");

                var AssignmentsStreamTask = Client.GetStreamAsync(String.Format("https://canvas.instructure.com/api/v1/courses/{0}/assignments?access_token={1}", Course.CourseID, AccessToken));

                var AssignmentsResults = AssignmentSerializer.ReadObject(await AssignmentsStreamTask) as List<CanvasAssignmentsParse>;

                foreach (var Assignment in AssignmentsResults)
                {
                    Console.WriteLine(Assignment.AssignmentName);
                    Console.WriteLine(Assignment.TimeDue);
                    Console.WriteLine();
                }
            }

        }
    }
}
