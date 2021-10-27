<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AssignmentInfo.aspx.cs" Inherits="WF.Sample.Pages.Document.AssignmentInfo"  
    MasterPageFile="~/Site.Master" Title="Vacation requests" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server" ItemType="WF.Sample.Models.AssignmentInfoModel">
<div class="ui form" style="min-width: 300px;">
    <asp:formview ID="AssignmentForm"  UpdateMethod="UpdateAssignmentModel" DataKeyNames="AssignmentId"  DefaultMode="Edit" SelectMethod="GetAssignmentModel" RenderOuterTable="false" runat="server" ItemType="WF.Sample.Models.AssignmentInfoModel" style="display: flex">
        <EditItemTemplate>
                             
        <div style="display: flex; margin-bottom: 15px">
            <div style="flex: auto; margin-right: 30px">
                <asp:TextBox style="display: none" runat="server" class="ui" ID="Page" Text="<%# BindItem.AssignmentId %>" />
                
                <div class="field <%= GetErrorStyle("Name") %>" id="formName" style="font-size: 20px">
                    <asp:Label runat="server" AssociatedControlID="Name" Text="Name" />
                    <asp:TextBox runat="server" class="ui" ID="Name" Text="<%# BindItem.Name %>" />
                </div>

                <div style="display: flex">
                    <div class="field" style="width: 50%; margin-right: 15px">
                        <asp:Label runat="server" AssociatedControlID="DeadlineToStart" Text="Deadline to start" />
                        <asp:TextBox runat="server" TextMode="DateTimeLocal"  style="width:100%;padding-left:20px" class="ui form-control" ID="DeadlineToStart" Text="<%#BindItem.DeadlineToStart%>"  />
                    </div>
                    <div class="field" style="width: 50%">
                        <asp:Label runat="server" AssociatedControlID="DeadlineToComplete" Text="Deadline to complete" />
                        <asp:TextBox runat="server" TextMode="DateTimeLocal" style="width:100%;padding-left:20px" class="ui form-control" ID="DeadlineToComplete" Text="<%# BindItem.DeadlineToComplete %>" />
                    </div>
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="Description" Text="Description" />
                    <asp:TextBox runat="server" TextMode="multiline" style="width:100%;padding-left:20px; min-height: 200px;" class="ui" ID="Description" Text="<%# BindItem.Description %>" />
                </div>
            </div>
        
            <div style="min-width: 350px; max-width: 350px; background: #F4F4F4; padding: 15px; border-radius: 10px">
                <% if (ForCreate){ %>
                    <div class="field <%= GetErrorStyle("AssignmentCode") %>" id="formCode">
                        <asp:Label runat="server" AssociatedControlID="AssignmentCode" Text="Assignment code" />
                        <asp:TextBox runat="server" class="ui" ID="AssignmentCode" Text="<%# BindItem.AssignmentCode %>" />
                    </div>
                <% }%>
                <% else {%>
                    <div style="display: flex; margin-bottom: 15px">
                        <div style="width: 50%">Assignment code</div>
                        <div style="width: 50%"><%# Item.AssignmentCode %></div>
                    </div>
                <% }%>
                <div style="display: flex; margin-bottom: 15px">
                    <div style="width: 50%">Document number</div>
                    <div style="width: 50%"><a href="<%#: Page.ResolveUrl("~/Document/Edit/" + Item.ProcessId) %>"><%# Item.DocumentNumber %></a></div>
                </div>
                
                <div style="display: flex; margin-bottom: 15px">
                    <div style="width: 50%">Assignment state</div>
                    <div style="width: 50%">
                        <% if (IsDeleted){%>
                            <span style="color: red">Deleted</span>
                        <% }%>
                        <% else if(IsActive){%>
                            <span style="color: green">Active</span>
                        <% }%>
                        <% else{%>
                            <span style="color: orange">Not Active</span>
                        <% }%>
                    </div>
                </div>
                
                <% if (!ForCreate){%>
                    <div style="display: flex; margin-bottom: 15px">
                        <div style="width: 50%">Date creation</div>
                        <div style="width: 50%"><%# Item.DateCreation %></div>
                    </div>
                <%}%>

                <% if (DateStart != null){ %>
                    <div style="display: flex; margin-bottom: 15px">
                        <div style="width: 50%">Date start</div>
                        <div style="width: 50%"><%# Item.DateStart %></div>
                    </div>
                <%}%>

                <%if (DateFinish != null){%>
                <div style="display: flex; margin-bottom: 15px">
                        <div style="width: 50%">Date finish</div>
                        <div style="width: 50%"><%# Item.DateFinish %></div>
                    </div>
                <%}%>
                <%if (!ForCreate){%>
                    <div class="field ">
                        <label>Status</label>
                        <%if (IsActive && !IsDeleted){%>
                            <asp:DropDownList ID="StatusState" AppendDataBoundItems="true"
                                              runat="server"  DataTextField="Value" DataValueField="Key" 
                                              SelectedValue="<%# BindItem.StatusState %>"
                                              SelectMethod="GetStatuses"  CssClass="ui selection dropdown">
                               <asp:ListItem Value=""></asp:ListItem>
                            </asp:DropDownList>
                        <%}%>
                        <%else{%>
                             <asp:TextBox runat="server" class="ui" ID="StatusStateReadonly" Text="<%# Item.StatusState %>"  ReadOnly="True"/>
                        <%}%>
                    </div>
                <%}%>

                <div class="field <%= GetErrorStyle("Executor") %>">
                    <label>Executor</label>
                    <%if (IsActive && !IsDeleted){%>
                        <asp:DropDownList ID="Executor" AppendDataBoundItems="true" 
                                          runat="server" DataTextField="Value"
                                          DataValueField="Key" SelectedValue="<%# BindItem.Executor %>"
                                          SelectMethod="GetExecutors" CssClass="ui selection dropdown">
                           <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    <%}%>
                    <%else{%>
                         <asp:TextBox runat="server" class="ui" ID="ExecutorName" Text="<%# Item.ExecutorName %>"  ReadOnly="True"/>
                    <%}%>
                </div>
               
                <div style="display: flex; margin-bottom: 15px">
                    <div style="width: 50%">Observers</div>
                    <div style="width: 50%" ><%#: GetValue(String.Join(", ", Item.Observers.Select(x => x.Value)))%></div>
                </div>
                <div style="display: flex; margin-bottom: 15px">
                    <div style="width: 50%">Tags</div>
                    <div style="width: 50%"><%#: GetValue(String.Join(", ", Item.Tags)) %></div>
                </div>
            </div>
        </div>
        <div class="field">
            <asp:TextBox runat="server" style="display: none" class="ui" ID="FormAction" Text="<%# BindItem.FormAction %>" />
            <asp:TextBox runat="server" style="display: none" class="ui" ID="ProcessId" Text="<%# BindItem.ProcessId %>" />
            
            <%if (IsActive && !IsDeleted){%>
                <%if (ForCreate){%>
                    <asp:Button id="CreateButton" Text="Create"  CssClass="ui primary button" commandname="Update" runat="server" />
                    <a href="<%#: Page.ResolveUrl("~/Document/Edit/" + Item.ProcessId) %>" class="ui secondary button" >Cancel</a>
                <%}%>
                <%else{%>
                    <asp:Button id="SaveButton" Text="Save" CssClass="ui primary button" commandname="Update" runat="server" />
                    <a href="<%#: Page.ResolveUrl("~/Document/Assignments") %>" class="ui secondary button" >Cancel</a>
                <%}%>
            <%}%>
            
            <%if (!ForCreate){%>
                <a href="<%#: Page.ResolveUrl("~/Document/DeleteAssignments?ids="+Item.AssignmentId) %>" class="ui primary button">Delete assignment</a>
            <%}%>
        </div>
        </EditItemTemplate>
    </asp:formview>
</div>
    
</asp:Content>

