<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Outbox.aspx.cs" MasterPageFile="~/Site.Master" 
    Inherits="WF.Sample.Pages.Document.Outbox" Title="Vacation requests" %>
<%@ Import Namespace="WF.Sample.Models" %>

<%@ Register TagPrefix="uc" TagName="PagingControl" Src="~/Controls/Paging.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="wfe-application-toolbar">
        <a runat="server" href="~/Document/Edit" class="ui primary button">Create</a>
        <a onclick="DeleteSelected()" href="#" class="ui secondary button">Delete</a>
    </div>
       
    <asp:Repeater id="DocsTableRepeater" runat="server" ItemType="WF.Sample.Models.OutboxDocumentModel">
          <HeaderTemplate>
             <table class="grid">
                <tbody>
                    <tr class="grid-header">
                        <th style="width:20px"></th>
                        <th style="width:20px">Number</th>
                        <th>State</th>
                        <th>Name</th>
                        <th>Comment </th>
                        <th>Author</th>
                        <th>Manager</th>
                        <th>Sum</th>
                        <th>Approval count</th>
                        <th>First approval time</th>
                        <th>Last approval time</th>
                        <th>Last approval</th>
                    </tr>
          </HeaderTemplate>
           
          <ItemTemplate>
              <tr ondblclick="SetHref('<%#: GetEditUrl(Item) %>')">
                <td><div class="ui checkbox"><input type="checkbox" name="checkedbox" class="selectedValues" value="<%#: Item.Id %>" /><label></label></div> </td>
                <td><a href="<%#: Page.ResolveUrl("~/Document/Edit/" + Item.Id) %>"><%#: Item.Number %></a></td>
                <td><%#: Item.StateName %> </td>
                <td><a href="<%#: Page.ResolveUrl("~/Document/Edit/" + Item.Id) %>"><%#: Item.Name %></a></td>
                <td><%#: Item.Comment %> </td>
                <td><%#: Item.AuthorName %> </td>
                <td><%#: Item.ManagerName %></td>
                <td><%#: Item.Sum.ToString("n2") %></td>
                <td><%#: Item.ApprovalCount %></td>
                <td><%#: Item.FirstApprovalTime %></td>
                <td><%#: Item.LastApprovalTime %></td>
                  <td><%#: Item.LastApproval %></td>
            </tr>
          </ItemTemplate>
            
          <FooterTemplate>
                </tbody>
             </table>
          </FooterTemplate>
             
       </asp:Repeater>
    <uc:PagingControl id="PagingControl" Model="<%# this %>" runat="server" />
    
    <script>
           function SetHref(url) {
               if (url)
                document.location.href = url;
            }
        function DeleteSelected() {
            var data = new Array();
            var selectedValues = $('.selectedValues:checked');

            if (selectedValues.length < 1) {
                alert('Please, select rows for deleting!');
                return;
            }

            for (var i = 0; i < selectedValues.length; i++) {
                data.push({ name: 'ids', value: selectedValues[i].value });
            }

            $.ajax({
                type: "POST",
                url: '<%= Page.ResolveUrl("~/Document/DeleteRows") %>',
                data: data,
                success: function(msg) {
                    alert(msg);
                    location.reload();
                }
            });
        }
    </script>

</asp:Content>
