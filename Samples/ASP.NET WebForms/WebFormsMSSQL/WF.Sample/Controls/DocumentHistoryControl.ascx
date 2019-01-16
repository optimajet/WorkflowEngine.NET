<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentHistoryControl.ascx.cs" Inherits="WF.Sample.Controls.DocumentHistoryControl" %>

<% if (Model != null && Model.Items != null && Model.Items.Count > 0) { %>

    <h1>Document's Transition History</h1>

    <asp:Repeater DataSource="<%# Model.Items %>" id="HistoryTableRepeater" runat="server" ItemType="WF.Sample.Business.Model.DocumentTransitionHistory">
        <HeaderTemplate>
           <table class="table">
            <tbody>
                <tr class="table-header">
                    <th>From</th>
                    <th>To</th>
                    <th>Command</th>
                    <th>Executor</th>
                    <th>TransitionTime</th>
                    <th>Availiable for</th>
                </tr>
        </HeaderTemplate>
             
        <ItemTemplate>
            <tr>
                <td><label><%#: Item.InitialState %></label></td>
                <td><label><%#: Item.DestinationState %></label></td>
                <td><label><%#: Item.Command %></label></td>
                <td><label><%#: Item.Employee != null ? Item.Employee.Name : "" %></label></td>
                <td><label><%#: Item.TransitionTime.HasValue ? Item.TransitionTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "" %></label></td>
                <td><label><%#: Item.AllowedToEmployeeNames %></label></td>
            </tr>
        </ItemTemplate>
             
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
             
    </asp:Repeater>

<% } %>