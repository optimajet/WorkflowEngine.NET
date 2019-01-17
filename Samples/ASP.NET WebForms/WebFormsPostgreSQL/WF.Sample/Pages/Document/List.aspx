<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs" MasterPageFile="~/Site.Master" 
    Inherits="WF.Sample.Pages.Document.List" Title="Vacation requests" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="wfe-application-toolbar">
        <a runat="server" href="~/Document/Edit" class="ui primary button">Create</a>
        <a onclick="DeleteSelected()" href="#" class="ui secondary button">Delete</a>
    </div>

    <asp:Repeater id="DocsTableRepeater" runat="server" ItemType="WF.Sample.Models.DocumentModel">
          <HeaderTemplate>
             <table class="grid">
                <tbody>
                    <tr class="grid-header">
                        <th style="width:20px">#</th>
                        <th style="width:20px">Number</th>
                        <th>State</th>
                        <th>Name</th>
                        <th>Comment </th>
                        <th>Author</th>
                        <th>Manager</th>
                        <th>Sum</th>
                    </tr>
          </HeaderTemplate>
             
          <ItemTemplate>
            <tr ondblclick="javascript: document.location.href = '<%#: Page.ResolveUrl("~/Document/Edit/" + Item.Id) %>'">
                <td><div class="ui checkbox"><input type="checkbox" name="checkedbox" class="selectedValues" value="<%#: Item.Id %>" /><label></label></div> </td>
                <td><a href="<%#: Page.ResolveUrl("~/Document/Edit/" + Item.Id) %>"><%#: Item.Number %></a></td>
                <td><%#: Item.StateName %> </td>
                <td><a href="<%#: Page.ResolveUrl("~/Document/Edit/" + Item.Id) %>"><%#: Item.Name %></a></td>
                <td><%#: Item.Comment %> </td>
                <td><%#: Item.AuthorName %> </td>
                <td><%#: Item.ManagerName %></td>
                <td><%#: Item.Sum.ToString("n2") %></td>
            </tr>
          </ItemTemplate>
             
          <FooterTemplate>
                </tbody>
             </table>
          </FooterTemplate>
             
       </asp:Repeater>
    Current Page: <%= PageNumber + 1 %><br />
    Items count: <%= Count %> <br />
    
    <% if (PageNumber != 0) { %>
        <a href="<%= GetActionName() %>?page=0">fist page</a>
    <% } %>

    <% if (PageNumber > 0) { %>
        <a href="<%= GetActionName() %>?page=<%= PageNumber - 1 %>">prev page</a>
    <% } %>

    <% if ((PageNumber + 1) * PageSize < Count) { %>
        <a href="<%= GetActionName() %>?page=<%= PageNumber + 1 %>">next page</a>
    <% } %>

    <% if ((PageNumber + 2) * PageSize < Count) { %>
        <a href="<%= GetActionName() %>?page=<%= Count / PageSize %>">last page</a>
    <% } %>

    <script>
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
