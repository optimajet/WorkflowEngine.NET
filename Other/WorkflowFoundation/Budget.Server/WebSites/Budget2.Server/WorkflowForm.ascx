<%@ Control Language="C#" AutoEventWireup="true" Codebehind="WorkflowForm.ascx.cs" Inherits="Budget2.Server.Shell.Views.WorkflowForm" %>
<%@ Register TagName="WorkflowButton" TagPrefix="budget" Src="~/WorkflowButtons.ascx" %>
<%@ Register assembly="DevExpress.XtraReports.v10.1.Web, Version=10.1.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.XtraReports.Web" tagprefix="dx" %>
        <asp:MultiView runat="server" ID="mvMain" ActiveViewIndex = "0">
             <asp:View runat = "server" ID="vUsed">
                    <h2><asp:Literal runat="server" ID="liUsed">Документ уже обработан.</asp:Literal></h2>
            </asp:View>
            <asp:View runat = "server" ID="vAccessDenied">
                    <h2><asp:Literal runat="server" ID="liAccessDenied">В доступе к документу отказано.</asp:Literal></h2>
            </asp:View>
             <asp:View runat = "server" ID="vAccessGranted">
                    <table>
                        <tr>
                            <td> 
                                <budget:WorkflowButton runat="server" ID="WorkflowButtonsUp" />
                            </td>                            
                        </tr>
                         <tr>
                            <td>
                                <asp:Panel runat="server" ID="pnComment">
                                    <asp:TextBox ID="tbComment" runat="server" Width="100%" Height="100px" 
                                        TextMode="MultiLine"></asp:TextBox>
                                </asp:Panel> 
                           </td>                            
                        </tr>
                        <tr>
                            <td> <!-- Отчет -->
                                <dx:ReportToolbar ID="ReportToolbar1" runat="server" 
                                    ReportViewer="<%# ReportViewer1 %>" ShowDefaultButtons="False">
                                    <Items>
                                        <dx:ReportToolbarButton ItemKind="Search" />
                                        <dx:ReportToolbarSeparator />
                                        <dx:ReportToolbarButton ItemKind="PrintReport" />
                                        <dx:ReportToolbarButton ItemKind="PrintPage" />
                                        <dx:ReportToolbarSeparator />
                                        <dx:ReportToolbarButton Enabled="False" ItemKind="FirstPage" />
                                        <dx:ReportToolbarButton Enabled="False" ItemKind="PreviousPage" />
                                        <dx:ReportToolbarLabel ItemKind="PageLabel" />
                                        <dx:ReportToolbarComboBox ItemKind="PageNumber" Width="65px">
                                        </dx:ReportToolbarComboBox>
                                        <dx:ReportToolbarLabel ItemKind="OfLabel" />
                                        <dx:ReportToolbarTextBox IsReadOnly="True" ItemKind="PageCount" />
                                        <dx:ReportToolbarButton ItemKind="NextPage" />
                                        <dx:ReportToolbarButton ItemKind="LastPage" />
                                        <dx:ReportToolbarSeparator />
                                        <dx:ReportToolbarButton ItemKind="SaveToDisk" />
                                        <dx:ReportToolbarButton ItemKind="SaveToWindow" />
                                        <dx:ReportToolbarComboBox ItemKind="SaveFormat" Width="70px">
                                            <elements>
                                                <dx:ListElement Value="pdf" />
                                                <dx:ListElement Value="xls" />
                                                <dx:ListElement Value="xlsx" />
                                                <dx:ListElement Value="rtf" />
                                                <dx:ListElement Value="mht" />
                                                <dx:ListElement Value="html" />
                                                <dx:ListElement Value="txt" />
                                                <dx:ListElement Value="csv" />
                                                <dx:ListElement Value="png" />
                                            </elements>
                                        </dx:ReportToolbarComboBox>
                                    </Items>
                                    <styles>
                                        <LabelStyle>
                                        <Margins MarginLeft="3px" MarginRight="3px" />
                                        </LabelStyle>
                                    </styles>
                                </dx:ReportToolbar>
                                <dx:ReportViewer ID="ReportViewer1" runat="server">
                                </dx:ReportViewer>
                            </td>                            
                        </tr>
                         <tr>
                            <td> 
                               <asp:Label ID="lbError" runat="server" ForeColor="Red" Visible="False"></asp:Label>
                            </td>                            
                        </tr>
                        <tr>
                            <td> 
                                <budget:WorkflowButton runat="server" ID="WorkflowButtons" />
                            </td>                            
                        </tr>
                        </table>
            </asp:View>
            <asp:View runat = "server" ID="vComplete">
                    <h2><asp:Literal runat="server" ID="Literal1">Утверждение завершено успешно.</asp:Literal></h2>
            </asp:View>
        </asp:MultiView>

