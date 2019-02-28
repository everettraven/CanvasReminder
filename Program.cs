using System;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Security;



/*
    Program written by Bryce Palmer to go recieve due assignments and remind
    users of due assignments and how long they have to do said assignment.
    Currently only meant to test interactions with the Canvas API and parsing JSON.
    Only can send text reminders to AT&T phone numbers for testing purposes.   
*/

namespace ConsoleCanvasTracker
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();
        private static string AccessToken;
        private static int AssignmentsDue = 0;

        static void Main(string[] args)
        {
            try
            {
                string Email = "";
                string PhoneNumber = "";
                SecureString EmailPassword = new SecureString();

                //Just make a nice little load up
                Console.Title = "Canvas Tracker";
                Console.WriteLine("Canvas Tracker Console Version");
                Console.WriteLine("-------------------------------------------------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine("Please type your access token here");
                 AccessToken = Console.ReadLine(); 
               // GetAccessToAccount().Wait();

                Console.WriteLine("Would you like to implement a text reminder?");


                if(Console.ReadLine() == "yes")
                {
                    Console.WriteLine("Please enter your Email Address");
                    Email = Console.ReadLine();

                    ConsoleKeyInfo key;

                    Console.Write("Please enter your Email password: ");
                    do
                    {
                        key = Console.ReadKey(true);

                        // Ignore any key out of range.
                        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                        {
                            // Append the character to the password.
                            EmailPassword.AppendChar(key.KeyChar);
                            Console.Write("*");
                        }
                        else if(key.Key == ConsoleKey.Backspace)
                        {
                            Console.Write("\b \b");
                        }
                        
                        // Exit if Enter key is pressed.
                    } while (key.Key != ConsoleKey.Enter);
                    Console.WriteLine();

                    Console.WriteLine("Please enter your phone number");
                    PhoneNumber = Console.ReadLine();

                    ProcessResults().Wait();
                    if (AssignmentsDue > 0)
                    {
                        SendEmail(Email, EmailPassword, PhoneNumber);
                    }
                    

                }
                else
                {

                    ProcessResults().Wait();

                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                Console.ReadKey();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static async Task ProcessResults()
        {


            //Set up the serializer
            var CourseSerializer = new DataContractJsonSerializer(typeof(List<CanvasCourseParse>));

            //Clear the request headers
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", AccessToken));

            //Get the courses stream 
            var CoursesStreamTask = Client.GetStreamAsync("https://canvas.instructure.com/api/v1/courses");

            //Set the stream into a value of lists
            var CoursesResults = CourseSerializer.ReadObject(await CoursesStreamTask) as List<CanvasCourseParse>;

            //Go into each individual course
            foreach (var Course in CoursesResults)
            {
                if(Course.CourseName != null)
                {
                    Console.WriteLine(Course.CourseName);
                    Console.WriteLine();

                    AssignmentGrab(Course).Wait();
                }

            }

        }


        private static async Task AssignmentGrab(CanvasCourseParse Course)
        {

            var AssignmentSerializer = new DataContractJsonSerializer(typeof(List<CanvasAssignmentsParse>));

            Console.WriteLine("Assignments");
            Console.WriteLine("--------------------------------------------");

            //Start the Assignments Stream
            var AssignmentsStreamTask = Client.GetStreamAsync(String.Format("https://canvas.instructure.com/api/v1/courses/{0}/assignments", Course.CourseID));

            //Set the stream into a list
            var AssignmentsResults = AssignmentSerializer.ReadObject(await AssignmentsStreamTask) as List<CanvasAssignmentsParse>;

            //Go into each individual assignment
            foreach (var Assignment in AssignmentsResults)
            {
                //Check to see if the Assignment Due date is before the current date 
                if (DateTime.Compare(Assignment.TimeDue, DateTime.Now) <= 0)
                {
                    Console.WriteLine();
                }
                else
                {
                    AssignmentsDue = AssignmentsDue + 1;
                    Console.WriteLine(Assignment.AssignmentName);
                    Console.WriteLine(Assignment.TimeDue);
                    Console.WriteLine();

                }

            }

        }

        public static void SendEmail(string Email, SecureString EmailPassword, string PhoneNumber)
        {
            try
            {
                MailMessage TextMessage = new MailMessage(Email, String.Format("{0}@tmomail.net", PhoneNumber),"",String.Format("Hey there! You have {0} assignments due in the future. Make sure to check Canvas and do them!",AssignmentsDue));


                SmtpClient EmailClient = new SmtpClient("smtp.gmail.com");
                EmailClient.UseDefaultCredentials = false;
                EmailClient.Port = 587;
                EmailClient.EnableSsl = true;
                EmailClient.Timeout = 10000;
                EmailClient.Credentials = new System.Net.NetworkCredential(Email, EmailPassword);
                EmailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                EmailClient.Send(TextMessage);

            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }



        }


    }
}
