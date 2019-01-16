<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="WF.Sample.Pages.Settings.Edit" 
    MasterPageFile="~/Site.Master" Title="Settings" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Settings</h2>

    <table class="table">
    <tbody>
        <tr>
            <th>Param</th>
            <th>Value</th>
        </tr>
        <tr>
            <td><code>Workflow Scheme</code></td>
            <td>
                <div>
                    <a runat="server" class="ui primary button" target="_blank" href="~/Designer">Open in Designer</a>
                </div>
            </td>
        </tr>
        <tr>
            <td><code>Roles</code></td>
            <td>
                
                <asp:Repeater id="RolesTableRepeater" DataSource="<%# Model.Roles %>" runat="server" 
                    ItemType="WF.Sample.Business.Model.Role">
                  <HeaderTemplate>
                    <table class="table">
                        <tbody>
                        <tr>
                            <th style="width:20px">Number</th>
                            <th>Name</th>
                        </tr>
                  </HeaderTemplate>
             
                  <ItemTemplate>
                    <tr>
                        <td><%# Container.ItemIndex + 1 %></td>
                        <td><%#: Item.Name %></td>
                    </tr>
                  </ItemTemplate>
             
                  <FooterTemplate>
                     </table>
                  </FooterTemplate>
             
               </asp:Repeater>

            </td>
        </tr>
        <tr>
            <td><code>StructDivisions</code></td>
            <td>
                 <asp:PlaceHolder runat="server">
                    <%: Scripts.Render("~/Scripts/jquery.treeTable.min.js") %>
                    <%: Styles.Render("~/Content/themes/base/jquery.treeTable.css") %>
                </asp:PlaceHolder>
                
                <style>
                    table.table td.columnTree
                    {
                        padding-left: 20px;   
                    }    
                </style>

            
                <asp:Repeater id="StructDivisionRepeater" runat="server" 
                    ItemType="WF.Sample.Business.Model.StructDivision">
                    <HeaderTemplate>
                        <table id='SDTable' class="table">
                            <tbody>
                                <tr>
                                    <th>Name</th>
                                    <th>Roles</th>
                                </tr>
                    </HeaderTemplate>
             
                    <ItemTemplate>
                      <%# GenerateColumnsHtml("Columns", Item, null) %>
                    </ItemTemplate>
             
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
             
               </asp:Repeater>

                <script>
                    $(document).ready(function () {
                        $('#SDTable').treeTable(
                            {
                                initialState: "expanded"
                            }
                        );
                    });
                </script>

            </td>
        </tr>
    </tbody>
</table>
</asp:Content>