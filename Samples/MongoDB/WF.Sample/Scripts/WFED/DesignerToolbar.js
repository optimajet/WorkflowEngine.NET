function WorkflowDesignerToolbar() {
    this.type = 'WorkflowDesignerToolbar';

    var me = this;

    this.init = function (designer) {
        this.designer = designer;
        this.Layer = new Kinetic.Layer();
        this.designer.Stage.add(this.Layer);
        this.Layer.setZIndex(4);

        var x = 20;
        var y = 10;

        var index = 0;
        this.Items.forEach(function (item) {
            item.index = index;
            index++;
            if (!item.separator){
                item.ImageToolbar = new Image();
                item.ImageToolbar.onload = function () {
                    item.cImageToolbar = new Kinetic.Image({
                        x: x + item.index * 40,
                        y: y,
                        image: item.ImageToolbar,
                        width: 32,
                        height: 32,
                        strokeWidth: 0
                    });

                    item.cImageToolbar.on('click', item.click);
                    me.Layer.add(item.cImageToolbar);
                    WorkflowDesignerTooltip(me.Layer, item.cImageToolbar, item.title);
                    me.Layer.batchDraw();
                };
                item.ImageToolbar.src = me.designer.Settings.imagefolder + item.img;

                if (item.imgactive) {
                    item.ImageActiveToolbar = new Image();
                    item.ImageActiveToolbar.src = me.designer.Settings.imagefolder + item.imgactive;
                }
            }
        });

       
        this.CreateInfoBlock({
            x: x + (this.Items[this.Items.length - 1].index + 1) * 40,
            y: y - 7
        });

        var bgComponent = this.GetWorkflowDesignerBackground();
        bgComponent.RectBG.setDraggable(false);
    };

    this.draw = function () {
        this.GraphRedrawAll();
    };

    this.GraphRedrawAll = function () {
        this.UpdateInfoBlock();
        this.Layer.batchDraw();
    };

    this.CreateInfoBlock = function (pos) {
        this.infoText = new Kinetic.Text({
            text: this.GetInfoBlockText(),
            fontFamily: 'Calibri',
            fontSize: 12,
            padding: 5,
            fill: 'black'
        });

        this.info = new Kinetic.Group(pos);
        this.info.add(this.infoText);
        this.Layer.add(this.info);
    }

    this.UpdateInfoBlock = function (pos) {
        this.infoText.setText(this.GetInfoBlockText());
    };

    this.GetInfoBlockText = function () {
        var aCount = 0;
        var tCount = 0;
        var commandCount = 0;

        if (this.designer.data != undefined) {
            aCount = this.designer.data.Activities.length;
            tCount = this.designer.data.Transitions.length;
            commandCount = this.designer.data.Commands.length;
        }
        return WorkflowDesignerConstants.InfoBlockLabel.Activity + aCount + '\r\n' +
            WorkflowDesignerConstants.InfoBlockLabel.Transition + tCount + '\r\n' +
            WorkflowDesignerConstants.InfoBlockLabel.Command + commandCount;
    };

    this.ToolbarMovePress = function () {
        var bgComponent = this.GetWorkflowDesignerBackground();
        bgComponent.setMoveModeEnabled(!bgComponent._movemodeenabled);
        var obj = this.GetItemByCode('move').cImageToolbar;
        if (bgComponent._movemodeenabled) {
            obj.setStrokeWidth(4);
            obj.setStrokeEnabled(true);
            obj.setStroke(WorkflowDesignerConstants.SelectColor);
        }
        else {
            obj.setStrokeEnabled(false);
            obj.setStroke('black');
        }
        this.Layer.batchDraw();
    };
    this.GetWorkflowDesignerBackground = function () {
        return this.designer.GetComponentByType("WorkflowDesignerBackground");
    };
    this.CreateActivity = function () {
        var component = this.designer.GetComponentByType("WorkflowDesignerActivity");
        component.CreateNewActivity();
        this.designer.redrawAll();
    };

    this.AutoArrangement = function () {
        if (me.designer.data.Activities.length == 0)
            return;

        var activityStarts = new Array();
        me.designer.data.Activities.forEach(function (item) {
            if(item.IsInitial){
                activityStarts.push(item);
            }
        });
     
        me.designer.data.Activities.forEach(function (item) {
            if (!item.IsInitial) {
                var isFind = true;
                for (var i = 0; i < me.designer.data.Transitions.length; i++) {
                    var trans = me.designer.data.Transitions[i];
                    if (trans.Classifier == 'Direct' && trans.To == item) {
                        isFind = false;
                        break;
                    }
                }

                if (isFind)
                    activityStarts.push(item);
            }
        });

        if (activityStarts.length == 0)
            activityStarts.push(me.designer.data.Activities[0]);

        var pos = { x: 80, y: 80 };
        var step = { x: 250, y: 150 };

        var processedActivity = Array();
        
        var arrangementActivityFunc = function (items, startPos, isFirst) {        
            var currpos = { x: startPos.x, y: startPos.y };

            if(!isFirst)
                currpos.x += step.x;

            var tmpArray = new Array();
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                if (item.DesignerSettings == undefined)
                    item.DesignerSettings = {};

                if (!isFirst && $.inArray(item, processedActivity) >= 0) {
                    continue;
                }

                item.DesignerSettings.X = currpos.x;
                processedActivity.push(item);
                tmpArray.push(item);
            }

            for (var i = 0; i < tmpArray.length; i++) {
                var item = tmpArray[i];
              
                if (i > 0)
                    currpos.y += step.y;

                var activityChildren = new Array();
                me.designer.data.Transitions.forEach(function (trans) {
                    if (trans.Classifier == 'Direct' && trans.From == item){
                        activityChildren.push(trans.To);
                    }
                });

                item.DesignerSettings.Y = currpos.y;
                var endPos = arrangementActivityFunc(activityChildren, { x: currpos.x, y: currpos.y });
                currpos.y = endPos.y;
            }
            return { x: currpos.x, y: currpos.y };
        };
        
        arrangementActivityFunc(activityStarts, pos, true);

        me.designer.data.Transitions.forEach(function (t1) {
            if (t1.DesignerSettings == undefined)
                t1.DesignerSettings = {};
            t1.DesignerSettings.Bending = 0;
        });

        me.designer.Draw(me.designer.data);
    };
    
    this.CopySelectedGenUniqueValue = function (value, array, property) {
        var name = value;
        for (var i = 1; true; i++)
        {
            var isFind = false;
            for(var j = 0; j < array.length; j++){
                if (array[j][property] == name) {
                    isFind = true;
                    break;
                }
            }

            if (!isFind) {
                break;
            }            
            name = value + '_' + i;
        }

        return name;
    }

    this.CopySelected = function () {
        var componentA = this.designer.GetComponentByType("WorkflowDesignerActivity");
        var componentT = this.designer.GetComponentByType("WorkflowDesignerTransition");

        var selectedA = componentA.GetSelected();
        var selectedT = componentT.GetSelected();

        var newObjectsA = [];
        selectedA.forEach(function (item) {
            var newItem = JSON.parse(JSON.stringify(item.item));
            newItem.DesignerSettings.Y += 150;
            newItem.Name = me.CopySelectedGenUniqueValue(newItem.Name, me.designer.data.Activities, 'Name');

            newObjectsA.push({
                oldItem: item.item,
                newItem: newItem
            });

            me.designer.data.Activities.push(newItem);
        });

        var newObjectsT = [];
        selectedT.forEach(function (item) {
            var aFrom = item.item.From;
            var aTo = item.item.To;
            for (var i = 0; i < newObjectsA.length; i++) {
                if (aFrom == newObjectsA[i].oldItem)
                    aFrom = newObjectsA[i].newItem;

                if (aTo == newObjectsA[i].oldItem)
                    aTo = newObjectsA[i].newItem;
            }
            
            var newItem = JSON.parse(JSON.stringify(item.item));
            newItem.Name = me.CopySelectedGenUniqueValue(newItem.Name, me.designer.data.Transitions, 'Name');
            newItem.From = aFrom;
            newItem.To = aTo;

            newObjectsT.push({
                oldItem: item.item,
                newItem: newItem
            });

            me.designer.data.Transitions.push(newItem);
        });

        WorkflowDesignerCommon.DataCorrection(me.designer.data);
        me.designer.Draw(me.designer.data);

        newObjectsA.forEach(function (item) {
            componentA.SelectByItem(item.newItem);
        });

        newObjectsT.forEach(function (item) {
            componentT.SelectByItem(item.newItem);
        });
    };

    this.EditLocalization = function () {
        var labels = WorkflowDesignerConstants.LocalizationFormLabel;
        
        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.Localization,
            datadefault: { Culture: 'en-US', Type : 'State' },
            elements: [
                { name: labels.ObjectName, field: "ObjectName", type: "input" },
                {
                    name: labels.Type, field: "Type", type: "select", displayfield: 'Name', valuefield: 'Value',
                    datasource: [{ Name: labels.Types[0], Value: 'Command' },
                                    { Name: labels.Types[1], Value: 'State' },
                                    { Name: labels.Types[2], Value: 'Parameter' }]
                },
                { name: labels.IsDefault, field: "IsDefault", type: "checkbox" },
                { name: labels.Culture, field: "Culture", type: "input" },
                { name: labels.Value, field: "Value", type: "input" }
            ]
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['ObjectName', 'Type', 'Culture', 'Value'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['ObjectName', 'Type', 'Culture'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);
                me.SyncTable(me.designer.data.Localization, data, params);
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditTimer = function () {
        var labels = WorkflowDesignerConstants.TimerFormLabel;

        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.Timers,
            keyproperty: 'Name',
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Type, field: "Type", type: "select", datasource: this.designer.data.AdditionalParams.TimerTypes },
                { name: labels.Value, field: "Value", type: "input" },
                { name: labels.NotOverrideIfExists, field: "NotOverrideIfExists", type: "checkbox" }
            ]
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Type', 'Value'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);

                if (me.designer.data.Timers == undefined)
                    me.designer.data.Timers = [];

                me.SyncTable(me.designer.data.Timers, data, params);
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditActors = function () {
        var labels = WorkflowDesignerConstants.ActorFormLabel;
        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.Actors,
            keyproperty: 'Name',
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Rule, field: "Rule", type: "select", datasource: this.designer.getActorNames()/*this.designer.data.AdditionalParams.Rules */ },
                { name: labels.Value, field: "Value", type: "input" }
            ]
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Rule'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);
                me.SyncTable(me.designer.data.Actors, data, params);
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditParameters = function () {
        var labels = WorkflowDesignerConstants.ParameterFormLabel;

        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.Parameters,
            datadefault: { Purpose: 'Persistence' },
            keyproperty: labels.Name,
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Type, field: "Type", type: "input" },
                { name: labels.Purpose, field: "Purpose", type: "select", displayfield: 'Name', datasource: [{ Name: 'Temporary' }, { Name: 'Persistence' }, { Name: 'System' }] },
                { name: labels.DefaultValue, field: "DefaultValue", type: "input" }
            ]
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Type', 'Purpose', 'Parameter'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique)) {
                form.ClearTempField(data);
                me.SyncTable(me.designer.data.Parameters, data, params);
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditCodeActions = function () {
        var labels = WorkflowDesignerConstants.CodeActionsFormLabel;

        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.CodeActions,
            datadefault: {},
            keyproperty: labels.Name,
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.Type, field: "Type", type: "select", displayfield: 'Name', datasource: [{ Name: 'Action' }, { Name: 'Condition' }, { Name: 'RuleGet' }, { Name: 'RuleCheck' }] },
                { name: labels.IsGlobal, field: "IsGlobal", type: "checkbox" },
                { name: labels.ActionCode, field: "ActionCode", type: "code" }
            ],
            designer : me.designer
        };

        var form = new WorkflowDesignerForm(params);

        var saveFunc = function(data, formparams) {
            if (form.CheckRequired(data, ['Name', 'Type', 'ActionCode.code'], WorkflowDesignerConstants.FieldIsRequired) &&
                form.CheckUnique(data, ['Name', 'Type'], WorkflowDesignerConstants.FieldMustBeUnique)) {
               
                form.ClearTempField(data);
                me.SyncTable(me.designer.data.CodeActions, data, params);
                for (var i = 0; i < me.designer.data.CodeActions.length; i++) {
                    var code = me.designer.data.CodeActions[i].ActionCode;
                    me.designer.data.CodeActions[i].ActionCode = encodeURIComponent(code.code);
                    me.designer.data.CodeActions[i].Usings = encodeURIComponent(code.usings);
                }
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    }

    this.EditCommands = function () {
        var labels = WorkflowDesignerConstants.CommandFormLabel;

        var params = {
            type: 'table',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.Commands,
            datadefault: {},
            keyproperty: 'Name',
            elements: [
                { name: labels.Name , field: "Name", type: "input" },
                {
                    name: labels.InputParameters, field: "InputParameters", type: "table", elements: [
                        { name: labels.InputParametersName, code: 'ipname', field: "Name", type: "input" },
                        { name: labels.InputParametersParameter, code: 'ipparameter', field: "Parameter.Name", type: "select", displayfield: 'Name', datasource: me.designer.data.Parameters }]
                }
            ]
        };

        var form = new WorkflowDesignerForm(params);

        var validFunc = function (formControl, data) {
            var isValid = true;

            isValid &= formControl.CheckRequired(data, ['Name'], WorkflowDesignerConstants.FieldIsRequired);
            isValid &= formControl.CheckUnique(data, ['Name'], WorkflowDesignerConstants.FieldMustBeUnique);
            data.forEach(function (item) {
                if(!formControl.CheckRequired(item.InputParameters, ['Name', 'Parameter.Name'], WorkflowDesignerConstants.FieldIsRequired)){
                    isValid = false;
                }
            });
            return isValid;
        }

        var saveFunc = function (data) {
            if (validFunc(form, data)) {
                form.ClearTempField(data);
                me.SyncTable(me.designer.data.Commands, data, params);
                WorkflowDesignerCommon.DataCorrection(me.designer.data);
                me.GraphRedrawAll();
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.EditAdditionalParameters = function () {
        var labels = WorkflowDesignerConstants.AdditionalParamsFormLabel;

        var params = {
            type: 'form',
            title: labels.Title,
            width: '800px',
            data: this.designer.data.AdditionalParams,
            readonly: true, 
            elements: [
                { name: labels.IsObsolete, field: "IsObsolete", type: "checkbox" },
                { name: labels.DefiningParameters, field: "DefiningParameters", type: "textarea" },
                {
                    name: labels.ProcessParameters , field: "ProcessParameters", type: "table", elements: [
                        { name: labels.ProcessParametersName, field: "Name", type: "input" },
                        { name: labels.ProcessParametersValue, field: "Value", type: "input" }
                    ]
                }
            ]
        };
        
        var form = new WorkflowDesignerForm(params);

        var saveFunc = function (data, formparams) {
            return true;          
        };

        form.showModal(saveFunc);
    };

    this.Items = [
        { title: WorkflowDesignerConstants.ToolbarLabel.CreateActivity, img: 'designer.tb.add.png', click: function () { me.CreateActivity(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.CopySelected, img: 'designer.tb.copy.png', click: function () { me.CopySelected(); } },
        { title: WorkflowDesignerConstants.ButtonTextDelete, img: 'designer.tb.delete.png', click: function () { me.designer.DeleteSelected(); } },        
        { separator: true },
        //{ title: WorkflowDesignerConstants.ToolbarLabel.Undo, img: 'designer.tb.undo.png', click: function () { me.designer.Undo(); } },
        //{ title: WorkflowDesignerConstants.ToolbarLabel.Redo, img: 'designer.tb.redo.png', click: function () { me.designer.Redo(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.Move, img: 'designer.tb.move.png', code: 'move', click: function () { me.ToolbarMovePress(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.ZoomIn, img: 'designer.tb.zoomIn.png', click: function () { me.designer.GraphLayerScale(0.1); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.ZoomOut, img: 'designer.tb.zoomOut.png', click: function () { me.designer.GraphLayerScale(-0.1); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.ZoomPositionDefault, img: 'designer.tb.zoomnorm.png', click: function () { me.designer.GraphLayerScaleNorm(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.AutoArrangement, img: 'designer.tb.arrangment.png', click: function () { me.AutoArrangement(); } },
        { separator: true },
        { title: WorkflowDesignerConstants.ToolbarLabel.Actors, img: 'designer.tb.actor.png', click: function () { me.EditActors(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.Commands, img: 'designer.tb.command.png', click: function () { me.EditCommands(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.Parameters, img: 'designer.tb.parameter.png', click: function () { me.EditParameters(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.Localization, img: 'designer.tb.locale.png', click: function () { me.EditLocalization(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.Timers, img: 'designer.tb.timer.png', click: function () { me.EditTimer(); } },
         { title: WorkflowDesignerConstants.ToolbarLabel.CodeActions, img: 'designer.tb.codeactions.png', click: function () { me.EditCodeActions(); } },
        { title: WorkflowDesignerConstants.ToolbarLabel.AdditionalParameters, img: 'designer.tb.additionalparameters.png', click: function () { me.EditAdditionalParameters(); } }];

    this.GetItemByCode = function (code) {
        for (var i = 0; i < this.Items.length; i++) {
            var a = this.Items[i];
            if (a.code == code)
                return a;
        }
        return undefined;
    };

    this.SyncTable = function (source, dest, params) {
        if (params.keyproperty == undefined) {
            source.splice(0, source.length);
            for (var i = 0; i < dest.length; i++) {
                var newItem = {};
                params.elements.forEach(function (e) {
                    newItem[e.field] = dest[i][e.field];
                });
                source.push(newItem);
            }
        }
        else {
            for (var i = source.length - 1; i >= 0; i--) {
                var findEl = $.grep(dest, function (el) {
                    return source[i][params.keyproperty] == el.keyproperty;
                });

                if (findEl.length == 0)
                    source.splice(i, 1);
                else {
                    params.elements.forEach(function (e) {
                        source[i][e.field] = findEl[0][e.field];
                    });
                }
            }

            for (var i = 0; i < dest.length; i++) {
                var findEl = $.grep(source, function (el) {
                    return dest[i][params.keyproperty] == el[params.keyproperty];
                });

                if (findEl.length == 0) {
                    var newItem = {};
                    params.elements.forEach(function (e) {
                        newItem[e.field] = dest[i][e.field];
                    });
                    source.push(newItem);
                }
            }
        }
    };
};