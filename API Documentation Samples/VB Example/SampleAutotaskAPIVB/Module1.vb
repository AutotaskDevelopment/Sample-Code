Imports System.Configuration
Imports SampleAutotaskAPIVB.Autotask.Net.Webservices

Module Module1

	Sub Main()

		Try
			Dim test As New AutotaskAPITests(ConfigurationManager.AppSettings("APIUsername"), ConfigurationManager.AppSettings("APIPassword"))

            ' Search for a resource given a username
            Dim resourceId As Long = test.FindResource("RESOURCE USER NAME")

            ' Search for a Contact given an email address
            Dim contact As Contact = test.FindContact("CONTACT EMAIL ADDRESS")

            ' Update Contact UDF
            contact = test.UpdateContactUDF(contact, "UDF NAME", "UDF VALUE")

            ' Update Contact Picklist UDF
            contact = test.UpdateContactUDFPicklist(contact, "UDF NAME", "UDF PICKLIST LABEL")

            ' Create NEW Contact with UDF
            Dim newContact As Contact = test.CreateContact(Convert.ToInt64(contact.AccountID), "FIRST NAME", "LAST NAME", "EMAIL", "UDF NAME", "UDF VALUE")

        Catch ex As Exception
			Throw New Exception("Error: " & ex.Message)
		End Try

	End Sub

End Module
