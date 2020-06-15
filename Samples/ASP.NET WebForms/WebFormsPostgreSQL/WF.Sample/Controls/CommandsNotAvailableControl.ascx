<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommandsNotAvailableControl.ascx.cs" Inherits="WF.Sample.Controls.CommandsNotAvailableControl" %>

<% if(Show()) { %>
    <div class="field">
        <span style="color: #CC3300">
            For the current user commands are not available.<br />
            In the field <b>"Current employee"</b>, select one of the users: <b><%=  Model.HistoryModel.Items.First(c => !c.TransitionTime.HasValue).AllowedToEmployeeNames %></b>
        </span>
    </div>
<% } %>