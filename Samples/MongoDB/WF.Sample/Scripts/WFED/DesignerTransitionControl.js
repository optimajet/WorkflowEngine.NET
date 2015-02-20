function WorkflowDesignerTransitionControl(parameters) {

    var me = this;
    this.manager = parameters.manager;
    this.designer = parameters.designer;
    this.from = parameters.from;
    this.to = parameters.to;
    this.item = parameters.item;

    this.setFrom = function (activity) {
        this.from = activity;
        this.item.From = activity.item;
    };

    this.setTo = function (activity) {
        this.to = activity;
        this.item.To = activity.item;
    };

    this.GetName = function () {
        return this.item.Name;
    };

    this.SetName = function (v) {
        this.item.Name = v;
    };

    this.control = undefined;
    this.arrow = undefined;
    this.line = undefined;

    if (this.item.DesignerSettings == undefined || this.item.DesignerSettings.Bending == undefined) {
        this.bending = 0;
    }
    else {
        this.bending = this.item.DesignerSettings.Bending;
    }

    this.from.RegisterTransition(this);
    this.to.RegisterTransition(this);

    

    this.start = undefined;
    this.end = undefined;
    this.middle = undefined;
    this.angle = undefined;
    this.lineAngle = undefined;

    this.activePoint = undefined;
    this.touchpoints = [];

    this._getCBPoint = function (xs, ys, xe, ye, angle, length, bending) {

        var lineAngle = angle;

        var lineLength = length;

        var k1;

        if (lineAngle < Math.PI / 2 && lineAngle > 0) {
            k1 = 1;
        } else if (lineAngle >= Math.PI / 2) {
            k1 = -1;
        } else if (lineAngle <= 0 && lineAngle > -Math.PI / 2) {
            k1 = -1;
        } else {
            k1 = 1;
        }

        k1 = (lineAngle > 0 ? 1 : -1) * k1;

        var lineBendLength = bending * lineLength * k1;

        var xc = (xs + xe) / 2;
        var yc = (ys + ye) / 2;


        var xcb = xc - lineBendLength * (Math.cos(Math.PI / 2 + lineAngle));

        var ycb = yc - lineBendLength * (Math.sin(Math.PI / 2 + lineAngle));

        return { x: xcb, y: ycb };

    };

    this.DrawTransition = function (fixedstartpoit, fixedendpoint) {
        var bending = this.bending;
        var me = this;

        var delta = 50;
        var fromRec = this.from.rectangle;
        var toRec = this.to.rectangle;

        var fromx, fromy, tox, toy;


        fromx = this.from.getX();
        fromy = this.from.getY();


        tox = this.to.getX();
        toy = this.to.getY();

        var ascx = fromx + fromRec.attrs.width / 2;
        var ascy = fromy + fromRec.attrs.height / 2;
        var aecx = tox + toRec.attrs.width / 2;
        var aecy = toy + toRec.attrs.height / 2;
        var xs, ys, xe, ye;

        if (aecx >= ascx - delta - toRec.attrs.width / 2 && aecx <= ascx + delta + toRec.attrs.width / 2 && aecy >= ascy) {
            xs = ascx;
            ys = fromy + fromRec.attrs.height;
            xe = aecx;
            ye = toy;
        } else if (aecx >= ascx - delta - toRec.attrs.width / 2 && aecx <= ascx + delta + toRec.attrs.width / 2 && aecy < ascy) {
            xs = ascx;
            ys = fromy;
            xe = aecx;
            ye = toy + toRec.attrs.height;
        } else if (aecx < ascx) {
            xs = fromx;
            ys = ascy;
            xe = tox + toRec.attrs.width;
            ye = aecy;
        } else {
            xs = fromx + fromRec.attrs.width;
            ys = fromy + fromRec.attrs.height / 2;
            xe = tox;
            ye = toy + toRec.attrs.height / 2;
        }

        if (fixedstartpoit != undefined) {
            xs = fixedstartpoit.x;
            ys = fixedstartpoit.y;
        }


        if (fixedendpoint != undefined) {
            xe = fixedendpoint.x;
            ye = fixedendpoint.y;
        }


        //Calculates the center point
        var lineAngle = (Math.atan2(ye - ys, xe - xs));

        var lineLength = this._getLineLength(xs, ys, xe, ye);

        var cbp = this._getCBPoint(xs, ys, xe, ye, lineAngle, lineLength, bending);

        var xcb = cbp.x; var ycb = cbp.y;
        var tension = 0.5;


        var angle = Math.atan2(ye - ycb, xe - xcb);

        var add = Math.PI / 10 * (bending > 0 ? 1 : bending < 0 ? -1 : 0);


        if (Math.abs(lineAngle) >= Math.PI / 2)
            add = -add;

        angle += add;

        var classifier = this.item.Classifier == undefined ? 'notspecified' : this.item.Classifier.toLowerCase();
        var color = classifier == 'notspecified' ? 'gray' : classifier == 'direct' ? 'green' : 'red';
        

        this.start = { x: xs, y: ys };
        this.end = { x: xe, y: ye };
        this.middle = { x: xcb, y: ycb };

        this.angle = angle;
        this.lineAngle = lineAngle;

        if (this.control) {
            WorkflowDesignerCommon.updateArrowByAngle(this.arrow, xe, ye, angle, 15, color);
            this.line.setPoints([xs, ys, xcb, ycb, xe, ye]);
            this.line.setTension(tension);
        }
        else {
            this.control = new Kinetic.Group({
                x: 0,
                y: 0,
                rotationDeg: 0
            });

            this.arrow = WorkflowDesignerCommon.createArrowByAngle(xe, ye, angle, 15, color);

            this.line = new Kinetic.Line({
                points: [xs, ys, xcb, ycb, xe, ye],
                stroke: color,
                strokeWidth: 1,
                lineCap: 'round',
                lineJoin: 'round',
                tension: tension
            });

            this.control.add(this.line);
            this.control.add(this.arrow);
            this.manager.Layer.add(this.control);
        }
    };

    this.DrawActivePoint = function () {       
        if (this.activePoint) {
            this._moveActivePoint(this.middle.x, this.middle.y);
        }
        else {
            var activePoint = this._createActivePoint(this.middle.x, this.middle.y, this.control);
            me.manager.APLayer.add(activePoint);
            this.activePoint = activePoint;
        }
    };

    this.DrawTouchPoints = function () {
        var tpShift = this._getLineLength(this.start.x, this.start.y, this.end.x, this.end.y) * 0.1;
        if (this.touchpoints.length == 2) {
            this._moveTouchPoints(this.touchpoints[0], this.start.x, this.start.y, tpShift, this.angle, this.lineAngle);
            this._moveTouchPoints(this.touchpoints[1], this.end.x, this.end.y, tpShift, this.angle, this.lineAngle, true);
        }
        else
        {
            var touchPoint1 = this._createTouchPoint(this.start.x, this.start.y, tpShift, this.angle, this.lineAngle, this.control);
            var touchPoint2 = this._createTouchPoint(this.end.x, this.end.y, tpShift, this.angle, this.lineAngle, this.control, true);

            me.manager.APLayer.add(touchPoint1);
            me.manager.APLayer.add(touchPoint2);
            this.touchpoints = [touchPoint1, touchPoint2];
        }
    };

    this.Draw = function (fixedstartpoit, fixedendpoint) {
        this.DrawTransition(fixedstartpoit, fixedendpoint);
        this.DrawActivePoint();
        this.DrawTouchPoints();
    };

    this.DeleteTouchPoint = function (isend) {
        for (var i = 0; i < this.touchpoints.length; i++) {
            if (this.touchpoints[i].isend == isend) {
                this.touchpoints[i].destroy();
            }
        }
    };

    this.Delete = function () {
        this.from.UnregisterTransition(this);
        this.to.UnregisterTransition(this);
        this.control.destroy();

        if (this.activePoint.ToolTip != undefined) {
            this.activePoint.ToolTip.destroy();
        }
        this.activePoint.destroy();
        for (var i = 0; i < this.touchpoints.length; i++) {
            this.touchpoints[i].destroy();
        }

        this.designer.data.Transitions.splice(this.designer.data.Transitions.indexOf(this.item), 1);
        this.manager.ItemControls.splice(this.manager.ItemControls.indexOf(this), 1);
    };

    this._onDelete = function () {
        if (confirm(WorkflowDesignerConstants.DeleteConfirm)) {
            me.Delete();
            me.designer.redrawAll();
        }
    };

    this.Select = function () {
        this.oldstroke = this.line.getStroke();
        this.line.setStroke(WorkflowDesignerConstants.SelectColor);
        this.line.setStrokeWidth(3);
        this.selected = true;
    };

    this.Deselect = function () {
        this.line.setStrokeWidth(1);
        if (this.oldstroke != undefined) {
            this.line.setStroke(this.oldstroke);
        }
        this.selected = false;
    };

    this._moveTouchPoints = function (tp, x, y, len, angle, lineAngle, isend) {
        var angleGrad = lineAngle * 180 / Math.PI;

        tp.setPosition({ x: x, y: y });
        tp.setRotationDeg(angleGrad);

        tp.circle.setPosition({
            x: (isend ? -1 : 1) * len * Math.cos(-(angle - lineAngle)),
            y: len * Math.sin(-(angle - lineAngle))
        });
    };

    this._createTouchPoint = function (x, y, len, angle, lineAngle, cTransition, isend) {

        var me = this;

        var angleGrad = lineAngle * 180 / Math.PI;

        var cTouchPoint = new Kinetic.Group({
            x: x,
            y: y,
            rotationDeg: angleGrad,
            draggable: true
        });

        cTouchPoint.isend = isend;

        var circle = new Kinetic.Circle({
            x: (isend ? -1 : 1) * len * Math.cos(-(angle - lineAngle)),
            y: len * Math.sin(-(angle - lineAngle)),
            radius: 5,
            fill: 'lightgray'

        });

        cTouchPoint.add(circle);
        cTouchPoint.circle = circle;
        cTouchPoint.transition = cTransition;



        var redraw = function () {
            var position = me.designer.CorrectPossition(circle.getAbsolutePosition(), me.manager.Layer);
            if (me.oldbending == undefined)
                me.oldbending = me.bending;
            me.bending = 0;

            var e = position;

            if (isend) {
                me.DrawTransition(undefined, e);
            } else {
                me.DrawTransition(e, undefined);
            }
            me.DrawActivePoint();
            me.DeleteTouchPoint(!isend);
            me.manager.batchDraw();
        };

        cTouchPoint.on('dragmove', function () {
            redraw();
        });

        cTouchPoint.on('dragend', function () {

            var position = circle.getAbsolutePosition();
            var activity = me.manager.getIntersectingActivity(position);


            if (activity != undefined) {
                me.oldbending = undefined;
                me.bending = 0;
                me.Sync();

                if (isend) {
                    me.to.UnregisterTransition(me);
                    me.setTo(activity);
                    me.to.RegisterTransition(me);

                } else {
                    me.from.UnregisterTransition(me);
                    me.setFrom(activity);
                    me.from.RegisterTransition(me);
                }

                me.Draw();
                
            } else {
                me.bending = me.oldbending;
                me.oldbending = undefined;
                me.Draw();
            }
        });

        return cTouchPoint;

    };

    this._moveActivePoint = function (x, y) {
        this.activePoint.setPosition({x:x, y:y});
    };

    this._createActivePoint = function (x, y, cTransition) {

        var me = this;

        var cActivePoint = new Kinetic.Group({
            x: x,
            y: y,
            rotationDeg: 0,
            draggable: true
        });

        var circle = new Kinetic.Circle({
            x: 0,
            y: 0,
            radius: 15,
            fill: 'lightgray'

        });

        cActivePoint.add(circle);

        var textvalue = '';

        var triggertype = this.item.Trigger.Type.toLowerCase();
        if (triggertype == 'auto') {
            textvalue += 'A';
        }
        else if (triggertype == 'command') {
            textvalue += 'C';
        }
        else if (triggertype == 'timer') {
            textvalue += 'T';
        }
        
            var conditiontype = this.item.Conditions[0].Type.toLowerCase();
            if (conditiontype == 'always') {
                textvalue += 'A';
            }
            else if (conditiontype == 'action') {
                textvalue += 'C';
            }
            else if (conditiontype == 'otherwise') {
                textvalue += 'O';
            }


        var text = new Kinetic.Text({
            x: -12,
            y: -11,
            text: textvalue,
            fontSize: 20,
            fontFamily: 'Calibri',
            fill: 'black',
            fontStyle: 'bold'
        });


        cActivePoint.add(text);

        var deleteButton = new Kinetic.Image({
            x: 12,
            y: -20,
            image: this.manager.ImageDelete,
            width: 10,
            height: 10
        });
        deleteButton.on('click', this._onDelete);
        cActivePoint.add(deleteButton);

        cActivePoint.transition = cTransition;

        var redraw = function (d, r) {
            var position = cActivePoint.getPosition();
            if (r)
                me.bending = 0;
            else {
                me.bending = me._getBendingKoeff(me.start.x, me.start.y, me.end.x, me.end.y, position.x, position.y);
            }


            if (Math.abs(me.bending) < 0.07)
                me.bending = 0;

            me.DrawTransition();
            me.DrawTouchPoints();

            if (d) {
                me.DrawActivePoint();
                me.Sync();
            }

            me.manager.batchDraw();
        };

        cActivePoint.on('click', function (e) {
            var tmpSelect = me.selected;

            if (!e.evt.ctrlKey)
                me.designer.DeselectAll();

            if (tmpSelect)
                me.Deselect();
            else
                me.Select();

            me.manager.batchDraw();
        });

        cActivePoint.on('dblclick', function () {
            me.designer.DeselectAll();
            me.Select();
            me.manager.batchDraw();

            me.ShowProperties();
        });

        cActivePoint.on('dragmove', function () {
            redraw(false);
        });

        cActivePoint.on('dragend', function () {
            redraw(true);
        });

        var tooltiptext = 'Trigger: ' + this.item.Trigger.Type;
        if (me.item.Trigger != undefined && me.item.Trigger.Command != undefined && me.item.Trigger.Type == 'Command')
            tooltiptext += ' ' + me.item.Trigger.Command.Name;

        if (me.item.Trigger != undefined && me.item.Trigger.Timer != undefined && me.item.Trigger.Type == 'Timer')
            tooltiptext += ' ' + me.item.Trigger.Timer.Name;

        tooltiptext += '\r\n\Condition: ' + this.item.Conditions[0].Type;
        if (me.item.Conditions[0] != undefined && me.item.Conditions[0].Type == 'Action')
            tooltiptext += ' ' + me.item.Conditions[0].ActionName;

        WorkflowDesignerTooltip(me.manager.Layer, cActivePoint, tooltiptext, true);

        return cActivePoint;
    };

    this._getLineLength = function (x1, y1, x2, y2) {
        return Math.sqrt(Math.pow((x2 - x1), 2) + Math.pow((y2 - y1), 2));
    };

    this._getBendingKoeff = function (x1, y1, x2, y2, xap, yap) {

        var a = y1 - y2;
        var b = x2 - x1;
        var c = (x1 * y2 - x2 * y1);
        if (b <= 0) {
            a = -a;
            b = -b;
            c = -c;
        }

        var yapn = -(c + a * xap) / b;
        var sign = yapn < yap ? -1 : 1;

        var len1 = this._getLineLength(x1, y1, x2, y2);
        var xc = (x1 + x2) / 2;
        var yc = (y1 + y2) / 2;
        var len2 = this._getLineLength(xc, yc, xap, yap);
        var bending = len2 / len1 * sign;

        if (b == 0)
            bending = -bending;

        return bending;
    };

    this.getIntersectingRect = function (rect) {
        var point = this.activePoint.getAbsolutePosition();
        if (point.x >= rect.xl && point.x < rect.xr && point.y >= rect.yl && point.y < rect.yr)
            return true;       
        return false;
    };

    this.ShowProperties = function () {

        var labels = WorkflowDesignerConstants.TransitionFormLabel;

        var params = {
            type: 'form',
            title: labels.Title,
            width: '800px',
            data: this.item,
            elements: [
                { name: labels.Name, field: "Name", type: "input" },
                { name: labels.From, field: "From.Name", type: "select", displayfield: 'Name', datasource: me.designer.data.Activities },
                { name: labels.To, field: "To.Name", type: "select", displayfield: 'Name', datasource: me.designer.data.Activities },
                { name: labels.Classifier, field: "Classifier", type: "select", datasource: ['Direct', 'Reverse', 'NotSpecified'] },
                {
                    name: labels.Restrictions, field: "Restrictions", code: 'restrictions', type: "table", datadefault: { Type: 'Allow' }, elements: [
                    { name: labels.RestrictionsType, code: 'resttype', field: "Type", type: "select", datasource: ['Allow', 'Restrict'] },
                    { name: labels.RestrictionsActor, code: 'restactor', field: "Actor.Name", type: "select", displayfield: 'Name', datasource:  me.designer.data.Actors }
                    ]
                },
                 { name: labels.AllowConcatenationType, field: "AllowConcatenationType", type: "select", datasource: ['And', 'Or'] },
                { name: labels.RestrictConcatenationType, field: "RestrictConcatenationType", type: "select", datasource: ['And', 'Or'] },
                {
                    name: labels.Trigger, field: "Trigger", code: 'trigger', type: "form", datadefault: { Type: 'Command' }, elements: [
                        { name: labels.TriggerType, code: 'triggertype', field: "Type", type: "select", datasource: ['Auto', 'Command', 'Timer'] },
                        { name: labels.TriggerCommand, code: 'triggercommand', field: "Command.Name", type: "select", displayfield: 'Name', datasource: me.designer.data.Commands },
                        { name: labels.TriggerTimer, code: 'triggertimer', field: "Timer.Name", type: "select", displayfield: 'Name', datasource: me.designer.data.Timers }
                    ]
                },
                {
                    name: labels.Condition, field: "Conditions", code: 'condition', type: "table", datadefault: { Type: 'Always', ResultOnPreExecution: 'Null' }, elements: [
                        { name: labels.ConditionType, code: 'conditiontype', field: "Type", type: "select", datasource: ['Always', 'Action', 'Otherwise'] },
                        { name: labels.ConditionAction, code: 'conditionaction', field: "Action.ActionName", type: "select", datasource: me.designer.getConditionNames() },
                        { name: labels.ConditionActionParameter, code: 'conditionactionparameter', field: "Action.ActionParameter", type: "input" },
                       { name: labels.ConditionInversion, code: 'conditioninversion', field: "ConditionInversion", type: "checkbox" },
                        { name: labels.ResultOnPreExecution, code: 'conditionresult', field: "ResultOnPreExecution", type: "select", datasource: ['True', 'False'] },
                    ],
                    onrowadded: function (row) {
                        var conditiontype = row.find('[name=conditiontype]');
                        var conditionactionrow = row.find('[name=conditionaction]');//.parent().parent();
                        var conditionresultrow = row.find('[name=conditionresult]');//.parent().parent();
                        var conditionactionparameterrow = row.find('[name=conditionactionparameter]');//.parent().parent();
                        var conditioninversionrow = row.find('[name=conditioninversion]');//.parent().parent();
                        var checkConditionType = function () {
                            var type = conditiontype[0].value;
                            if (type == 'Action') {
                                conditionactionrow.show();
                                conditionresultrow.show();
                                conditionactionparameterrow.show();
                                conditioninversionrow.show();

                            }
                            else {
                                conditionactionrow.hide();
                                conditionresultrow.hide();
                                conditionactionparameterrow.hide();
                                conditioninversionrow.hide();
                            }
                        }
                        conditiontype.on('change', checkConditionType);
                        checkConditionType();
                    }
                 
                },
                { name: labels.ConditionsConcatenationType, field: "ConditionsConcatenationType", type: "select", datasource: ['And', 'Or'] }],

            renderFinalFunc: function (control) {
                var restrictions = control.find('[name=restrictions]').parent().parent();
                var triggertype = control.find('[name=triggertype]');
                var triggercommandrow = control.find('[name=triggercommand]').parent().parent();
                var triggertimerrow = control.find('[name=triggertimer]').parent().parent();
                var allowconcatenationtr = $(control.find('[name=AllowConcatenationType]').parent().parent());
                var restrictconcatenationtr = $(control.find('[name=RestrictConcatenationType]').parent().parent());
                var conditionsconcatenationtr = $(control.find('[name=ConditionsConcatenationType]').parent().parent());

                var checkTriggerType = function () {
                    var type = triggertype[0].value;
                    if (type == 'Command') {
                        triggercommandrow.show();
                        triggertimerrow.hide();
                        restrictions.show();
                    }
                    else if (type == 'Timer') {
                        triggercommandrow.hide();
                        triggertimerrow.show();
                        restrictions.hide();
                        allowconcatenationtr.hide();
                        restrictconcatenationtr.hide();
                    }
                    else {
                        triggercommandrow.hide();
                        triggertimerrow.hide();
                        restrictions.hide();
                        allowconcatenationtr.hide();
                        restrictconcatenationtr.hide();
                    }
                }
                triggertype.on('change', checkTriggerType);
                checkTriggerType();

           
                allowconcatenationtr.hide();
                restrictconcatenationtr.hide();
                conditionsconcatenationtr.hide();

                var restrictionstd = $(control.find('[name=restrictions]').parent().parent().children()[0]);
                var restrictionsbtn = $('<button>Additional params</button>').button({
                    icons: {
                        primary: "ui-icon-wrench"
                    },
                    text: false
                }).on('click',function() {
                    if (allowconcatenationtr.is(':visible')) {
                        allowconcatenationtr.hide();
                    } else {
                        allowconcatenationtr.show();
                    }
                    if (restrictconcatenationtr.is(':visible')) {
                        restrictconcatenationtr.hide();
                    } else {
                        restrictconcatenationtr.show();
                    }
                    

                });
                restrictionstd.append('&nbsp;');
                restrictionstd.append(restrictionsbtn);


                var conditionstd = $(control.find('[name=condition]').parent().parent().children()[0]);
                var conditionsbtn = $('<button>Additional params</button>').button({
                    icons: {
                        primary: "ui-icon-wrench"
                    },
                    text: false
                }).on('click', function () {
                    if (conditionsconcatenationtr.is(':visible')) {
                        conditionsconcatenationtr.hide();
                    } else {
                        conditionsconcatenationtr.show();
                    }
                });;
                conditionstd.append('&nbsp;');
                conditionstd.append(conditionsbtn);
                


            }
        };

        var form = new WorkflowDesignerForm(params);

        var validFunc = function (formControl, data) {
            var isValid = true;
            
            isValid &= formControl.CheckRequired([data], ['Name'], WorkflowDesignerConstants.FieldIsRequired);

            var reqfields = ['Type'];
            if (data.Trigger.Type == 'Command') {
                reqfields.push('Command.Name');
            }
            else if (data.Trigger.Type == 'Timer') {
                reqfields.push('Timer.Name');
            }

            isValid &= formControl.CheckRequired([data.Trigger], reqfields, WorkflowDesignerConstants.FieldIsRequired);

            data.Conditions.forEach(function (c) {
                reqfields = ['Type'];
                if (c.Type == 'Action') {
                    reqfields.push('Action.ActionName');
                }

                isValid &= formControl.CheckRequired([c], reqfields, WorkflowDesignerConstants.FieldIsRequired);

                if (c.Type == 'Always' && data.Conditions.length > 1) {
                    isValid = false;
                    formControl.ControlAddError(c['control_Type'],WorkflowDesignerConstants.AlwaysConditionShouldBeSingle);
                }
                else if (c.Type == 'Otherwise' && data.Conditions.length > 1) {
                    isValid = false;
                    formControl.ControlAddError(c['control_Type'], WorkflowDesignerConstants.OtherwiseConditionShouldBeSingle);
                }
            });
            var c = $(data.control_Conditions.parent().parent().children()[0]).children('label');
            if (!data.Conditions.length > 0) {
                c.attr('title', WorkflowDesignerConstants.TransitionFormLabel.ConditionsListShouldNotBeEmpty);
                c.css('color', 'red');
                isValid = false;
            } else {
                c.attr('title', undefined);
                c.css('color', '');
            }

            me.designer.data.Transitions.forEach(function (a) {
                if (a != me.item && a.Name == data.Name) {
                    isValid = false;
                    formControl.ControlAddError(data.control_Name, WorkflowDesignerConstants.FieldMustBeUnique);
                }
            });

            if (!formControl.CheckRequired(data.Restrictions, ['Type', 'Actor.Name'], WorkflowDesignerConstants.FieldIsRequired)) {
                isValid = false;
            }

            return isValid;
        }

        var saveFunc = function (data) {
            if (validFunc(form, data)) {
                form.ClearTempField(data);
                me.item.Name = data.Name;
                me.item.From.Name = data.From.Name;
                me.item.To.Name = data.To.Name;
                me.item.Classifier = data.Classifier;
                me.item.Restrictions = data.Restrictions;
                me.item.Trigger = data.Trigger;
                me.item.Conditions = data.Conditions;
                me.item.ConditionsConcatenationType = data.ConditionsConcatenationType;
                me.item.AllowConcatenationType = data.AllowConcatenationType;
                me.item.RestrictConcatenationType = data.RestrictConcatenationType;
                WorkflowDesignerCommon.DataCorrection(me.designer.data);
                me.designer.Draw(me.designer.data);
                return true;
            }
            return false;
        };

        form.showModal(saveFunc);
    };

    this.Sync = function () {
        me.item.DesignerSettings.Bending = me.bending;
    };

    this.destroy = function () {
        this.control.destroy();
        this.activePoint.destroy();
        this.touchpoints.forEach(function (tp) { tp.destroy() });
    };
}

function WorkflowDesignerTransitionTempControl(parameters) {
    this.x = parameters.x;
    this.y = parameters.y;
    this.manager = parameters.manager;
    this.control = undefined;

    this.Draw = function (xe, ye) {
        this.control = new Kinetic.Group({
            x: 0,
            y: 0,
            rotationDeg: 0
        });
        this.line = new Kinetic.Line({
            points: [this.x, this.y, xe, ye],
            stroke: '#FFCC99',
            strrokeWidth: 1
        });

        var angle = Math.atan2(ye - this.y, xe - this.x);
        this.arrow = WorkflowDesignerCommon.createArrowByAngle(xe, ye, angle, 20, '#FFCC99');

        this.control.add(this.line);
        this.control.add(this.arrow);

        this.manager.Layer.add(this.control);
    };

    this.Redraw = function (p) {
        this.line.setPoints([this.x, this.y, p.x, p.y]);
        var angle = Math.atan2(p.y - this.y, p.x - this.x);
        WorkflowDesignerCommon.updateArrowByAngle(this.arrow, p.x, p.y, angle, 20, '#FFCC99');

    };

    this.Delete = function () {
        this.control.destroy();
    };
}