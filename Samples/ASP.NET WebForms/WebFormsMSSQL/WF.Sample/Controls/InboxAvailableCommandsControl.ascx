<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InboxAvailableCommandsControl.ascx.cs" Inherits="WF.Sample.Controls.InboxAvailableCommandsControl" %>

<div>
    <asp:Repeater id="CommandsRepeater" runat="server" ItemType="OptimaJet.Workflow.Core.Persistence.CommandName">
        <HeaderTemplate/>
        <ItemTemplate>
            <asp:Button ID="Button1" runat="server" Text="<%#:Item.Name %>" CommandName="<%#:Item.Name %>" class="ui floated button" OnClick="ExecuteCommand" />  
        </ItemTemplate>
        <FooterTemplate/>
    </asp:Repeater>
</div>
    <script>
       function SetHref(url) {
           if (url)
            document.location.href = url;
        }
    </script>