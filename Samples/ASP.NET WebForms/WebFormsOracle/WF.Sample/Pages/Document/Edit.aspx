<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="WF.Sample.Pages.Document.Edit"  
    MasterPageFile="~/Site.Master" Title="Edit" %>
<%@ Register TagPrefix="uc" TagName="CommandsNotAvailable" Src="~/Controls/CommandsNotAvailableControl.ascx" %>
<%@ Register TagPrefix="uc" TagName="DocumentHistoryControl" Src="~/Controls/DocumentHistoryControl.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:formview OnItemUpdating="DocumentFormView_ItemUpdating" id="DocumentFormView" SelectMethod="GetModel" 
        UpdateMethod="UpdateDocumentModel" DataKeyNames="Id" DefaultMode="Edit" RenderOuterTable="false"
        runat="server" ItemType="WF.Sample.Models.DocumentModel">
        <EditItemTemplate>
              
            <div class="ui form">

                <div class="fields">
                    <uc:CommandsNotAvailable runat="server" Model="<%# Item %>" />

                    <asp:Placeholder runat="server" Visible="<%# Item.Commands.Count() > 0 %>" >
                        <div class="field">
                            <asp:Repeater id="CommandsRepeater" DataSource="<%# Item.Commands %>" runat="server" ItemType="WF.Sample.Models.DocumentCommandModel">
                                <ItemTemplate>
                                    <asp:Button Text="<%# Item.value %>"  CssClass="<%# GetCommandButtonClass(Item.Classifier) %>"
                                                OnClick="ProcessCommandButtonClick" runat="server" CommandArgument="<%# Item.key %>" />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:Placeholder>

                    <asp:Placeholder runat="server" Visible="<%# Item.Commands.Count() > 0 || Item.AvailiableStates.Count > 0 %>" >

                        <div class="field">
                                <asp:Button id="SetStateButton" Text="Set This State" CssClass="ui button" commandname="Update" 
                                    OnClick="ProcessButtonClick" runat="server" CommandArgument="SetState" />
                        </div>
                        <div class="field">
                            <asp:DropDownList ID="StateDropDownList" runat="server" DataSource="<%# Item.AvailiableStates %>"
                                DataTextField="Value" DataValueField="Key" CssClass="ui selection dropdown">
                            </asp:DropDownList>
                        </div>
                        <div class="field">
                            <a runat="server" class="ui circular left floated button" target="_blank" style="text-align:right" 
                                href="<%# GetDesignerUrl(Item.Id) %>">Open in Workflow Designer</a>
                        </div>
                        <div class="field">
                            <a class="ui circular left floated button" target="_blank" style="text-align:right" href="<%#: Page.ResolveUrl("~/Document/AssignmentCreate?processid="+ Item.Id) %>">
                                Create assignment
                            </a>
                        </div>
                        

                    </asp:Placeholder>

                </div>

                <div class="three fields">
                    <div class='field <%= GetErrorStyle("Name") %>'>
                        <asp:Label runat="server" AssociatedControlID="NameTextBox" Text="Name" />
                        <asp:TextBox runat="server" ID="NameTextBox" Text="<%# BindItem.Name %>" />
                    </div>
   
                    <div class='field <%= GetErrorStyle("ManagerId") %>'>
                        <asp:Label runat="server" AssociatedControlID="ManagerIdDropdown" Text="Manager" />

                        <asp:DropDownList ID="ManagerIdDropdown" runat="server" 
                            SelectMethod="GetEmployees" SelectedValue="<%# BindItem.ManagerId %>"
                            AppendDataBoundItems="true"
                            DataTextField="Text" DataValueField="Value" CssClass="ui selection dropdown">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="NumberTextBox" Text="Number" />
                        <asp:TextBox ReadOnly="true" runat="server" ID="NumberTextBox" Text="<%# BindItem.Number %>" />
                    </div>
                </div>

                <div class="three fields">
                    <div class='field <%= GetErrorStyle("Sum") %>'>
                        <asp:Label runat="server" AssociatedControlID="SumTextBox" Text="Sum" />
                        <asp:TextBox runat="server" ID="SumTextBox" Text='<%# Bind("Sum", "{0:f2}") %>'  />
                    </div>

                        <div class='field'>
                        <asp:Label runat="server" AssociatedControlID="AuthorNameTextBox" Text="Author" />
                        <asp:HiddenField ID="AuthorIdHiddenField" runat="server" Value="<%# BindItem.AuthorId %>" />
                        <asp:TextBox runat="server" ReadOnly="true" ID="AuthorNameTextBox" Text="<%# BindItem.AuthorName %>" />
                    </div>

                        <div class='field'>
                        <asp:Label runat="server" AssociatedControlID="StateNameTextBox" Text="State" />
                        <asp:TextBox runat="server" ReadOnly="true" ID="StateNameTextBox" Text="<%# BindItem.StateName %>" />
                    </div>
                </div>

                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="CommentTextBox" Text="Comment" />
                    <asp:TextBox runat="server" Rows="6" Columns="100" Width="98%" TextMode="MultiLine" ID="CommentTextBox" 
                        Text="<%# BindItem.Comment %>" />
                </div>

                <div class="field">
                    <uc:DocumentHistoryControl runat="server" Model="<%# Item.HistoryModel %>" />
                </div>

                <div class="field">
                    <asp:Button id="SaveButton" Text="Save"  CssClass="ui primary button"
                        commandname="Update" OnClick="ProcessButtonClick" 
                        runat="server" CommandArgument="Save" />
                    <asp:Button id="SaveAndExitButton" Text="Save & Exit"  CssClass="ui secondary button"
                        commandname="Update" OnClick="ProcessButtonClick"
                        runat="server" CommandArgument="SaveAndExit" />
                </div>
            </div>
        </EditItemTemplate>
    </asp:formview>

</asp:Content>