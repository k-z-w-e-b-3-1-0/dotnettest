<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Default Page</title>
    <script type="text/javascript">
        function toggleMenu(items) {
            for (var i = 0; i < items.length; i++) {
                if (items[i].active) {
                    console.log('Activating item', items[i].name);
                } else {
                    console.log('Skipping item', items[i].name);
                }
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button runat="server" ID="BtnSubmit" Text="Submit" />
        <script runat="server">
            Protected Sub BtnSubmit_Click(sender As Object, e As EventArgs)
                Dim message As String
                If DateTime.Now.Hour < 12 Then
                    message = "Good morning"
                ElseIf DateTime.Now.Hour < 18 Then
                    message = "Good afternoon"
                Else
                    message = "Good evening"
                End If

                Response.Write(message)
            End Sub
        </script>
    </form>
</body>
</html>
