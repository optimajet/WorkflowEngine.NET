<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Assignments.aspx.cs" Inherits="WF.Sample.Pages.Document.Assignments"  
    MasterPageFile="~/Site.Master" Title="Vacation requests" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server" ItemType="WF.Sample.Models.AssignmentListModel">
    <div class="wfe-application-toolbar">
        <div class="ui form" style="min-width: 300px; width: 300px; display: flex">
            <asp:formview ID="formFilter" DefaultMode="Edit" SelectMethod="GetFilterModel" RenderOuterTable="false" runat="server" ItemType="WF.Sample.Models.AssignmentFilterModel" style="display: flex">
                <EditItemTemplate>
                    <div style="min-width: 300px; margin-right: 10px">
                        <label>Filters</label><br />
                        <div class="field">
                            <asp:DropDownList 
                                ID="FilterName" name="FilterName" 
                                runat="server" SelectMethod="GetFilters"
                                AppendDataBoundItems="true"  
                                DataTextField="Key" DataValueField="Key"
                                SelectedValue="<%# BindItem.FilterName %>" 
                                CssClass="ui selection dropdown">
                            </asp:DropDownList>
                        </div>
                    </div>
                <div style="min-width:150px;margin-right: 10px">
                    <asp:Label runat="server" AssociatedControlID="DocumentNumber"  Text="Document number" />
                    <asp:TextBox runat="server" class="ui" type="number" ID="DocumentNumber" Text="<%# BindItem.DocumentNumber %>" />
                </div>
                <div style="min-width:150px;margin-right: 10px">
                    <asp:Label runat="server" AssociatedControlID="AssignmentCode" Text="Assignment code" />
                    <asp:TextBox runat="server" class="ui" ID="AssignmentCode" Text="<%# BindItem.AssignmentCode %>" />
                </div>
                <div style="min-width: 200px; margin-right: 10px">
                    <label>Status</label><br />
                    <div class="field">
                         <asp:DropDownList 
                             ID="StatusState" AppendDataBoundItems="true" 
                             runat="server" SelectedValue="<%# BindItem.StatusState %>" 
                             SelectMethod="GetStatuses"  CssClass="ui selection dropdown">
                         </asp:DropDownList>
                    </div>
                </div>
                     
                <asp:TextBox style="display: none" runat="server" class="ui" ID="Page" Text="<%# BindItem.Page %>" />
                     
                <input type="submit" class="ui primary button" style="margin-top: 20px; margin-right: 10px" value="Find"/>
                <button type="button" onclick="DeleteSelected()" style="margin-top: 20px"  class="ui secondary button">Delete</button>
                </EditItemTemplate>
            </asp:formview>
        </div>
    </div>
    
    <asp:Repeater id="AssignmentsTableRepeater"   runat="server" ItemType="WF.Sample.Models.AssignmentItemModel">
        <HeaderTemplate>
            <table class="grid">
            <tr class="grid-header">
                <th style="width: 20px"></th>
                <th style="min-width: 40px">Number-Code</th>
                <th>Name</th>
                <th>Status</th>
                <th>Executor</th>
                <th>Date creation</th>
                <th>State</th>
            </tr>
        </HeaderTemplate>
               
        <ItemTemplate>
            <tr>
                <td><div class="ui checkbox"><input type="checkbox" class="selectedValues" value="<%#: Item.AssignmentId %>" /><label></label></div> </td>
                <td><a href="<%#: Page.ResolveUrl("~/Document/AssignmentInfo/" + Item.AssignmentId) %>"><%#:Item.DocumentNumber%>-<%#:Item.AssignmentCode%></a></td>
                <td style="max-width: 500px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap"><%#: Item.Name %></td>
                <td><%#: Item.StatusState%></td>
                <td><%#: Item.ExecutorName %></td>
                <td><%#: Item.DateCreation %> </td> 
                <td style="width: 100px">
                    <span style="<%#: GetAssignmentStateStyle(Item) %>"  ><%#:  GetAssignmentStateText(Item) %></span>
                </td>
            </tr>
        </ItemTemplate>

        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    
    Current Page: <%= PageNumber%><br />
    Items count: <%= Count %> <br />
        
    <% if (PageNumber != 1) { %>
        <a style="cursor: pointer" onclick="getPage(1)">fist page</a>
        <a>|</a>
    <% } %>
        
    <% if (PageNumber > 1) { %>
        <a style="cursor: pointer" onclick="getPage(<%=(PageNumber - 1)%>)">prev page</a>
        <a>|</a>
    <% } %>
        
    <% if (PageNumber < LastPage) { %>
        <a style="cursor: pointer" onclick="getPage(<%=(PageNumber+ 1)%>)">next page</a>
        <a>|</a>
    <% } %>
        
    <% if (PageNumber != LastPage) { %>
        <a style="cursor: pointer" onclick="getPage(<%=(LastPage)%>)">last page</a>
    <% } %>
    
    <script>
        function getPage(pageIndex)
        {
            document.getElementById('MainContent_formFilter_Page').value = pageIndex;
            document.getElementById('ctl01').submit();
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
                url: '<%= Page.ResolveUrl("~/Document/DeleteAssignments") %>',
                data: data,
                success: function(msg) {
                    alert("Deleted");
                       location.reload();
                    },
                error: function (){
                    alert("error");
                    }
            });
        }     
    </script>
</asp:Content>