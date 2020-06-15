﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="WF.Sample.Pages.Designer.Index" 
    MasterPageFile="~/Site.Master" Title="Designer" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

     <asp:PlaceHolder runat="server">
        <%: Scripts.Render("~/Scripts/konva.min.js") %>
        <%: Scripts.Render("~/Scripts/ace.js") %>
        <%: Scripts.Render("~/Scripts/semantic.min.js") %>
        <%: Scripts.Render("~/Scripts/workflowdesigner.min.js") %>
        <%: Scripts.Render("~/Scripts/json5.js") %>
        <%: Scripts.Render("~/Scripts/jquery.auto-complete.min.js") %>
    </asp:PlaceHolder>

    <div style="padding-bottom: 8px;">
        <div>
            <a href="javascript:OnNew()" class="ui secondary button">New scheme</a>
            <a href="javascript:OnSave()" class="ui secondary button">Save scheme</a>
            <a href="javascript:DownloadScheme()" class="ui primary button">Download XML</a>
            <a href="javascript:SelectScheme('wfe')" class="ui secondary button">Upload XML</a>
            <a href="javascript:DownloadSchemeBPMN()" class="ui secondary button">Download BPMN2</a>
            <a href="javascript:SelectScheme('bpmn')" class="ui secondary button">Upload BPMN2</a>
        </div>
        <input type="file" name="uploadFile" id="uploadFile" style="display:none" onchange="javascript: UploadScheme();">
    </div>
    <div id="wfdesigner" style="min-height:600px"></div>

    <script>
        var QueryString = function () {
            // This function is anonymous, is executed immediately and
            // the return value is assigned to QueryString!
            var query_string = {};
            var query = window.location.search.substring(1);
            var vars = query.split("&");
            for (var i = 0; i < vars.length; i++) {
                var pair = vars[i].split("=");
                // If first entry with this name
                if (typeof query_string[pair[0]] === "undefined") {
                    query_string[pair[0]] = pair[1];
                    // If second entry with this name
                } else if (typeof query_string[pair[0]] === "string") {
                    var arr = [query_string[pair[0]], pair[1]];
                    query_string[pair[0]] = arr;
                    // If third or later entry with this name
                } else {
                    query_string[pair[0]].push(pair[1]);
                }
            }
            return query_string;
        }();

        var schemecode = 'SimpleWF';
        var processid = QueryString.processid;
        var graphwidth = 1200;
        var graphminheight = 600;
        var graphheight = graphminheight;

        var wfdesigner = undefined;
        function wfdesignerRedraw() {
            var data;

            if (wfdesigner != undefined) {
                data = wfdesigner.data;
                wfdesigner.destroy();
            }

            WorkflowDesignerConstants.FormMaxHeight = 600;
            wfdesigner = new WorkflowDesigner({
                name: 'simpledesigner',
                apiurl: '<%= Page.ResolveUrl("~/Designer/API") %>',
                renderTo: 'wfdesigner',
                imagefolder: '<%= Page.ResolveUrl("~/Images/") %>',
                graphwidth: graphwidth,
                graphheight: graphheight
            });

            if (data == undefined) {
                var isreadonly = false;
                if (processid != undefined && processid != '')
                    isreadonly = true;

                var p = { schemecode: schemecode, processid: processid, readonly: isreadonly };
                if (wfdesigner.exists(p))
                    wfdesigner.load(p);
                else
                    wfdesigner.create(schemecode);
            }
            else {
                wfdesigner.data = data;
                wfdesigner.render();
            }
        }

       $(window).resize(function () {
            if (window.wfResizeTimer) {
                clearTimeout(window.wfResizeTimer);
                window.wfResizeTimer = undefined;
            }
            window.wfResizeTimer = setTimeout(function () {
                var w = $(window).width();
                var h = $(window).height();

                if (w > 300)
                    graphwidth = w - 40;

                if (h > 300)
                    graphheight = h - 250;

                if (graphheight < graphminheight)
                    graphheight = graphminheight;

                wfdesignerRedraw();
            }, 150);

        });

        $(window).resize();

    function DownloadScheme(){
        wfdesigner.downloadscheme();
    }

    function DownloadSchemeBPMN() {
        wfdesigner.downloadschemeBPMN();
    }

    var selectSchemeType;
    function SelectScheme(type) {
        if (type)
            selectSchemeType = type;

        var file = $('#uploadFile');
        file.trigger('click');
    }

    function UploadScheme() {

        if (selectSchemeType == "bpmn") {
            wfdesigner.uploadschemeBPMN($('form')[0], function () {
                wfdesigner.autoarrangement();
                alert('The file is uploaded!');
            });
        }
        else {
            wfdesigner.uploadscheme($('form')[0], function () {
                alert('The file is uploaded!');
            });
        }
    }

    function OnSave() {
        wfdesigner.schemecode = schemecode;

        var err = wfdesigner.validate();
        if (err != undefined && err.length > 0) {
            alert(err);
        }
        else {
            wfdesigner.save(function () {
                alert('The scheme is saved!');
            });
        }
    }
    function OnNew() {
        wfdesigner.create();
    }
    </script>
</asp:Content>