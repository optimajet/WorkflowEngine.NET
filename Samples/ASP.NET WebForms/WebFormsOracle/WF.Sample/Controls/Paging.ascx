<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Paging.ascx.cs" Inherits="WF.Sample.Controls.Paging" %>

Current Page: <%= Model.PageNumber%><br />
    Items count: <%= Model.Count %> <br />
    
    <% if (Model.PageNumber != 1) { %>
        <a href="<%= GetActionName() %>?page=1">fist page</a>
         <a>|</a>
    <% } %>
    
    <% if (Model.PageNumber > 1) { %>
        <a href="<%= GetActionName() %>?page=<%= Model.PageNumber - 1 %>">prev page</a>
         <a>|</a>
    <% } %>
    
    <% if (Model.PageNumber < LastPage) { %>
        <a href="<%= GetActionName() %>?page=<%= Model.PageNumber + 1 %>">next page</a>
         <a>|</a>
    <% } %>
    
    <% if (Model.PageNumber != LastPage) { %>
        <a href="<%= GetActionName() %>?page=<%= LastPage%>">last page</a>
    <% } %>
    