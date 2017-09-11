namespace EmoticonWebProject
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web.UI;

    using EmoticonWebProject.Autotask;

    public partial class Default : Page
    {
        /// <summary>
        /// The Page_Load Method.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string ticketId = this.Request.Params["TicketID"];
            if (string.IsNullOrEmpty(ticketId))
            {
                return;
            }

            string rating = this.Request.Params["Rating"];

            ATWS client = this.GetClient();
            Ticket ticket = this.GetTicket(client, ticketId);
            if (ticket == null)
            {
                return;
            }

            // If the ticket has no UDF values populated the user defined field array will be empty.
            if (ticket.UserDefinedFields.Length == 0)
            {
                UserDefinedField responseRating = new UserDefinedField { Name = "Emoticon Response", Value = rating };
                UserDefinedField responseDate = new UserDefinedField { Name = "Emoticon Response Date", Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) };
                ticket.UserDefinedFields = new UserDefinedField[2];
                ticket.UserDefinedFields[0] = responseRating;
                ticket.UserDefinedFields[1] = responseDate;
            }
            else
            {
                UserDefinedField responseRating = ticket.UserDefinedFields.First(udf => udf.Name.Equals("Emoticon Response"));
                responseRating.Value = rating;
                UserDefinedField responseDate = ticket.UserDefinedFields.First(udf => udf.Name.Equals("Emoticon Response Date"));
                responseDate.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            }

            ATWSResponse response = client.update(new Entity[] { ticket });
            if (response.ReturnCode != -1)
            {
                return;
            }

            StringBuilder message = new StringBuilder();
            foreach (ATWSError atwsError in response.Errors)
            {
                message.Append(atwsError.Message).Append(Environment.NewLine);
            }

            throw new Exception(message.ToString());
        }

        /// <summary>
        /// Creates the ATWS client for the API.
        /// </summary>
        /// <returns>The ATWS client.</returns>
        private ATWS GetClient()
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

        /// <summary>
        /// Gets the ticket using the ticket id.
        /// </summary>
        /// <param name="client">The API client.</param>
        /// <param name="ticketId">The id of the ticket to get.</param>
        /// <returns>A ticket entity or null if no results were returned.</returns>
        private Ticket GetTicket(ATWS client, string ticketId)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<queryxml>");
            xml.AppendLine("<entity>Ticket</entity>");
            xml.AppendLine("<query>");
            xml.Append("<field>id<expression op=\"equals\">").Append(ticketId).AppendLine("</expression></field>");
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
                return response.EntityResults[0] as Ticket;
            }

            return null;
        }
    }
}