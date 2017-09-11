Imports System.Configuration
Imports System.Net
Imports System.Text

Imports SampleAutotaskAPIVB.Autotask.Net.Webservices

''' <summary>
''' Public Class AutotaskAPITests.
''' </summary>
Public Class AutotaskApiTests
    Private ReadOnly _atwsServices As ATWS = Nothing
    Private ReadOnly _webServiceBaseApiUrl As String = ConfigurationManager.AppSettings("APIServiceURLZoneInfo")

    ''' <summary>
    ''' Public Constructor for API Tests.
    ''' </summary>
    Public Sub New(user As String, pass As String)
        Me._atwsServices = New ATWS With { .Url = Me._webServiceBaseAPIURL }

        Try
            Dim zoneInfo As New ATWSZoneInfo()
            zoneInfo = Me._atwsServices.getZoneInfo(user)
            If zoneInfo.ErrorCode >= 0 Then
                Me._atwsServices = New ATWS With { .Url = zoneInfo.URL }
                Dim cache As New CredentialCache From { {New Uri(Me._atwsServices.Url), "BASIC", New NetworkCredential(user, pass)} }
                Me._atwsServices.Credentials = cache
            Else
                Throw New Exception("Error with getZoneInfo()")
            End If
        Catch ex As Exception
            Throw New Exception("Error with getZoneInfo()- error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' This method searches for a resource given a username.
    ''' </summary>
    ''' <param name="resourceUserName">Contains the resource username to search for</param>
    ''' <returns>ID of the resource.</returns>
    Public Function FindResource(resourceUserName As String) As Long
        Dim ret As Long = -1

        ' Query Resource to get the owner of the lead
        Dim strResource As New StringBuilder()
        strResource.Append("<queryxml version=""1.0"">")
        strResource.Append("<entity>Resource</entity>")
        strResource.Append("<query>")
        strResource.Append("<field>UserName<expression op=""equals"">")
        strResource.Append(resourceUserName)
        strResource.Append("</expression></field>")
        strResource.Append("</query></queryxml>")

        Dim respResource As ATWSResponse = Me._atwsServices.query(strResource.ToString())

        If respResource.ReturnCode > 0 AndAlso respResource.EntityResults.Length > 0 Then
            ' Get the ID for the resource
            ret = respResource.EntityResults(0).id
        End If
        Return ret
    End Function

    ''' <summary>
    ''' This method searches for a contact given an email address.
    ''' </summary>
    ''' <param name="contactEmail">The contact email address to search for.</param>
    ''' <returns>The matching contact.</returns>
    Public Function FindContact(contactEmail As String) As Contact
        Dim contact As Contact = Nothing

        If contactEmail.Length > 0 Then
            ' Query Contact to see if the contact is already in the system
            Dim strResource As New StringBuilder()
            strResource.Append("<queryxml version=""1.0"">")
            strResource.Append("<entity>Contact</entity>")
            strResource.Append("<query>")
            strResource.Append("<field>EMailAddress<expression op=""equals"">")
            strResource.Append(contactEmail)
            strResource.Append("</expression></field>")
            strResource.Append("</query></queryxml>")

            Dim respResource As ATWSResponse = Me._atwsServices.query(strResource.ToString())

            If respResource.ReturnCode > 0 AndAlso respResource.EntityResults.Length > 0 Then
                contact = DirectCast(respResource.EntityResults(0), Contact)
            End If
        End If

        Return contact
    End Function

    ''' <summary>
    ''' This method will update a UDF for a given Contact.
    ''' </summary>
    ''' <param name="contact">Contact to update.</param>
    ''' <param name="udfName">UDF Name to update.</param>
    ''' <param name="udfValue">UDF Value to update</param>
    ''' <returns>Returns the updated Contact.</returns>
    Public Function UpdateContactUdf(contact As Contact, udfName As String, udfValue As String) As Contact
        Dim retContact As Contact = Nothing
        Dim fldFieldToFind As UserDefinedField = Nothing
        fldFieldToFind = FindUserDefinedField(contact.UserDefinedFields, UDFName)
        If fldFieldToFind IsNot Nothing Then
            fldFieldToFind.Value = UDFValue
        Else
            fldFieldToFind = New UserDefinedField With { .Name = udfName, .Value = udfValue }
            contact.UserDefinedFields = New UserDefinedField(){ fldFieldToFind }
        End If

        Dim entUpdateContact = New Entity() {contact}
        Dim respUpdateContact As ATWSResponse = Me._atwsServices.update(entUpdateContact)
        If respUpdateContact.ReturnCode = -1 Then
            Throw New Exception("Could not update the Contact: " + respUpdateContact.EntityReturnInfoResults(0).Message)
        End If
        If respUpdateContact.ReturnCode > 0 AndAlso respUpdateContact.EntityResults.Length > 0 Then
            retContact = DirectCast(respUpdateContact.EntityResults(0), Contact)
        End If

        Return retContact
    End Function

    ''' <summary>
    ''' This method will update a UDF Picklist for a given Contact.
    ''' </summary>
    ''' <param name="contact">Contact to update.</param>
    ''' <param name="udfName">UDF Name to update.</param>
    ''' <param name="udfPicklistLabel">UDF Picklist Label to update</param>
    ''' <returns>Returns the updated Contact.</returns>
    Public Function UpdateContactUdfPicklist(contact As Contact, udfName As String, udfPicklistLabel As String) As Contact
        Dim retContact As Contact = Nothing
        Dim fldFieldToFind As UserDefinedField = Nothing
        fldFieldToFind = FindUserDefinedField(contact.UserDefinedFields, UDFName)
        Dim fieldsUdfContact As Field() = Me._atwsServices.getUDFInfo("Contact")
        Dim strUdfPickListValue As String = PickListValueFromField(fieldsUdfContact, UDFName, UDFPicklistLabel)
        If fldFieldToFind IsNot Nothing Then            
            fldFieldToFind.Value = strUdfPickListValue
        Else
            fldFieldToFind = New UserDefinedField With { .Name = udfName, .Value = strUdfPickListValue }
            contact.UserDefinedFields = New UserDefinedField(){ fldFieldToFind }
        End If

        Dim entUpdateContact = New Entity() {contact}
        Dim respUpdateContact As ATWSResponse = Me._atwsServices.update(entUpdateContact)
        If respUpdateContact.ReturnCode = -1 Then
            Throw New Exception("Could not update the Contact: " + respUpdateContact.EntityReturnInfoResults(0).Message)
        End If
        If respUpdateContact.ReturnCode > 0 AndAlso respUpdateContact.EntityResults.Length > 0 Then
            retContact = DirectCast(respUpdateContact.EntityResults(0), Contact)
        End If

        Return retContact
    End Function

    ''' <summary>
    ''' Creates a new Contact.
    ''' </summary>
    ''' <param name="accountId">Account ID of account to create contact.</param>
    ''' <param name="firstName">Contact first name.</param>
    ''' <param name="lastName">Contact last name.</param>
    ''' <param name="email">Contact email address.</param>
    ''' <param name="udfName">UDF Name to set.</param>
    ''' <param name="udfValue">UDF Value to set</param>
    ''' <returns>>Returns the new Contact.</returns>
    Public Function CreateContact(accountId As Long, firstName As String, lastName As String, email As String, udfName As String, udfValue As String) As Contact
        Dim retContact As Contact = Nothing

        ' Time to create the Contact
        Dim contactAct As New Contact With {
            .AccountID = accountID,
            .FirstName = firstName,
            .LastName = lastName,
            .EMailAddress = email.ToLower(),
            .Active = "1"
        }

        Dim fieldsUdfContact As Field() = Me._atwsServices.getUDFInfo("Contact")

        ' Create a container to hold all the UDF's
        Dim udfContainer As New List(Of UserDefinedField)()

        Dim fldFieldToFind As Field = FindField(fieldsUdfContact, UDFName)
        If fldFieldToFind IsNot Nothing Then
            Dim userDefinedField As New UserDefinedField With {
                .Name = UDFName,
                .Value = UDFValue
            }
            udfContainer.Add(userDefinedField)
        End If

        ' Time to add the UDF's from the container to the Contact
        contactAct.UserDefinedFields = udfContainer.ToArray()

        Dim entContact = New Entity() {contactAct}

        Dim respContact As ATWSResponse = Me._atwsServices.create(entContact)
        If respContact.ReturnCode > 0 AndAlso respContact.EntityResults.Length > 0 Then
            retContact = DirectCast(respContact.EntityResults(0), Contact)
        Else
            If respContact.EntityReturnInfoResults.Length > 0 Then
                Throw New Exception("Could not create the Contact: " + respContact.EntityReturnInfoResults(0).Message)
            End If
        End If

        Return retContact
    End Function

    ''' <summary>
    ''' Returns the value of a picklistitem given an array of fields and the field name that contains the picklist
    ''' </summary>
    ''' <param name="fields">array of Fields that contains the field to find</param>
    ''' <param name="strField">name of the Field to search in</param>
    ''' <param name="strPickListName">name of the picklist value to return the value from</param>
    ''' <returns>value of the picklist item to search for</returns>
    Protected Shared Function PickListValueFromField(fields As Field(), strField As String, strPickListName As String) As String
        Dim strRet As String = String.Empty

        Dim fldFieldToFind As Field = FindField(fields, strField)
        If fldFieldToFind Is Nothing Then
            Throw New Exception("Could not get the " & strField & " field from the collection")
        End If
        Dim plvValueToFind As PickListValue = FindPickListLabel(fldFieldToFind.PicklistValues, strPickListName)
        If plvValueToFind IsNot Nothing Then
            strRet = plvValueToFind.Value
        End If

        Return strRet
    End Function

    ''' <summary>
    ''' Returns the label of a picklist when the value is sent
    ''' </summary>
    ''' <param name="fields">entity fields</param>
    ''' <param name="strField">picklick to choose from</param>
    ''' <param name="strPickListValue">value ("id") of picklist</param>
    ''' <returns>picklist label</returns>
    Protected Shared Function PickListLabelFromValue(fields As Field(), strField As String, strPickListValue As String) As String
        Dim strRet As String = String.Empty

        Dim fldFieldToFind As Field = FindField(fields, strField)
        If fldFieldToFind Is Nothing Then
            Throw New Exception("Could not get the " & strField & " field from the collection")
        End If
        Dim plvValueToFind As PickListValue = FindPickListValue(fldFieldToFind.PicklistValues, strPickListValue)
        If plvValueToFind IsNot Nothing Then
            strRet = plvValueToFind.Label
        End If

        Return strRet
    End Function

    ''' <summary>
    ''' Used to find a specific Field in an array based on the name
    ''' </summary>
    ''' <param name="field">array containing Fields to search from</param>
    ''' <param name="name">contains the name of the Field to search for</param>
    ''' <returns>Field match</returns>
    Protected Shared Function FindField(field As Field(), name As String) As Field
        Return Array.Find(field, Function(element) element.Name = name)
    End Function

    ''' <summary>
    ''' Used to find a specific value in a picklist
    ''' </summary>
    ''' <param name="pickListValue">array of PickListsValues to search from</param>
    ''' <param name="name">contains the name of the PickListValue to search for</param>
    ''' <returns>PickListValue match</returns>
    Protected Shared Function FindPickListLabel(pickListValue As PickListValue(), name As String) As PickListValue
        Return Array.Find(pickListValue, Function(element) element.Label = name)
    End Function

    ''' <summary>
    ''' Used to find a specific value in a picklist
    ''' </summary>
    ''' <param name="pickListValue">array of PickListsValues to search from</param>
    ''' <param name="valueId">contains the value of the PickListValue to search for</param>
    ''' <returns>PickListValue match</returns>
    Protected Shared Function FindPickListValue(pickListValue As PickListValue(), valueId As String) As PickListValue
        Return Array.Find(pickListValue, Function(element) element.Value = valueID)
    End Function

    ''' <summary>
    ''' Used to find a specific User Defined Field in an array based on the name
    ''' </summary>
    ''' <param name="field">array containing User Defined Field to search from</param>
    ''' <param name="name">contains the name of the User Defined Field to search for</param>
    ''' <returns>UserDefinedField match</returns>
    Protected Shared Function FindUserDefinedField(field As UserDefinedField(), name As String) As UserDefinedField
        Return Array.Find(field, Function(element) element.Name = name)
    End Function
End Class

