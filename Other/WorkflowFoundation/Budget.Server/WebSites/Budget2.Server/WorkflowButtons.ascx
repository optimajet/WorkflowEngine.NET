<%@ Control Language="C#" AutoEventWireup="true" Codebehind="WorkflowButtons.ascx.cs" Inherits="Budget2.Server.Shell.Views.WorkflowButtons" %>
<asp:Table runat="server" ID="buttonTable">
    <asp:TableRow>
        <asp:TableCell runat="server" ID="cSign">
            <asp:Button runat="server" ID="btnSign" Text="Утвердить" OnClick="cSign_Click" />
        </asp:TableCell>
         <asp:TableCell runat="server" ID="cDenial">
             <asp:Button runat="server" ID="btnDeny" Text="Отказать"   OnClick="cDenial_Click" />
        </asp:TableCell>
          <asp:TableCell runat="server" ID="cDenialByTechnicalCauses">
            <asp:Button runat="server" ID="Button1" Text="Отказать по ТП"  OnClick="cDenialByTechnicalCauses_Click" />
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>

