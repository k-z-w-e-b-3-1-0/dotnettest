<%@ Control Language="VB" AutoEventWireup="false" CodeBehind="Header.ascx.vb" Inherits="Controls_Header" %>
<div class="header">
    <h1><asp:Literal runat="server" ID="LitTitle" /></h1>
    <asp:LoginView runat="server">
        <AnonymousTemplate>
            <a href="/Account/Login">Login</a>
        </AnonymousTemplate>
        <LoggedInTemplate>
            Welcome, <asp:LoginName runat="server" />!
        </LoggedInTemplate>
    </asp:LoginView>
</div>
<script runat="server">
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        If Not Page.IsPostBack Then
            LitTitle.Text = Page.Title
        End If
    End Sub
</script>

