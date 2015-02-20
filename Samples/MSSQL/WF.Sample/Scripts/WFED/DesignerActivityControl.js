function WorkflowDesignerActivityControl(parameters) {
    var me = this;
    this.manager = parameters.manager;
    this.designer = parameters.designer;
    this.x = parameters.x;
    this.y = parameters.y;
    this.item = parameters.item;
    this.control = undefined;
    this.rectangle = undefined;
    this.text = undefined;
    this.deleteButton = undefined;
    this.createTransitionAndActivityButton = undefined;
    this.createTransitionButton = undefined;
    this.selected = false;
    
    this.dependentTransitions = new Array();
    this.getX = function () {
        return this.rectangle.attrs.x + this.control.attrs.x;
    };    
    this.getY = function () {
        return this.rectangle.attrs.y + this.control.attrs.y;
    };

    this.GetName = function () {
        return this.item.Name;
    };

    this.SetName = function (v) {
        this.item.Name = v;
    };

    this.Draw = function () {

        var settings = me.designer.Settings;

        me.control = new Kinetic.Group({
            x: parameters.x,
            y: parameters.y,
            rotationDeg: 0,
            draggable: true,
            dragBoundFunc: function (pos) {
                var kx = settings.DefaultMoveStep * me.manager.Layer.getScaleX();
                var ky = settings.DefaultMoveStep * me.manager.Layer.getScaleY();
                var pos = {
                    x: Math.round(pos.x / ky) * ky,
                    y: Math.round(pos.y / ky) * ky
                };

                if (me.selected) {
                    var oldpos = this.getAbsolutePosition();
                    me.manager.ObjectMove({ sender: me, changepos: { x: pos.x - oldpos.x, y: pos.y - oldpos.y } });
                }
                return pos;
            }
        });

        var rectColor = WorkflowDesignerConstants.ActivityColor;

        if (me.item.IsFinal)
            rectColor = WorkflowDesignerConstants.ActivityFinalColor;

        if (me.item.IsInitial)
            rectColor = WorkflowDesignerConstants.ActivityInitialColor;

        if (me.designer.GetCurrentActivity() == me.item.Name)
            rectColor = WorkflowDesignerConstants.ActivityCurrentColor;

        me.rectangle = new Kinetic.Rect({
            x: 0,
            y: 0,
            width: this.designer.Settings.DefaultActivityWidth,
            height: this.designer.Settings.DefaultActivityHeight,
            stroke: 'black',
            strokeWidth: 1,
            fill: rectColor,
            shadowOpacity: 0.5
        });

        me.text = new Kinetic.Text({
            x: 2,
            y: 2,
            text: this.GetName(),
            fontSize: 12,
            fontFamily: 'Calibri',
            fontStyle: 'bold',
            fill: 'black'
        });

        me.text.setX((me.rectangle.attrs.width - me.text.getWidth()) / 2);
        me.text.setY((me.rectangle.attrs.height - me.text.getHeight()) / 2 - 6);

        if (me.item.State == undefined)
            me.item.State = '';

        me.stateText = new Kinetic.Text({
            x: 2,
            y: 2,
            text: WorkflowDesignerConstants.ActivityFormLabel.State + ': ' + me.item.State,
            fontSize: 10,
            fontFamily: 'Calibri',
            fill: 'black'
        });
        me.stateText.setX((me.rectangle.attrs.width - me.stateText.getWidth()) / 2);
        me.stateText.setY(me.text.getY() + 15);

        me.deleteButton = new Kinetic.Image({
            x: me.rectangle.attrs.width - 13,
            y: 3,
            image: me.manager.ImageDeleteActivity,
            width: 10,
            height: 10
        });

        me.createTransitionAndActivityButton = new Kinetic.Image({
            x: me.rectangle.attrs.width - 13,
            y: me.rectangle.attrs.height - 26,
            image: me.manager.ImageCreateTransitionAndActivity,
            width: 10,
            height: 10
        });

        me.createTransitionButton = new Kinetic.Image({
            x: me.rectangle.attrs.width - 13,
            y: me.rectangle.attrs.height - 13,
            image: me.manager.ImageCreateTransition,
            width: 10,
            height: 10
        });

        me.control.add(me.rectangle);
        me.control.add(me.text);
        me.control.add(me.stateText);
        me.control.add(me.deleteButton);
        me.control.add(me.createTransitionAndActivityButton);
        me.control.add(me.createTransitionButton);

        this.control.on('dragend', this.Sync);
        this.control.on('dragmove', this._onMove);
        this.control.on('click', this._onClick);
        this.control.on('dblclick', this._onDblClick);
        this.deleteButton.on('click', this._onDelete);
        this.createTransitionAndActivityButton.on('click', this._onCreateTransitionAndActivity);
        this.createTransitionButton.on('click', this._onCreateTransition);

        me.manager.Layer.add(me.control);
    };
    this.Delete = function () {

        this.control.destroy();

        this.designer.data.Activities.splice(this.designer.data.Activities.indexOf(this.item), 1);
        this.manager.ItemControls.splice(this.manager.ItemControls.indexOf(this), 1);

        var todel = new Array();
        for (var i = 0; i < this.dependentTransitions.length; i++) {
            todel.push(this.dependentTransitions[i]);
        }

        for (var i = 0; i < todel.length; i++) {
            todel[i].Delete();
        }
    };
    this.Select = function () {
        this.rectangle.setStrokeWidth(4);
        this.rectangle.setOpacity(0.5);
        this.rectangle.setStroke(WorkflowDesignerConstants.SelectColor);
        this.selected = true;
    };
    this.Deselect = function () {
        this.rectangle.setStrokeWidth(1);
        this.rectangle.setOpacity(1);
        this.rectangle.setStroke('black');
        this.selected = false;
    };
    this.ObjectMove = function (e) {
        var pos = this.control.getAbsolutePosition();
        pos.x += e.x;
        pos.y += e.y;
        this.control.setAbsolutePosition(pos);

        this.Sync();

        if (me.dependentTransitions.length < 1)
            return;

        for (var i = 0; i < me.dependentTransitions.length; i++) {
            me.dependentTransitions[i].Draw();
        }
    };
    this._onMove = function () {
        if (me.dependentTransitions.length < 1)
            return;

        for (var i = 0; i < me.dependentTransitions.length; i++) {
            var t = me.dependentTransitions[i];
            t.Draw();
        }

        me.manager.redrawTransitions();
    };
    this._onClick = function (e) {
        var tmpSelect = me.selected;

        if (!e.evt.ctrlKey)
            me.designer.DeselectAll();

        if (tmpSelect)
            me.Deselect();
        else
            me.Select();

        me.manager.batchDraw();
    };
    this._onDblClick = function () {
        me.designer.DeselectAll();
        me.Select();
        me.manager.batchDraw();

        me.ShowProperties();
    };
    this._onDelete = function () {
        if (confirm(WorkflowDesignerConstants.DeleteConfirm)) {
            me.Delete();
            me.designer.redrawAll();
        }
    };
    this._onCreateTransitionAndActivity = function () {
        me.manager.createTransitionAndActivity(me);
    };
    this._onCreateTransition = function () {
        me.manager.createTransition(me);
    };
    this.RegisterTransition = function (cTransition) {
        var f = false;

        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() == cTransition.GetName()) {
                f = true;
                break;
            }
        }

        if (!f)
            this.dependentTransitions.push(cTransition);
    };
    this.UnregisterTransition = function (cTransition) {
        var f = false;
        var nt = new Array();
        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() != cTransition.GetName()) {
                nt.push(this.dependentTransitions[i]);
            }
        }
        this.dependentTransitions = nt;
    };
    this.RegisterTransition = function (cTransition) {
        var f = false;

        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() == cTransition.GetName()) {
                f = true;
                break;
            }
        }

        if (!f)
            this.dependentTransitions.push(cTransition);
    };
    this.UnregisterTransition = function (cTransition) {
        var f = false;
        var nt = new Array();
        for (var i = 0; i < this.dependentTransitions.length; i++) {
            if (this.dependentTransitions[i].GetName() != cTransition.GetName()) {
                nt.push(this.dependentTransitions[i]);
            }
        }
        this.dependentTransitions = nt;
    };
    this.getRectPos = function () {
        var rectPos = this.rectangle.getAbsolutePosition();
        var xl = rectPos.x;
        var yl = rectPos.y;
        var xr = xl + this.rectangle.getWidth() * this.manager.Layer.getScaleX();
        var yr = yl + this.rectangle.getHeight() * this.manager.Layer.getScaleY();
        return { xl: xl, yl: yl, xr: xr, yr: yr };
    };
    this.getIntersectingActivity = function (point) {
        var pos = this.getRectPos();
        return point.x >= pos.xl && point.x < pos.xr && point.y >= pos.yl && point.y < pos.yr;
    };
    this.getIntersectingActivityRect = function (a) {
        var b = this.getRectPos();
        if (a.xl > b.xr || a.xr < b.xl || a.yl > b.yr || a.yr < b.yl)
            return false;
        return true;
    };

    this.ShowProperties = function () {
        var labels = WorkflowDesignerConstants.ActivityFormLabel;

        var impparam = [
            { name: labels.ImpOrder, code: 'impOrder', field: "Order", type: "input", width:"40px" },
            { name: labels.ImpAction, code: 'impAction', field: "ActionName", type: "select", datasource: me.designer.getActionNames()/*data.AdditionalParams.Actions*/ },
            { name: labels.ImpActionParameter, code: 'impparam', field: "ActionParameter", type: "input" }
        ];

        var params = {
            type: 'form',
            title: labels.Title,
            width: '800px',
            data: this.item,
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.State, field: "State", type: "input" },
                { name: labels.IsInitial, field: "IsInitial", type: "checkbox" },
                { name: labels.IsFinal, field: "IsFinal", type: "checkbox" },
                { name: labels.IsForSetState, field: "IsForSetState", type: "checkbox" },
                { name: labels.IsAutoSchemeUpdate, field: "IsAutoSchemeUpdate", type: "checkbox" },
            {
                name: labels.Implementation, field: "Implementation", type: "table", elements: impparam,
                onrowadded: function (row) {
                    var order = row.find('[name=impOrder]');
                    if (order[0].value === "")
                        order[0].value = row.parent().children().length;
                }
            },
            {
                name: labels.PreExecutionImplementation, field: "PreExecutionImplementation", type: "table", elements: impparam,
                onrowadded: function(row) {
                    var order = row.find('[name=impOrder]');
                    if (order[0].value === "")
                        order[0].value = row.parent().children().length;
                }
            }]
        };

        var form = new WorkflowDesignerForm(params);

        var validFunc = function (formControl, data) {
            var isValid = true;
            isValid &= formControl.CheckRequired([data], ['Name'], WorkflowDesignerConstants.FieldIsRequired);

            me.designer.data.Activities.forEach(function (a) {
                if (a != me.item && a.Name == data.Name) {
                    isValid = false;
                    formControl.ControlAddError(data.control_Name, WorkflowDesignerConstants.FieldMustBeUnique);
                }
            });

            if (!formControl.CheckRequired(data.Implementation, ['ActionName', 'Order'], WorkflowDesignerConstants.FieldIsRequired)) {
                isValid = false;
            }

            if (!formControl.CheckRequired(data.PreExecutionImplementation, ['ActionName', 'Order'], WorkflowDesignerConstants.FieldIsRequired)) {
                isValid = false;
            }
            return isValid;
        }

        var saveFunc = function (data) {
            if (validFunc(form, data)) {
                form.ClearTempField(data);

                me.item.Name = data.Name;
                me.item.State = data.State;
                me.item.IsInitial = data.IsInitial;
                me.item.IsFinal = data.IsFinal;
                me.item.IsForSetState = data.IsForSetState;
                me.item.IsAutoScheme = data.IsAutoScheme;

                me.item.Implementation = data.Implementation;
                me.item.PreExecutionImplementation = data.PreExecutionImplementation;

                WorkflowDesignerCommon.DataCorrection(me.designer.data);
                me.designer.Draw(me.designer.data);
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.Sync = function () {
        if (me.item.DesignerSettings == undefined)
            me.item.DesignerSettings = {};

        var pos = me.control.getPosition();

        me.item.DesignerSettings.X = pos.x;
        me.item.DesignerSettings.Y = pos.y;
    };
};