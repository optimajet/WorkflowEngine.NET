function WorkflowDesignerTooltip(layer, obj, text, includeInObj) {
    var me = this;

    obj.on('mouseover', function () {
        if (obj.ToolTip != undefined) {
            return;
        }

        var tooltip = new Kinetic.Label({
            x: obj.getX() + obj.getWidth() / 2,
            y: obj.getY() + 30,
            opacity: 0.75
        });

        tooltip.add(new Kinetic.Tag({
            fill: 'black',
            pointerDirection: 'up',
            pointerWidth: 10,
            pointerHeight: 10,
            lineJoin: 'round',
            shadowColor: 'black',
            shadowBlur: 10,
            shadowOffset: 10,
            shadowOpacity: 0.5
        }));

        tooltip.add(new Kinetic.Text({
            text: text,
            fontFamily: 'Calibri',
            fontSize: 12,
            padding: 5,
            fill: 'white'
        }));

        if (includeInObj) {
            tooltip.attrs.x = 0;
            tooltip.attrs.y = 15;
            obj.add(tooltip);
        }
        else {
            layer.add(tooltip);
        }

        obj.ToolTip = tooltip;
        layer.batchDraw();
    });

    obj.on('mouseleave', function () {
        me.Destroy(obj);
        layer.batchDraw();
    });

    this.Destroy = function (obj) {
        if (obj.ToolTip != undefined) {
            obj.ToolTip.destroy();
            obj.ToolTip = undefined;
        }
    };
}