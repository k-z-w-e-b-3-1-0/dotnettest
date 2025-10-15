<%@ Control Language="VB" AutoEventWireup="false" CodeBehind="Footer.ascx.vb" Inherits="Controls_Footer" %>
<div class="footer">
    <asp:Label ID="LblYear" runat="server"></asp:Label>
</div>
<script runat="server">
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        Dim currentYear As Integer = DateTime.Now.Year
        If currentYear > 2020 Then
            LblYear.Text = String.Format("© {0} Contoso", currentYear)
        Else
            LblYear.Text = "© 2020 Contoso"
        End If
    End Sub
</script>
