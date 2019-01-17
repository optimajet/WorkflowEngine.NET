<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FoldersControl.ascx.cs" Inherits="WF.Sample.Controls.FoldersControl" %>

<div style="width: 300px; float: left;">
    <div class="ui menu">
        <a runat="server" href="~/" class="item"><%= (Folder == 0 ? "<b>All vacation requests</b>" : "All vacation requests") %></a>
        <a runat="server" href="~/Document/Inbox" class="item"><%= (Folder == 1 ? "<b>Inbox</b>" : "Inbox") %></a>
        <a runat="server" href="~/Document/Outbox" class="item"><%= (Folder == 2 ? "<b>Outbox</b>" : "Outbox") %></a>
    </div>
</div>