<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HeaderControl.ascx.cs" Inherits="WF.Sample.Controls.HeaderControl" %>

<script>
    function CurrentEmployee_OnChange(sender) {
        window.location.search = "CurrentEmployee=" + sender.value;
    }
</script>

<div class="ui form" style="min-width: 300px; float: right;">
    <div class="field">
        <label>Current employee</label>
        <asp:DropDownList DataTextField="Text" DataValueField="Value" id="EmployeeList" runat="server" 
            CssClass="ui dropdown" />
    </div>
</div>
