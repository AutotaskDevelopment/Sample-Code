<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Upload_Attachment_Example.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Attachment Upload Example</title>
    <link rel="stylesheet" type="text/css" href="~/Content/bootstrap.css" />
    <link rel="stylesheet" type="text/css" href="~/Content/bootstrap-theme.css" />
    <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />
</head>
<body>
    <br/><br/>
    <div class="container">
        <form id="form1" runat="server">
            <div class="form-group">
                <asp:Label AssociatedControlID="exampleInputEmail" runat="server">Email</asp:Label>
                <asp:TextBox ID="exampleInputEmail" CssClass="form-control" placeholder="Email" runat="server" required="requried"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Label AssociatedControlID="exampleInputFile" runat="server">File input</asp:Label>
                <asp:FileUpload ID="exampleInputFile" runat="server" required="requried" />
                <p class="help-block">Upload a file to be attached to your account.</p>
            </div>
            <asp:Button ID="submitButton" CssClass="btn btn-default" UseSubmitBehavior="true" runat="server" Text="Submit" OnClick="SubmitButtonClick" />
            <div ID="error" class="alert alert-danger" role="alert" Visible="False" runat="server"></div>            
        </form>
        <div ID="success" Visible="False" runat="server">
            <h2>Your file was successfuly uploaded to <asp:HyperLink id="accountCommand" runat="server"></asp:HyperLink></h2>
        </div>
    </div>

    <script type="text/javascript" src="Scripts/jquery-1.12.4.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.12.1.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.js"></script>
    <script>
        $(document).ready(function () {
            $.ajax({
                url: "Default.aspx/GetEmails",
                method: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $("#<%=exampleInputEmail.ClientID%>").autocomplete({
                        source: data.d
                    });
                },
                error: function() {
                    alert("Error with GetEmails ajax call");
                }
            });            
        });
    </script>
</body>
</html>
