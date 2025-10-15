<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Reports.aspx.vb" Inherits="Reports" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Reports</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Repeater ID="RptOrders" runat="server">
            <ItemTemplate>
                <div class="order">
                    Order #<%# Eval("OrderNumber") %>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <script runat="server">
            Protected Function GetStatusColor(status As String) As String
                Select Case status
                    Case "New"
                        Return "blue"
                    Case "Processing"
                        Return "orange"
                    Case "Complete"
                        Return "green"
                    Case Else
                        Return "gray"
                End Select
            End Function
        </script>
    </form>
</body>
</html>
