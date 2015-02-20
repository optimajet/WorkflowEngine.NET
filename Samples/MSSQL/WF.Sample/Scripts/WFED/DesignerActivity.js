function WorkflowDesignerActivity() {

    this.type = 'WorkflowDesignerActivity';
    
    this.init = function (designer) {
        this.designer = designer;
        this.Layer = new Kinetic.Layer();
        this.designer.Stage.add(this.Layer);
        this.Layer.setZIndex(1);

        this.ImageCreateTransitionAndActivity = new Image();
        this.ImageCreateTransitionAndActivity.src = this.designer.Settings.imagefolder + 'designer.createat.png';

        this.ImageCreateTransition = new Image();
        this.ImageCreateTransition.src = this.designer.Settings.imagefolder + 'designer.createt.png';

        this.ImageDeleteActivity = new Image();
        this.ImageDeleteActivity.src = this.designer.Settings.imagefolder + 'designer.delete.png';
    };
    this.ItemControls = new Array();
    this.draw = function () {

        if (this.ItemControls != null) {
            this.ItemControls.forEach(function (control) {
                control.control.destroy();
            });
        }

        this.ItemControls = new Array();

        var me = this;

        if (this.designer.data.Activities != undefined) {
            this.designer.data.Activities.forEach(function (item) {
                var actX, actY;

                if (item.DesignerSettings.X != '' && item.DesignerSettings.Y != '') {
                    actX = Number(item.DesignerSettings.X);
                    actY = Number(item.DesignerSettings.Y);                   
                }
                else {
                    var position = this.GetDefaultPosition();
                    actX = position.x;
                    actY = position.y;
                }

                var itemControl = new WorkflowDesignerActivityControl({
                    x: actX,
                    y: actY,
                    item: item,
                    designer: me.designer,
                    manager: me
                });

                me.ItemControls.push(itemControl);
                itemControl.Draw();
                itemControl.Sync();
            });
        }

        this.Layer.batchDraw();
    };
    this.CreateNewActivity = function (position, item) {
        if (position == undefined) {
            position = this.GetDefaultPosition();
        }

        if (item == undefined)
            item = {IsAutoSchemeUpdate: true, IsForSetState: true};

        if (item.Name == undefined)
            item.Name = this.GetDefaultName();

        if (this.designer.data.Activities.length == 0)
            item.IsInitial = true;

        if (item.DesignerSettings) {
            item.DesignerSettings = new {
                X: position.x,
                Y: position.y
            }
        }

        var itemControl = new WorkflowDesignerActivityControl({
            x: position.x,
            y: position.y,
            item: item,
            designer: this.designer,
            manager: this
        });

        this.designer.data.Activities.push(item);
        this.ItemControls.push(itemControl);
        itemControl.Draw();
        itemControl.Sync();
        
        return itemControl;
    };

    this.GetDefaultName = function () {
        var name = WorkflowDesignerConstants.ActivityNamePrefix;
        var index = 1;
        for (var i = 0; i < this.designer.data.Activities.length; i++) {
            var item = this.designer.data.Activities[i];
            if (item.Name == name + index) {
                index++;
                i = -1;
            }
        }
        return name + index;
    };
    this.GetDefaultPosition = function () {
        var position = this.designer.CorrectPossition(
            {
                x: 50,
                y: 50
            }, this.Layer);

        var step = this.designer.Settings.DefaultMoveStep / this.Layer.getScaleX();

        for (var i = 0; i < this.ItemControls.length; i++) {
            var pos = this.ItemControls[i].control.getPosition();
            if (pos.x == position.x && pos.y == position.y) {
                position.x += step;
                position.y += step;
                i = -1;
            }
        }
        return position;
    };

    this.find = function (name) {
        if (typeof (name) == 'object' && name.Name != undefined)
            return this.find(name.Name);

        for (var i = 0; i < this.ItemControls.length; i++) {
            if (this.ItemControls[i].GetName() == name)
                return this.ItemControls[i];
        }
        return undefined;
    };
    this.getIntersectingActivity = function (point) {
        for (var i = 0; i < this.ItemControls.length; i++) {
            var a = this.ItemControls[i];
            if (a.getIntersectingActivity(point))
                return a;
        }

        return undefined;
    };

    this.LayerSetOffset = function (a) {
        this.Layer.setOffset(a);
    };
    this.LayerScale = function (a) {
        this.Layer.setScale({ x: this.Layer.getScale().x + a, y: this.Layer.getScale().y + a });
    };
    this.LayerScaleNorm = function (a) {
        this.Layer.setScale({ x: 1, y: 1 });
        this.Layer.setOffset({ x: 0, y: 0 });
    };

    this.redrawTransitions = function () {
        if (this.cTransition == undefined) {
            this.cTransition = this.designer.GetComponentByType('WorkflowDesignerTransition');
        }
        return this.cTransition.batchDraw();
    };
    this.batchDraw = function () {
        this.Layer.batchDraw();
    };

    this.DeselectAll = function () {
        this.ItemControls.forEach(function (item) {
            item.Deselect();
        });
    };

    this.GetSizeForSaveAsImage = function (size) {
        for (var i = 0; i < this.ItemControls.length; i++) {
            var a = this.ItemControls[i];
            var rectPos = a.rectangle.getAbsolutePosition();
            var xl = rectPos.x;
            var yl = rectPos.y;
            var xr = xl + a.rectangle.getWidth() * this.Layer.getScaleX();
            var yr = yl + a.rectangle.getHeight() * this.Layer.getScaleX();

            if (xl < size.x1) size.x1 = xl;
            if (yl < size.y1) size.y1 = yl;

            if (xr > size.x2) size.x2 = xr;
            if (yr > size.y2) size.y2 = yr;

        }

        return size;
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
            if (item.getIntersectingActivityRect(rect))
                item.Select();
        });
    };

    this.SelectByItem = function (obj) {
        this.ItemControls.forEach(function (item) {
            if (item.item == obj)
                item.Select();
        });
    };

    this.ObjectMove = function (e) {
        this.ItemControls.forEach(function (item) {
            if (item.selected && e.sender != item) {
                item.ObjectMove(e.changepos);
            }
        });

        this.redrawTransitions();
    };

    this.createTransitionAndActivity = function (a) {
        var pos = {
            x: a.control.getX() + a.rectangle.attrs.width * 2,
            y: a.control.getY()
        };

        var toA = this.CreateNewActivity(pos);
        var cTransition = this.designer.GetComponentByType('WorkflowDesignerTransition');
        cTransition.CreateNewTransition(a, toA);
        this.designer.redrawAll();
    };
    this.createTransition = function (a) {
        var cTransition = this.designer.GetComponentByType('WorkflowDesignerTransition');
        return cTransition.CreateNewTransition(a);
    };
};