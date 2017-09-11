namespace Upload_Attachment_Example
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Services;
    using System.Web.UI;

    using Upload_Attachment_Example.Autotask;

    public partial class Default : Page
    {
        /// <summary>
        /// The API client.
        /// </summary>
        private static ATWS client;

        /// <summary>
        /// Gets the list of active emails in the database.
        /// </summary>
        /// <returns>The list of active emails.</returns>
        [WebMethod]
        public static ArrayList GetEmails()
        {
            if (client == null)
            {
                client = GetClient();
            }

            ArrayList emails = (ArrayList)HttpContext.Current.Session["Emails"];
            if (emails != null)
            {
                return emails;
            }

            emails = new ArrayList();
            List<Contact> contacts = GetContacts();
            foreach (Contact contact in contacts)
            {
                string email = contact.EMailAddress.ToString();
                if (!emails.Contains(email))
                {
                    emails.Add(email);
                }
            }

            HttpContext.Current.Session["Emails"] = emails;
            HttpContext.Current.Session["Contacts"] = contacts;
            return emails;
        }

        /// <summary>
        /// The Page_Load method.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (client == null)
            {
                client = GetClient();
            }            
        }

        /// <summary>
        /// The submitButton_Click method.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void SubmitButtonClick(object sender, EventArgs e)
        {
            this.error.Visible = false;
            string email = this.exampleInputEmail.Text;
            ArrayList emails = (ArrayList)this.Session["Emails"];
            List<Contact> contacts = (List<Contact>)this.Session["Contacts"];
            if (string.IsNullOrEmpty(email) || !emails.Contains(email))
            {
                this.error.Visible = true;
                this.error.InnerText = "Email doesn't exist in the database.";
                return;
            }

            if (!this.exampleInputFile.HasFile)
            {
                this.error.Visible = true;
                this.error.InnerText = "Please select a file to upload.";
                return;
            }

            Contact contact = contacts.Find(c => c.EMailAddress.ToString().Equals(email));
            Account account = GetAccount(contact.AccountID);
            Attachment attachment = new Attachment
            {
                Info = new AttachmentInfo
                {
                    FullPath = this.exampleInputFile.FileName,
                    ParentID = contact.AccountID,
                    ParentType = 1, // Account
                    Publish = 1, // All Autotask Users
                    Title = this.exampleInputFile.FileName,
                    Type = "FILE_ATTACHMENT"
                },
                Data = this.exampleInputFile.FileBytes
            };

            long result = client.CreateAttachment(attachment);
            if (result > 0)
            {
                ATWSZoneInfo zoneInfo = client.getZoneInfo(ConfigurationManager.AppSettings["Username"]);
                this.success.Visible = true;
                this.accountCommand.NavigateUrl = zoneInfo.WebUrl + "Autotask/AutotaskExtend/ExecuteCommand.aspx?Code=OpenAccount&AccountID=" + attachment.Info.ParentID;
                this.accountCommand.Text = account?.AccountName.ToString();
            }
            else
            {
                this.error.Visible = true;
                this.error.InnerText = "The file failed to upload. Please try again.";
            }
        }

        /// <summary>
        /// Gets the account with the given id.
        /// </summary>
        /// <param name="accountId">The id of the account to get.</param>
        /// <returns>An account entity or null if none were returned.</returns>
        private static Account GetAccount(object accountId)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<queryxml>");
            xml.AppendLine("<entity>Account</entity>");
            xml.AppendLine("<query>");
            xml.Append("<field>id<expression op=\"equals\">").Append(accountId).AppendLine("</expression></field>");
            xml.AppendLine("</query>");
            xml.AppendLine("</queryxml>");
            ATWSResponse response = client.query(xml.ToString());
            if (response.ReturnCode == -1)
            {
                StringBuilder message = new StringBuilder();
                foreach (ATWSError atwsError in response.Errors)
                {
                    message.Append(atwsError.Message).Append(Environment.NewLine);
                }

                throw new Exception(message.ToString());
            }

            if (response.EntityResults.Length > 0)
            {
                return response.EntityResults[0] as Account;
            }

            return null;
        }

        /// <summary>
        /// Gets the list of active contacts in the database.
        /// </summary>
        /// <returns>A list of contact entities.</returns>
        private static List<Contact> GetContacts()
        {
            List<Contact> contacts = new List<Contact>();
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<queryxml>");
            xml.AppendLine("<entity>Contact</entity>");
            xml.AppendLine("<query>");
            xml.AppendLine("<field>Active<expression op=\"equals\">1</expression></field>");
            xml.AppendLine("</query>");
            xml.AppendLine("</queryxml>");
            ATWSResponse response = client.query(xml.ToString());
            if (response.ReturnCode == -1)
            {
                StringBuilder message = new StringBuilder();
                foreach (ATWSError atwsError in response.Errors)
                {
                    message.Append(atwsError.Message).Append(Environment.NewLine);
                }

                throw new Exception(message.ToString());
            }

            // The API returns a max of 500 records. If 500 are returned the query must be looped to return the rest.
            if (response.EntityResults.Length == 500)
            {
                contacts.AddRange(response.EntityResults.Cast<Contact>());
                while (response.EntityResults.Length == 500)
                {
                    // Results are returned in order or id. Making the next query get all results greater than the last
                    // id value will get the next section of results.
                    long lastId = response.EntityResults[response.EntityResults.Length - 1].id;
                    xml = new StringBuilder();
                    xml.AppendLine("<queryxml>");
                    xml.AppendLine("<entity>Contact</entity>");
                    xml.AppendLine("<query>");
                    xml.Append("<field>id<expression op=\"greaterthan\">")
                        .Append(lastId)
                        .AppendLine("</expression></field>");
                    xml.AppendLine("<field>Active<expression op=\"equals\">1</expression></field>");
                    xml.AppendLine("</query>");
                    xml.AppendLine("</queryxml>");
                    response = client.query(xml.ToString());
                    if (response.ReturnCode == -1)
                    {
                        StringBuilder message = new StringBuilder();
                        foreach (ATWSError atwsError in response.Errors)
                        {
                            message.Append(atwsError.Message).Append(Environment.NewLine);
                        }

                        throw new Exception(message.ToString());
                    }

                    contacts.AddRange(response.EntityResults.Cast<Contact>());
                }
            }
            else
            {
                contacts.AddRange(response.EntityResults.Cast<Contact>());
            }

            return contacts;
        }

        /// <summary>
        /// Creates the ATWS client for the API.
        /// </summary>
        /// <returns>The ATWS client.</returns>
        private static ATWS GetClient()
        {
            string username = ConfigurationManager.AppSettings["Username"];
            string password = ConfigurationManager.AppSettings["Password"];
            ATWS service = new ATWS { Url = ConfigurationManager.AppSettings["BaseURL"] };

            try
            {
                ATWSZoneInfo zoneInfo = service.getZoneInfo(username);
                string urlZone = zoneInfo.URL;
                service = new ATWS { Url = urlZone };
                NetworkCredential credentials = new NetworkCredential(username, password);
                CredentialCache cache = new CredentialCache { { new Uri(service.Url), "Basic", credentials } };
                service.Credentials = cache;
            }
            catch (Exception exception)
            {
                throw new Exception("Error creating web service: " + exception.Message);
            }

            return service;
        }
    }
}