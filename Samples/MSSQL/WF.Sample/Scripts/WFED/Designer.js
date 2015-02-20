var WorkflowDesignerCommon = {
    createArrowByAngle: function (x, y, angle, headlen, colour) {

        if (colour == undefined)
            colour = 'red';

        var wedge = new Kinetic.Wedge({
            x: x,
            y: y,
            radius: headlen,
            angle: 40,
            fill: colour,
            rotation: angle * 180 / Math.PI - 200
        });

        return wedge;
    },
    updateArrowByAngle: function (arrow, x, y, angle, headlen, colour) {

        if (colour == undefined)
            colour = 'red';

        arrow.setPosition({ x: x, y: y });
        arrow.setRadius(headlen);
        arrow.setFill(colour);
        arrow.setRotation(angle * 180 / Math.PI - 200);
    },
    createUUID: function () {

        var s = [];
        var hexDigits = "0123456789abcdef";
        for (var i = 0; i < 36; i++) {
            s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
        }
        s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
        s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
        s[8] = s[13] = s[18] = s[23] = "-";

        var uuid = s.join("");
        return uuid;
    },
    DataCorrection: function (data) {
        if (data.AdditionalParams == undefined)
            data.AdditionalParams = {};

        if (data.AdditionalParams.Actions == undefined)
            data.AdditionalParams.Actions = [];

        if (data.AdditionalParams.Rules == undefined)
            data.AdditionalParams.Rules = [];

        var checkAdditionalParams = function (item, params) {
            if (item == undefined)
                return;

            var findAction = $.grep(params, function (el) {
                return el == item;
            });

            if (findAction.length == 0)
                params.push(item);
        };

        var checkLink = function (value, array, propertyName) {
            if (value == undefined || array == undefined)
                return;

            var findItems = $.grep(array, function (el) {
                return value == el[propertyName];
            });

            if (findItems.length > 0){
                return findItems[0];
            }
        };

        data.Activities.forEach(function (a) {
            if (a.Implementation != undefined) {
                a.Implementation.forEach(function (ai) {
                    checkAdditionalParams(ai.Name, data.AdditionalParams.Actions);
                });
                a.PreExecutionImplementation.forEach(function (ai) {
                    checkAdditionalParams(ai.Name, data.AdditionalParams.Actions);
                });
            }
        });

        data.Transitions.forEach(function (t) {

            if (t.From != undefined) {
                t.From = checkLink(t.From.Name, data.Activities, 'Name');
            }

            if (t.To != undefined) {
                t.To = checkLink(t.To.Name, data.Activities, 'Name');
            }

            if (t.Restrictions != undefined) {
                t.Restrictions.forEach(function (cip) {
                    cip.Actor = checkLink(cip.Actor.Name, data.Actors, 'Name');
                });
            }

            if (t.Condition != undefined && t.Condition.Action != undefined) {
                checkAdditionalParams(t.Condition.Action.Name, data.AdditionalParams.Actions);                
            }

            if (t.Trigger != undefined && t.Trigger.Command != undefined) {
                t.Trigger.Command = checkLink(t.Trigger.Command.Name, data.Commands, 'Name');
            }

            if (t.Trigger != undefined && t.Trigger.Timer != undefined) {
                t.Trigger.Timer = checkLink(t.Trigger.Timer.Name, data.Timers, 'Name');
            }
        });

        data.Commands.forEach(function (c) {
            if (c.InputParameters != undefined) {
                c.InputParameters.forEach(function (cip) {
                    cip.Parameter = checkLink(cip.Parameter.Name, data.Parameters, 'Name');
                });
            }
        });

        data.Actors.forEach(function (a) {
            if (a.Rule != undefined) {
                checkAdditionalParams(a.Rule, data.AdditionalParams.Rules);
            }
        });

    },
    download: function (url, data, method) {
        if (url && data) {
            var inputs = new Array();
            data.forEach(function (item) {
                var tmp = $('<input type="hidden"/>');
                tmp.attr('name', item.name);
                tmp.attr('value', item.value);
                inputs.push(tmp);
            });

            var form = $('<form action="' + url + '" method="' + (method || 'post') + '"></form>');
            form.append(inputs);
            form.appendTo('body').submit().remove();
        };
    }
};

function WorkflowDesigner(settings) {
    var me = this;
    this.Settings = settings;

    this.GetName = function () { return me.Settings.name; };
    this.error = function (msg) {
        alert(msg);
    };
    this.load = function (params) {
        var data = new Array();

        this.schemecode = params.schemecode;
        this.processid = params.processid;
        this.schemeid = params.schemeid;
        this.readonly = params.readonly;

        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });
        data.push({ name: 'operation', value: 'load' });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            success: function (response) {
                try{
                    me.data = JSON.parse(response);
                }
                catch (exception_var) {
                    me.error(response);
                }
                me.render();
            },
            error: function (request, textStatus, errorThrown) {
                me.error(textStatus + ' ' + errorThrown);
            }
        });
    };

    this.exists = function (params) {
        var data = new Array();

        this.schemecode = params.schemecode;
        this.processid = params.processid;
        this.schemeid = params.schemeid;
        this.readonly = params.readonly;

        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });
        data.push({ name: 'operation', value: 'exists' });

       var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: false,
            error: function (request, textStatus, errorThrown) {
                me.error(textStatus + ' ' + errorThrown);
            }
       }).responseText;

        try{
            return JSON.parse(res);
        }
        catch (exception_var) {
            me.error(res);
            return false;
        }

    };

    this.create = function () {
        var data = new Array();
        data.push({ name: 'operation', value: 'load' });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            success: function (response) {
                try{
                    me.data = JSON.parse(response);
                }
                catch (exception_var) {
                    me.error(response);
                }
                me.render();
            }
        });
    };

    this.render = function () {
        if (me.Graph)
            me.Graph.destroy();

        me.Graph = new WorkflowGraph(this.Settings.renderTo, me, me.Settings,
        [
            WorkflowDesignerBackground,
            WorkflowDesignerToolbar,
            WorkflowDesignerActivity,
            WorkflowDesignerTransition,
            WorkflowDesignerKeyboard
        ]);

        WorkflowDesignerCommon.DataCorrection(me.data);
        me.Graph.Draw(me.data);
    };

    this.save = function (successFunc) {
        if (me.readonly) {
            alert(WorkflowDesignerConstants.ErrorReadOnlySaveText);
            return;
        }

        var data = new Array();
        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });        
        data.push({ name: 'operation', value: 'save' });
        data.push({ name: 'data', value: JSON.stringify(this.data) });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            type: "post",
            success: function (response) {
                try {
                    me.data = JSON.parse(response);
                }
                catch (exception_var) {
                    me.error(response);
                }
                me.render();

                if (successFunc) {
                    setTimeout(function() {successFunc(me);}, 100);
                }
            }
        });
    };

    this.downloadscheme = function (params) {
        var data = new Array();
        data.push({ name: 'operation', value: 'downloadscheme' });
        data.push({ name: 'data', value: JSON.stringify(this.data) });
        WorkflowDesignerCommon.download(this.Settings.apiurl, data, 'post');
    };

    this.uploadscheme = function (form, successFunc) {

        var iframeid = this.GetName() + '_uploadiframe';

        // Create the iframe...
        var iframe = document.createElement("iframe");
        iframe.setAttribute("id", iframeid);
        iframe.setAttribute("name", iframeid);
        iframe.setAttribute("width", "0");
        iframe.setAttribute("height", "0");
        iframe.setAttribute("border", "0");
        iframe.setAttribute("style", "width: 0; height: 0; border: none;");

        // Add to document...
        form.parentNode.appendChild(iframe);
        window.frames[iframeid].name = iframeid;

        iframeId = document.getElementById(iframeid);

        // Add event...
        var eventHandler = function () {

            if (iframeId.detachEvent) iframeId.detachEvent("onload", eventHandler);
            else iframeId.removeEventListener("load", eventHandler, false);

            // Message from server...
            if (iframeId.contentDocument) {
                content = iframeId.contentDocument.body.innerHTML;
            } else if (iframeId.contentWindow) {
                content = iframeId.contentWindow.document.body.innerHTML;
            } else if (iframeId.document) {
                content = iframeId.document.body.innerHTML;
            }

            me.data = JSON.parse(content);
            me.render();

            // Del the iframe...
            setTimeout('iframeId.parentNode.removeChild(iframeId)', 250);

            if (successFunc)
                successFunc(me);
        }

        if (iframeId.addEventListener) iframeId.addEventListener("load", eventHandler, true);
        if (iframeId.attachEvent) iframeId.attachEvent("onload", eventHandler);

        form.setAttribute("target", iframeid);
        form.setAttribute("action", this.createurl('uploadscheme'));
        form.setAttribute("method", "post");
        form.setAttribute("enctype", "multipart/form-data");
        form.setAttribute("encoding", "multipart/form-data");

        form.submit();
    };
    this.createurl = function(operation){
        var url = this.Settings.apiurl;
        var separator = '?';
        if (url.indexOf('?') >= 0)
            separator = '&';

        url += separator + "operation=" + operation;
        separator = '&';

        if (this.schemeid != undefined) {
            url += separator + "schemeid=" + this.schemeid;
        }
       
        if (this.processid != undefined) {
            url += separator + "processid=" + this.processid;
        }

        if (this.schemecode != undefined) {
            url += separator + "schemecode=" + this.schemecode;
        }

        return url;
    };

    this.validate = function () {
        var err = undefined;
        
        var findActivityInitilal = $.grep(me.data.Activities, function (el) {
            return el.IsInitial == true;
        });

        if (findActivityInitilal.length != 1)
            err = WorkflowDesignerConstants.ErrorActivityIsInitialCountText;

        return err;
    };

    this.destroy = function () {
        this.schemecode = undefined;
        this.processid = undefined;
        this.schemeid = undefined;
        this.data = undefined;
        this.Graph.destroy();
    }

    this.compile = function (item, successFunc) {
        item = { Name: item.Name, Type: item.Type, IsGlobal: item.IsGlobal, ActionCode: item.ActionCode, Usings: item.Usings };
        var data = new Array();
        data.push({ name: 'schemecode', value: this.schemecode });
        data.push({ name: 'processid', value: this.processid });
        data.push({ name: 'schemeid', value: this.schemeid });
        data.push({ name: 'operation', value: 'compile' });
        data.push({ name: 'data', value: JSON.stringify(item) });

        var res = $.ajax({
            url: this.Settings.apiurl,
            data: data,
            async: true,
            type: "post",
            success: function (response) {
                try {
                    response = JSON.parse(response);
                }
                catch (exceptionVar) {
                    me.error(response);
                }

                if (successFunc) {
                    setTimeout(function () { successFunc(response); }, 100);
                }
            }
        });
    }

};
