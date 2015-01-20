<%@ Page Title="" Language="C#" MasterPageFile="~/Shared/DefaultMaster.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Budget2.Server.Demand.Default" %>
<%@ Register TagName="WorkflowForm" TagPrefix="budget" Src="~/WorkflowForm.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="DefaultContent" runat="server">
    <budget:WorkflowForm runat="server" Id="wForm"></budget:WorkflowForm>
</asp:Content>
