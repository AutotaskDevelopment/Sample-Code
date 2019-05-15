using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;

using SampleAutotaskAPI.Autotask.Net.Webservices;

namespace SampleAutotaskAPI
{
    /// <summary>
    /// Public Class AutotaskAPITests.
    /// </summary>
    public class AutotaskApiTests
    {
        /// <summary>
        /// The web service object.
        /// </summary>
        private readonly ATWS atwsServices;

        /// <summary>
        /// The base URL for the web service.
        /// </summary>
        private readonly string webServiceBaseApiUrl = ConfigurationManager.AppSettings["APIServiceURLZoneInfo"];

        /// <summary>
        /// Initializes a new instance of the <see cref="AutotaskApiTests"/> class.
        /// </summary>
        /// <param name="user">The username.</param>
        /// <param name="pass">The password.</param>
        public AutotaskApiTests(string user, string pass, string trackingId)
        {
            this.atwsServices = new ATWS { Url = this.webServiceBaseApiUrl };
            try
            {
                ATWSZoneInfo zoneInfo = this.atwsServices.getZoneInfo(user);
                if (zoneInfo.ErrorCode >= 0)
                {
                    string urlZone = zoneInfo.URL;
                    this.atwsServices = new ATWS { Url = urlZone };
                    NetworkCredential credentials = new NetworkCredential(user, pass);
                    CredentialCache cache = new CredentialCache { { new Uri(this.atwsServices.Url), "Basic", credentials } };
                    this.atwsServices.Credentials = cache;
                    this.atwsServices.AutotaskIntegrationsValue = new AutotaskIntegrations { IntegrationCode = trackingId };
                }
                else
                {
                    throw new Exception("Error with getZoneInfo()");
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error with getZoneInfo()- error: " + exception.Message);
            }
        }

        /// <summary>
        /// This method searches for a resource given a username.
        /// </summary>
        /// <param name="resourceUserName">Contains the resource username to search for</param>
        /// <returns>ID of the resource.</returns>
        public long FindResource(string resourceUserName)
        {
            long ret = -1;

            // Query Resource to get the owner of the lead
            StringBuilder strResource = new StringBuilder();
            strResource.Append("<queryxml version=\"1.0\">");
            strResource.Append("<entity>Resource</entity>");
            strResource.Append("<query>");
            strResource.Append("<field>UserName<expression op=\"equals\">");
            strResource.Append(resourceUserName);
            strResource.Append("</expression></field>");
            strResource.Append("</query></queryxml>");

            ATWSResponse respResource = this.atwsServices.query(strResource.ToString());

            if (respResource.ReturnCode > 0 && respResource.EntityResults.Length > 0)
            {
                // Get the ID for the resource
                ret = respResource.EntityResults[0].id;
            }

            return ret;
        }

        /// <summary>
        /// This method searches for a contact given an email address.
        /// </summary>
        /// <param name="contactEmail">The contact email address to search for.</param>
        /// <returns>The matching contact.</returns>
        public Contact FindContact(string contactEmail)
        {
            Contact contact = null;

            if (contactEmail.Length <= 0)
            {
                return null;
            }

            // Query Contact to see if the contact is already in the system
            StringBuilder strResource = new StringBuilder();
            strResource.Append("<queryxml version=\"1.0\">");
            strResource.Append("<entity>Contact</entity>");
            strResource.Append("<query>");
            strResource.Append("<field>EMailAddress<expression op=\"equals\">");
            strResource.Append(contactEmail);
            strResource.Append("</expression></field>");
            strResource.Append("</query></queryxml>");

            ATWSResponse respResource = this.atwsServices.query(strResource.ToString());

            if (respResource.ReturnCode > 0 && respResource.EntityResults.Length > 0)
            {
                contact = (Contact)respResource.EntityResults[0];
            }

            return contact;
        }

        /// <summary>
        /// This method will update a UDF for a given Contact.
        /// </summary>
        /// <param name="contact">Contact to update.</param>
        /// <param name="udfName">UDF Name to update.</param>
        /// <param name="udfValue">UDF Value to update</param>
        /// <returns>Returns the updated Contact.</returns>
        public Contact UpdateContactUdf(Contact contact, string udfName, string udfValue)
        {
            Contact retContact = null;
            UserDefinedField fldFieldToFind = FindUserDefinedField(contact.UserDefinedFields, udfName);
            if (fldFieldToFind != null)
            {
                fldFieldToFind.Value = udfValue;
            }
            else
            {
                fldFieldToFind = new UserDefinedField { Name = udfName, Value = udfValue };
                contact.UserDefinedFields = new[] { fldFieldToFind };
            }

            Entity[] entUpdateContact = { contact };
            ATWSResponse respUpdateContact = this.atwsServices.update(entUpdateContact);
            if (respUpdateContact.ReturnCode == -1)
            {
                throw new Exception("Could not update the Contact: " + respUpdateContact.EntityReturnInfoResults[0].Message);
            }

            if (respUpdateContact.ReturnCode > 0 && respUpdateContact.EntityResults.Length > 0)
            {
                retContact = (Contact)respUpdateContact.EntityResults[0];
            }

            return retContact;
        }

        /// <summary>
        /// This method will update a UDF Picklist for a given Contact.
        /// </summary>
        /// <param name="contact">Contact to update.</param>
        /// <param name="udfName">UDF Name to update.</param>
        /// <param name="udfPicklistLabel">UDF Picklist Label to update</param>
        /// <returns>Returns the updated Contact.</returns>
        public Contact UpdateContactUdfPicklist(Contact contact, string udfName, string udfPicklistLabel)
        {
            Contact retContact = null;
            UserDefinedField fldFieldToFind = FindUserDefinedField(contact.UserDefinedFields, udfName);
            Field[] fieldsUdfContact = this.atwsServices.getUDFInfo("Contact");
            string strUdfPickListValue = PickListValueFromField(fieldsUdfContact, udfName, udfPicklistLabel);
            if (fldFieldToFind != null)
            {
                fldFieldToFind.Value = strUdfPickListValue;
            }
            else
            {
                fldFieldToFind = new UserDefinedField { Name = udfName, Value = strUdfPickListValue };
                contact.UserDefinedFields = new[] { fldFieldToFind };
            }

            Entity[] entUpdateContact = { contact };
            ATWSResponse respUpdateContact = this.atwsServices.update(entUpdateContact);
            if (respUpdateContact.ReturnCode == -1)
            {
                throw new Exception("Could not update the Contact: " + respUpdateContact.EntityReturnInfoResults[0].Message);
            }

            if (respUpdateContact.ReturnCode > 0 && respUpdateContact.EntityResults.Length > 0)
            {
                retContact = (Contact)respUpdateContact.EntityResults[0];
            }

            return retContact;
        }

        /// <summary>
        /// Creates a new Contact.
        /// </summary>
        /// <param name="accountId">Account ID of account to create contact.</param>
        /// <param name="firstName">Contact first name.</param>
        /// <param name="lastName">Contact last name.</param>
        /// <param name="email">Contact email address.</param>
        /// <param name="udfName">UDF Name to set.</param>
        /// <param name="udfValue">UDF Value to set</param>
        /// <returns>>Returns the new Contact.</returns>
        public Contact CreateContact(long accountId, string firstName, string lastName, string email, string udfName, string udfValue)
        {
            Contact retContact = null;

            // Time to create the Contact
            Contact contactAct = new Contact
            {
                AccountID = accountId,
                FirstName = firstName,
                LastName = lastName,
                EMailAddress = email.ToLower(),
                Active = "1"
            };

            Field[] fieldsUdfContact = this.atwsServices.getUDFInfo("Contact");

            // Create a container to hold all the UDF's
            List<UserDefinedField> udfContainer = new List<UserDefinedField>();

            Field fldFieldToFind = FindField(fieldsUdfContact, udfName);
            if (fldFieldToFind != null)
            {
                udfContainer.Add(new UserDefinedField()
                {
                    Name = udfName,
                    Value = udfValue
                });
            }

            // Time to add the UDF's from the container to the Contact
            contactAct.UserDefinedFields = udfContainer.ToArray();

            Entity[] entContact = { contactAct };

            ATWSResponse respContact = this.atwsServices.create(entContact);
            if (respContact.ReturnCode > 0 && respContact.EntityResults.Length > 0)
            {
                retContact = (Contact)respContact.EntityResults[0];
            }
            else
            {
                if (respContact.EntityReturnInfoResults.Length > 0)
                {
                    throw new Exception("Could not create the Contact: " + respContact.EntityReturnInfoResults[0].Message);
                }
            }

            return retContact;
        }

        /// <summary>
        /// Creates a ticket note by the provided resource id using Impersonation.
        /// </summary>
        /// <param name="assignResourceId">The resource id.</param>
        /// <returns>A new ticket note.</returns>
        public AccountNote CreateAccountNoteAs(long impersonateResourceId, long assignResourceId, DateTime startDateTime, DateTime endDateTime)
        {
            AccountNote retNote = null;
            Field[] fields = this.atwsServices.GetFieldInfo("AccountNote");
            string actionTypeValue = PickListValueFromField(fields, "ActionType", "General");
            AccountNote note = new AccountNote
            {
                id = 0,
                AccountID = 0,
                Note = "Test note created via impersonation",
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                ActionType = actionTypeValue,
                AssignedResourceID = assignResourceId
            };

            Entity[] entContact = { note };

            this.atwsServices.AutotaskIntegrationsValue.ImpersonateAsResourceID = (int)impersonateResourceId;
            ATWSResponse respContact = this.atwsServices.create(entContact);
            if (respContact.ReturnCode > 0 && respContact.EntityResults.Length > 0)
            {
                retNote = (AccountNote)respContact.EntityResults[0];
            }
            else
            {
                if (respContact.EntityReturnInfoResults.Length > 0)
                {
                    throw new Exception("Could not create the Account Note: " + respContact.EntityReturnInfoResults[0].Message);
                }
            }

            return retNote;
        }

        /// <summary>
        /// Returns the value of a picklistitem given an array of fields and the field name that contains the picklist
        /// </summary>
        /// <param name="fields">array of Fields that contains the field to find</param>
        /// <param name="strField">name of the Field to search in</param>
        /// <param name="strPickListName">name of the picklist value to return the value from</param>
        /// <returns>value of the picklist item to search for</returns>
        protected static string PickListValueFromField(Field[] fields, string strField, string strPickListName)
        {
            string strRet = string.Empty;

            Field fldFieldToFind = FindField(fields, strField);
            if (fldFieldToFind == null)
            {
                throw new Exception("Could not get the " + strField + " field from the collection");
            }

            PickListValue plvValueToFind = FindPickListLabel(fldFieldToFind.PicklistValues, strPickListName);
            if (plvValueToFind != null)
            {
                strRet = plvValueToFind.Value;
            }

            return strRet;
        }

        /// <summary>
        /// Returns the label of a picklist when the value is sent
        /// </summary>
        /// <param name="fields">entity fields</param>
        /// <param name="strField">picklick to choose from</param>
        /// <param name="strPickListValue">value ("id") of picklist</param>
        /// <returns>picklist label</returns>
        protected static string PickListLabelFromValue(Field[] fields, string strField, string strPickListValue)
        {
            string strRet = string.Empty;

            Field fldFieldToFind = FindField(fields, strField);
            if (fldFieldToFind == null)
            {
                throw new Exception("Could not get the " + strField + " field from the collection");
            }

            PickListValue plvValueToFind = FindPickListValue(fldFieldToFind.PicklistValues, strPickListValue);
            if (plvValueToFind != null)
            {
                strRet = plvValueToFind.Label;
            }

            return strRet;
        }

        /// <summary>
        /// Used to find a specific Field in an array based on the name
        /// </summary>
        /// <param name="field">array containing Fields to search from</param>
        /// <param name="name">contains the name of the Field to search for</param>
        /// <returns>Field match</returns>
        protected static Field FindField(Field[] field, string name)
        {
            return Array.Find(field, element => element.Name == name);
        }

        /// <summary>
        /// Used to find a specific value in a picklist
        /// </summary>
        /// <param name="pickListValue">array of PickListsValues to search from</param>
        /// <param name="name">contains the name of the PickListValue to search for</param>
        /// <returns>PickListValue match</returns>
        protected static PickListValue FindPickListLabel(PickListValue[] pickListValue, string name)
        {
            return Array.Find(pickListValue, element => element.Label == name);
        }

        /// <summary>
        /// Used to find a specific value in a picklist
        /// </summary>
        /// <param name="pickListValue">array of PickListsValues to search from</param>
        /// <param name="valueID">contains the value of the PickListValue to search for</param>
        /// <returns>PickListValue match</returns>
        protected static PickListValue FindPickListValue(PickListValue[] pickListValue, string valueID)
        {
            return Array.Find(pickListValue, element => element.Value == valueID);
        }

        /// <summary>
        /// Used to find a specific User Defined Field in an array based on the name
        /// </summary>
        /// <param name="field">array containing User Defined Field to search from</param>
        /// <param name="name">contains the name of the User Defined Field to search for</param>
        /// <returns>UserDefinedField match</returns>
        protected static UserDefinedField FindUserDefinedField(UserDefinedField[] field, string name)
        {
            return Array.Find(field, element => element.Name == name);
        }
    }
}
