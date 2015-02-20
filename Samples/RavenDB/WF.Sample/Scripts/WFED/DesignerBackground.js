function WorkflowDesignerBackground() {

    this.type = 'WorkflowDesignerBackground';
    this.init = function (designer) {
        var me = this;

        this.designer = designer;
        this.BackgroundLayer = new Kinetic.Layer();
        this.designer.Stage.add(this.BackgroundLayer);
        this.BackgroundLayer.setZIndex(0);

        this.SelectionLayer = new Kinetic.Layer();
        this.designer.Stage.add(this.SelectionLayer);
        this.SelectionLayer.setZIndex(1);


        var bgImage = new Image();
        bgImage.onload = function () {
            me.RectBG.setFillPatternImage(bgImage);
            me.BackgroundLayer.batchDraw();
        };
        bgImage.src = this.designer.Settings.imagefolder + 'grid.gif';

        this.RectBG = new Kinetic.Rect({
            x: 0,
            y: 0,
            width: 5000,
            height: 5000,
            draggable: false,
            dragBoundFunc: function (pos) {
                var kx = me.designer.Settings.DefaultMoveStep * me.BackgroundLayer.getScaleX();
                var ky = me.designer.Settings.DefaultMoveStep * me.BackgroundLayer.getScaleY();
                var correctpos = {
                    x: Math.round(pos.x / ky) * ky,
                    y: Math.round(pos.y / ky) * ky
                };

                me.designer.GraphLayerSetOffset(-correctpos.x / me.BackgroundLayer.getScaleX(), -correctpos.y / me.BackgroundLayer.getScaleY());
                return correctpos;
            },
            designerparam: 'background'
        });

        this.BackgroundLayer.add(this.RectBG);
        this.BackgroundLayer.batchDraw();

        this.designer.Stage.on('mousedown.background', function (evt) {
            if (!me._movemodeenabled && evt.target.attrs.designerparam == 'background') {
                me._mousedownpos = me.designer.CorrectPossition({ x: evt.evt.layerX, y: evt.evt.layerY }, me.SelectionLayer);
            }
        });

        this.designer.Stage.on('mousemove.backgrounde', function (evt) {
            if (!me._movemodeenabled && me._mousedownpos) {
                var pos = me.designer.CorrectPossition({ x: evt.evt.layerX, y: evt.evt.layerY }, me.SelectionLayer);
                me.DrawSelectionRect(pos);
            }
        });

        this.designer.Stage.on('mouseup.background', function (e) {
            if (!me._movemodeenabled && me._mousedownpos != undefined) {
                var pos = me.getSelectionRectPos();
                if (pos == undefined) {
                    if(!e.evt.ctrlKey)
                        me.designer.DeselectAll();
                }
                else if (Math.abs(pos.xl - pos.xr) > 10 || Math.abs(pos.yl - pos.yr) > 10) {
                    me.designer.DeselectAll();
                    me.designer.ComponentsExecute('SelectByPosition', pos);
                }
            }
            me._mousedownpos = undefined;
            me.DeleteSelectionRect();
        });
    };

    this.setMoveModeEnabled = function (flag) {
        this._movemodeenabled = flag;
        this.RectBG.setDraggable(this._movemodeenabled);
    };

    this.LayerScale = function (a) {
        this.BackgroundLayer.setScale({ x: this.BackgroundLayer.getScale().x + a, y: this.BackgroundLayer.getScale().y + a });
    }

    this.LayerScaleNorm = function (a) {
        this.BackgroundLayer.setScale({ x: 1, y: 1 });
        this.SelectionLayer.setScale({ x: 1, y: 1 });
        this.RectBG.setPosition({ x: 0, y: 0 });
    }

    this.DrawSelectionRect = function (pos) {
        if (!this.RectSelection) {
            this.RectSelection = new Kinetic.Rect({
                x: this._mousedownpos.x,
                y: this._mousedownpos.y,
                width: pos.x - this._mousedownpos.x,
                height: pos.y - this._mousedownpos.y,
                draggable: false,
                fill: '#66CCFF',
                opacity: 0.2
            });
            this.SelectionLayer.add(this.RectSelection);
        }
        else {

            this.RectSelection.setWidth(pos.x - this._mousedownpos.x);
            this.RectSelection.setHeight(pos.y - this._mousedownpos.y);
        }

        this.SelectionLayer.batchDraw();
    };

    this.DeleteSelectionRect = function (pos) {
        if (this.RectSelection) {
            this.RectSelection.destroy();
            this.RectSelection = undefined;
            this.SelectionLayer.batchDraw();
        }
    };

    this.getSelectionRectPos = function () {
        if (this.RectSelection == undefined)
            return undefined;
        var rectPos = this.RectSelection.getAbsolutePosition();
        var xl = rectPos.x;
        var yl = rectPos.y;
        var xr = xl + this.RectSelection.getWidth() * this.SelectionLayer.getScaleX();
        var yr = yl + this.RectSelection.getHeight() * this.SelectionLayer.getScaleX();
        return { xl: Math.min(xl, xr), yl: Math.min(yl, yr), xr: Math.max(xl, xr), yr: Math.max(yl, yr) };
    };
};