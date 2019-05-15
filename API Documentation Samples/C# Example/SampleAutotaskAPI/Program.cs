using System;
using System.Configuration;

using SampleAutotaskAPI.Autotask.Net.Webservices;

namespace SampleAutotaskAPI
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                AutotaskApiTests test = new AutotaskApiTests(
                    ConfigurationManager.AppSettings["APIUsername"],
                    ConfigurationManager.AppSettings["APIPassword"],
                    ConfigurationManager.AppSettings["APITrackingID"]);

                // Search for a resource given a username
                long resourceId = test.FindResource("RESOURCE USER NAME");
                long resourceId2 = test.FindResource("RESOURCE USER NAME");

                // Create Ticket Note Impersonated as resource above
                AccountNote note = test.CreateAccountNoteAs(resourceId, resourceId2, DateTime.Now, DateTime.Now.AddDays(5));

                // Search for a Contact given an email address
                Contact contact = test.FindContact("CONTACT EMAIL ADDRESS");

                // Update Contact UDF
                contact = test.UpdateContactUdf(contact, "UDF NAME", "UDF VALUE");

                // Update Contact Picklist UDF
                contact = test.UpdateContactUdfPicklist(contact, "UDF NAME", "UDF PICKLIST LABEL");

                // Create NEW Contact with UDF
                Contact newContact = test.CreateContact(Convert.ToInt64(contact.AccountID), "FIRST NAME", "LAST NAME", "EMAIL", "UDF NAME", "UDF VALUE");

            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }
    }
}
