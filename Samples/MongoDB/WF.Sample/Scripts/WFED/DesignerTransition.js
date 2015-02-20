function WorkflowDesignerTransition() {
    this.type = 'WorkflowDesignerTransition';

    this.init = function (designer) {
        this.designer = designer;
        this.Layer = new Kinetic.Layer();
        this.designer.Stage.add(this.Layer);
        this.Layer.setZIndex(2);

        this.designer = designer;
        this.APLayer = new Kinetic.Layer();
        this.designer.Stage.add(this.APLayer);
        this.APLayer.setZIndex(3);

        this.ImageDelete = new Image();
        this.ImageDelete.src = this.designer.Settings.imagefolder + 'designer.delete.png';
    };
    this.ItemControls = new Array();
    this.draw = function () {
        if (this.ItemControls != null) {
            this.ItemControls.forEach(function (control) {
                control.destroy();
            });
        }

        this.ItemControls = new Array();

        var me = this;

        if (this.designer.data.Transitions != undefined) {
            this.designer.data.Transitions.forEach(function (item) {
                var cActivity = me.designer.GetComponentByType('WorkflowDesignerActivity');
                var itemControl = new WorkflowDesignerTransitionControl({
                    from: cActivity.find(item.From),
                    to: cActivity.find(item.To),
                    item: item,
                    designer: me.designer,
                    manager: me
                });

                me.ItemControls.push(itemControl);
                itemControl.Draw();
            });
        }

        this.batchDraw();
    };

    this.batchDraw = function () {
        this.Layer.batchDraw();
        this.APLayer.batchDraw();
    };
    this.getIntersectingActivity = function (point) {
        var cActivity = this.designer.GetComponentByType('WorkflowDesignerActivity');
        return cActivity.getIntersectingActivity(point);
    };
    this.LayerSetOffset = function (x, y) {
        this.Layer.setOffset(x, y);
        this.APLayer.setOffset(x, y);
    };
    this.LayerScale = function (a) {
        this.Layer.setScale({ x: this.Layer.getScale().x + a, y: this.Layer.getScale().y + a });
        this.APLayer.setScale({ x: this.APLayer.getScale().x + a, y: this.APLayer.getScale().y + a });
    };
    this.LayerScaleNorm = function (a) {
        this.Layer.setScale({ x: 1, y: 1 });
        this.Layer.setOffset({ x: 0, y: 0 });

        this.APLayer.setScale({ x: 1, y: 1 });
        this.APLayer.setOffset({ x: 0, y: 0 });
    };

    this.DeselectAll = function () {
        this.ItemControls.forEach(function (item) {
            item.Deselect();
        });
    };
    this.GetSelected = function () {
        var res = new Array();
        this.ItemControls.forEach(function (item) {
            if (item.selected) res.push(item);
        });
        return res;
    };


    this.SelectByPosition = function (rect) {
        this.ItemControls.forEach(function (item) {
            if (item.getIntersectingRect(rect))
                item.Select();
        });
    };
    this.SelectByItem = function (obj) {
        this.ItemControls.forEach(function (item) {
            if (item.item == obj)
                item.Select();
        });
    };

    this.CreateNewTransition = function (fromA, toA) {
        var me = this;

        if (toA == undefined) {
            var xs = fromA.control.getX() + fromA.rectangle.attrs.width;
            var ys = fromA.control.getY() + fromA.rectangle.attrs.height / 2;

            var pos = { x: xs, y: ys };
            var tt = new WorkflowDesignerTransitionTempControl({ x: pos.x, y: pos.y, manager: this });
            tt.Draw(pos.x + 10, pos.y);

            this.batchDraw();

            var onMousemove = function (e) {
                var p = me.designer.CorrectPossition({ x: e.evt.layerX, y: e.evt.layerY }, me.Layer);
                tt.Redraw(p);
                me.Layer.batchDraw();
            };

            var onMouseup = function (e) {
                var pos = { x: e.evt.layerX, y: e.evt.layerY };
                var to = me.getIntersectingActivity(pos);
                if (to != undefined) {
                    me.CreateNewTransition(fromA, to);
                }
                tt.Delete();
                me.designer.Stage.off('mousemove.WorkflowDesignerTransitionTempControl', onMousemove);
                me.designer.Stage.off('mouseup.WorkflowDesignerTransitionTempControl', onMouseup);
                me.batchDraw();
            };

            this.designer.Stage.on('mousemove.WorkflowDesignerTransitionTempControl', onMousemove);
            this.designer.Stage.on('mouseup.WorkflowDesignerTransitionTempControl', onMouseup);

            return tt;
        }
        else
        {
            var item = {
                Name: this.GetDefaultName(fromA.GetName(), toA.GetName()),
                From: fromA.item,
                To: toA.item,
                Trigger:    { Type: 'Auto'   },
                Conditions: [{ Type: 'Always' }],
                AllowConcatenationType: 'And',
                RestrictConcatenationType: 'And',
                ConditionsConcatenationType: 'And',
                Classifier: 'Direct',
                DesignerSettings: {}
            };

            var itemControl = new WorkflowDesignerTransitionControl({
                from: fromA,
                to: toA,
                item: item,
                designer: me.designer,
                manager: me
            });

            me.ItemControls.push(itemControl);
            this.designer.data.Transitions.push(item);
            itemControl.Draw();
            return itemControl;
        }
    };

    this.GetDefaultName = function (a,b) {
        var name = a + '_' + b + '_';
        var index = 1;
        for (var i = 0; i < this.designer.data.Transitions.length; i++) {
            var item = this.designer.data.Transitions[i];
            if (item.Name == name + index) {
                index++;
                i = -1;
            }
        }
        return name + index;
    };
};