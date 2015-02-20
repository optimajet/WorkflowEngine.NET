function WorkflowGraph(container, designer, settings, components) {
    var me = this;
    me.container = container;
    me.designer = designer;

    if (settings == undefined) {
        settings = new Object();
    }

    if (settings.Container == undefined) {
        settings.Container = 'container';
    }

    if (settings.graphwidth == undefined) {
        settings.graphwidth = 1024;
    }

    if (settings.graphheight == undefined) {
        settings.graphheight = 768;
    }

    if (settings.DefaultActivityWidth == undefined) {
        settings.DefaultActivityWidth = 150;
    }

    if (settings.DefaultActivityHeight == undefined) {
        settings.DefaultActivityHeight = 75;
    }

    if (settings.DefaultMoveStep == undefined) {
        settings.DefaultMoveStep = 10;
    }

    if (settings.imagefolder == undefined)
        settings.imagefolder = '/images/';

    this.Settings = settings;
    this.Settings.ContainerStage = this.container + '_stage';
    $('#' + this.container).append('<div id=\'' + this.Settings.ContainerStage + '\'></div>');

    this.Stage = new Kinetic.Stage({
        container: this.Settings.ContainerStage,
        width: this.Settings.graphwidth,
        height: this.Settings.graphheight,
        stroke: 'black',
    });

    //Components
    this.Components = new Array();
    this.AddComponent = function (componentclass) {
        var obj = new componentclass();
        obj.init(this);
        me.Components.push(obj);
        return obj;
    };
    this.GetComponentByType = function (type) {
        for (var i = 0; i < this.Components.length; i++) {
            if (this.Components[i].type == type)
                return this.Components[i];
        }
        return undefined;
    };
    this.ComponentsExecute = function (func, params) {
        me.Components.forEach(function (item) {
            if (item[func])
                item[func](params);
        });
    };

    if (components) {
        components.forEach(function (item) {
            me.AddComponent(item);
        });
    }

    this.Draw = function (data) {
        me.data = data;
        me.ComponentsExecute('draw');
    };

    this.GraphLayerSetOffset = function (x, y) {
        me.ComponentsExecute('LayerSetOffset', { x: x, y: y });
        me.redrawAll();
    };

    this.GraphLayerScale = function (a) {
        me.ComponentsExecute('LayerScale', a);
        me.redrawAll();
    };
    this.GraphLayerScaleNorm = function (a) {
        me.ComponentsExecute('LayerScaleNorm', a);
        me.redrawAll();
    };
    this.DeselectAll = function () {
        me.ComponentsExecute('DeselectAll');
        me.redrawAll();
    };
    this.redrawAll = function () {
        me.ComponentsExecute('GraphRedrawAll');
        me.Stage.batchDraw();
    };
    this.CorrectPossition = function (e, layer) {
        if (layer.getScaleX() == 0 || layer.getScaleY() == 0)
            return { x: layer.getOffsetX(), y: 0 };

        return {
            x: e.x / layer.getScaleX() + layer.getOffsetX(),
            y: e.y / layer.getScaleY() + layer.getOffsetY()
        }
    };

    this.DeleteSelected = function () {
        var array = new Array();

        this.Components.forEach(function (item) {
            if (item.GetSelected)
                array = array.concat(item.GetSelected());
        });

        if (array.length > 0) {
            if (confirm(WorkflowDesignerConstants.DeleteConfirm)) {
                array.forEach(function (item) {
                    item.Delete();
                });

                this.redrawAll();
            }
        }
    };

    this.destroy = function () {
        this.Stage.destroy();
    };

    this.GetCurrentActivity = function () {
        if (me.data == undefined || me.data.AdditionalParams == undefined || me.data.AdditionalParams.ProcessParameters == undefined) {
            return undefined;
        }

        for (var i = 0; i < me.data.AdditionalParams.ProcessParameters.length; i++)
        {
            var item = me.data.AdditionalParams.ProcessParameters[i];
            if (item.Name == 'CurrentActivity')
                return item.Value;
        }
        return undefined;
    };

    this.getActionNames = function () {
        var me = this;
        var codeActions = new Array();
        for (var i = 0; i < me.data.CodeActions.length; i++) {
            var codeAction = me.data.CodeActions[i];
            if (codeAction.Type.toLowerCase() === 'action')
                codeActions.push(codeAction.Name);
        }
        return me.data.AdditionalParams.Actions.concat(codeActions);
    },

    this.getConditionNames = function () {
        var me = this;
        var codeActions = new Array();
        for (var i = 0; i < me.data.CodeActions.length; i++) {
            var codeAction = me.data.CodeActions[i];
            if (codeAction.Type.toLowerCase() === 'condition')
                codeActions.push(codeAction.Name);
        }
        return me.data.AdditionalParams.Actions.concat(codeActions);
    }

    this.getActorNames = function () {
        var me = this;
        var actors = new Array();
        for (var i = 0; i < me.data.CodeActions.length; i++) {
            var codeAction = me.data.CodeActions[i];
            if (codeAction.Type.toLowerCase() === 'ruleget')
                actors.push(codeAction.Name);
        }
        return me.data.AdditionalParams.Rules.concat(actors);
    }
};