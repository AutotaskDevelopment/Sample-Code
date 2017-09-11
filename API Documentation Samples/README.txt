Before you begin:

For both C# and Visual Basic solutions, open the app.config file and replace  API USERNAME and API PASSWORD with your own Autotask login credentials.
 
NOTE: The Web Services API respects all security level settings assigned to your Autotask username.


To use the sample solutions:

In the code samples found in the C# program.cs file or the Visual Basic Module1.vb file, replace the upper case placeholder values with the actual values. For example:

In the lines


 ' Search for a resource given a username 
 Dim resourceID As Long = test.FindResource("RESOURCE USER NAME")


Replace RESOURCE USER NAME with the actual Autotask user name of the resource you are searching for.
