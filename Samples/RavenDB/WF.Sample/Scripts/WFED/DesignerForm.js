function WorkflowDesignerForm(parameters) {
    this.type = 'WorkflowDesignerForm';

    this.parameters = parameters;
    this.id = WorkflowDesignerCommon.createUUID();

    this.isReadOnly = function () {
        return this.parameters.readonly;
    }

    this.showModal = function (saveFunc) {
        var me = this;
        me.window = $('<div></div>');
        me.window.id = this.id;
        if (this.parameters.type == 'table') {
            var table = this.generateTable(this.parameters);
            me.window.append(table);
        }
        else if (this.parameters.type == 'form') {
            var form = this.generateForm(this.parameters);
            if (this.parameters.renderFinalFunc != undefined) {
                this.parameters.renderFinalFunc(form);
            }
            me.window.append(form);
        }

        me.window.dialog({
            modal: true,
            title: this.parameters.title,
            width: this.parameters.width,
            maxHeight: WorkflowDesignerConstants.FormMaxHeight,
            beforeClose: function (event, ui) {
                me.ClearError();
                if (saveFunc != undefined && !me.isReadOnly()) {
                    var data = me.getEditData(me.parameters);
                    return saveFunc(data, me.parameters);
                }
            },
            close: function(event, ui)
            {
                $(this).dialog('destroy').remove();
            }
        });
    };
    
    this.getEditData = function (p) {
        var me = this;
        var data;
        if (p.type == 'form') {
            data = {};
            p.elements.forEach(function (element) {
                data['control_' + element.field] = element.control;

                if (element.type == 'table' || element.type == 'form') {
                    data[element.field] = me.getEditData(element);
                }
                else {
                    me.SetValueByPropertyName(data, element.field,
                        me.getEasyControlValue(element));
                }
            });
        }
        else if (p.type == 'table') {
            data = [];
            var table = p.control;
            p.elements.forEach(function (element) {
                var code = me.getElementCode(element);
                var filter = '[name=' + code + ']';
                var elementControls = table.find(filter);
                if (elementControls != undefined) {
                    for (var i = 0; i < elementControls.length; i++) {
                        if (data[i] == undefined)
                            data[i] = {};
                        data[i]['control_' + element.field] = elementControls[i];

                        if (element.type == 'table' || element.type == 'form') {
                            data[i][element.field] = me.getEditData({ type: element.type, control: $(elementControls[i]), elements: element.elements });
                        }
                        else {
                            me.SetValueByPropertyName(data[i], element.field,
                                me.getEasyControlValue({ type: element.type, control: elementControls[i] }));
                        }
                    }
                }
            });

            if (p.keyproperty) {
                var trArray = table.children('tbody').children('tr');
                
                for (var i = 0; i < trArray.length; i++) {
                    if (data[i] == undefined)
                        data[i] = {};

                    data[i].keyproperty = $(trArray[i]).attr('keyproperty');
                }
            }
        }

        return data;
    }

    this.generateForm = function (p, prefix) {
        var me = this;
       
        var parent = $('<table class="WorkflowDesignerTable"></table>');
        parent.attr('name', me.getElementCode(p));
        var res = new Array();
        p.elements.forEach(function (element) {
            var row = $('<tr></tr>');
            
            if (prefix == undefined) prefix = '';
            var elementPrefix = prefix + '_' + element.field;

            var label = $('<label>oooo</label>');
            label[0].innerHTML = element.name;
            row.append($('<td></td>').append(label));
            
            if (element.type == 'table') {

                if (element.fieldFunc) {
                    element.data = element.fieldFunc(p.data);
                } else {
                    element.data = p.data[element.field];
                }

                var table = me.generateTable(element, elementPrefix);
                row.append($('<td></td>').append(table));
            }
            else if (element.type == 'form') {

                if (element.fieldFunc) {
                    element.data = element.fieldFunc(p.data);
                } else {
                    element.data = p.data[element.field];
                }

                var table = me.generateForm(element, elementPrefix);
                row.append($('<td></td>').append(table));
            }
            else {
                var control = me.generateEasyControls(element,
                    me.GetValueByPropertyName(p.data, element.field),
                    elementPrefix);
                label[0].for = control[0].id;
                element.control = control[0];
                row.append($('<td></td>').append(control[0]));
            }

            res.push(row);
        });
        p.control = parent;
        parent.append(res);
        return parent;
    };
    this.generateTable = function (p, prefix) {
        var me = this;

        var table = $('<table class="WorkflowDesignerTable"></table>');
        table.attr('name', me.getElementCode(p));

        var tablethead = $('<thead></thead>');
        var headtr = $('<tr></tr>');

        p.elements.forEach(function (c) {
            var headth = $('<th></th>');
            headth[0].innerHTML = c.name;
            if (c.width != undefined)
                headth[0].width = c.width;
            headtr.append(headth);
        });

        if (!this.isReadOnly()) {
            headtr.append('<th></th>');
        }

        tablethead.append(headtr);
        table.append(tablethead);

        var addrow = function (item) {
            var row = $('<tr></tr>');
            if (p.keyproperty)
                row.attr('keyproperty', item[p.keyproperty]);

            if (prefix == undefined) prefix = '';
            var prefixElement = prefix + WorkflowDesignerCommon.createUUID();

            p.elements.forEach(function (e) {
                if (e.type == 'table') {
                    if (e.fieldFunc) {
                        e.data = e.fieldFunc(item);
                    } else {
                        e.data = item[e.field];
                    }

                    var table = me.generateTable(e, prefixElement);
                    row.append($('<td></td>').append(table));
                }                
                else {
                    var control = me.generateEasyControls(e, me.GetValueByPropertyName(item, e.field), prefixElement, item);
                    row.append($('<td></td>').append(control));
                }
            });

            if (!me.isReadOnly()) {
                var deletebutton = $('<a class="btnDelete"></a>');
                deletebutton[0].innerHTML = WorkflowDesignerConstants.ButtonTextDelete;
                deletebutton[0].href = "#"
                deletebutton.on('click', function () {
                    row.remove();
                });
                row.append($('<td></td>').append(deletebutton));
            }

            table.append(row);

            if (p.onrowadded != undefined)
                p.onrowadded(row);

        };
        
        if (p.data != undefined) {
            p.data.forEach(function (item) {
                addrow(item);
            });
        }

        p.control = table;

        var res = new Array();
        res.push(table);
        if(!this.isReadOnly()){
            var createbutton = $('<a class="btnAdd"></a>');
            createbutton[0].innerHTML = WorkflowDesignerConstants.ButtonTextCreate;
            createbutton[0].href = "#";
            createbutton.on('click', function(){
                var defaultdata = {};
                if(p.datadefault)
                    defaultdata = p.datadefault;
                addrow(defaultdata);
            });
            res.push(createbutton);
        }
        return res;
    };

  

    this.generateEasyControls = function (p, value, prefix, item) {
        var me = this;
        if (p.type == 'input') {
            var control = $('<input style="width: 100%;"></input>');
            control[0].id = this.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);

            if(value != undefined)
                control[0].value = value;

            if (me.isReadOnly())
                control.attr('readonly', true);

            return control;
        }
        else if (p.type == 'code') {

            if (value == undefined)
                value = '';
            var control = $('<button>' + WorkflowDesignerConstants.EditCodeLabel.EditCodeButton + '</button>');
            control[0].id = this.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);
            control[0].code = {};
            control[0].code.code = (decodeURIComponent(value));
            var usings = (item.Usings);

            if (usings == undefined) {
                usings = me.parameters.designer.data.AdditionalParams.Usings.join(';') + ';';
            } else {
                usings = (decodeURIComponent(usings));
            }


            control[0].code.usings = usings;

            var form = $('<div title="Edit code">' +

                '<div id="' + control[0].id + '_usings' + '" style="width:inherit;"><h3>' + WorkflowDesignerConstants.EditCodeLabel.Usings + '</h3><textarea rows="10" style="width: 100%;" id="'
                + control[0].id + '_usingsedit' + '">' + '</textarea></div>' +
            
                '<div id="' + control[0].id + '_function_upper' + '" style="width:inherit;">'  + '</div>' +

                '<div id="' + control[0].id + '_editor' + '" style="height:600px;width:inherit;">' + this.htmlEncode(control[0].code.code) + '</div>' +


                '<div id="' + control[0].id + '_function_lower' + '" style="width:inherit;">' +'}' + '</div>' +
                '</div>');

            form[0].id = control[0].id + '_form';

            if (me.isReadOnly())
                control.attr('disabled', true);
            control.on('click',
                function (event) {
                   
                   var dialog  = form.dialog({
                        autoOpen: false,
                        height: 790,
                        width: 1000,
                        modal: true,
                        buttons: {
                            "Compile": function () {
                                var items = me.getEditData(me.parameters);
                                var item = undefined;
                                for (var i = 0; i < items.length; i++) {
                                    if (items[i].control_ActionCode.id == control[0].id) {
                                        item = items[i];
                                        break;
                                    }
                                }

                                if (item == undefined)
                                    return;
                                item.ActionCode = encodeURIComponent(ace.edit(control[0].id + '_editor').getValue());
                                item.Usings = encodeURIComponent($('#' + control[0].id + '_usingsedit')[0].value.replace(/(\r\n|\n|\r)/gm, ""));
                                var callbackfn = function(response) {
                                    $('<div title="' + (response.Success ? "Success" : "Error") + '">' + (response.Success ? "Compillation succeeded" : response.Message) + '</div>').dialog({
                                        modal: true,
                                        height: response.Success ? 150 : 500,
                                        width:  response.Success ? 200 : 500,
                                        buttons: {
                                            Ok: function () {
                                                $(this).dialog("close");
                                            }
                                        }
                                    });
                               }

                                me.parameters.designer.designer.compile(item, callbackfn);
                            }
                        },
                        close: function () {
                            control[0].code = {};
                           
                            control[0].code.code = (ace.edit(control[0].id + '_editor').getValue());
                            control[0].code.usings = ($('#' + control[0].id + '_usingsedit')[0].value.replace(/(\r\n|\n|\r)/gm, ""));
                        }
                   });

                   $('#' + control[0].id + '_usingsedit')[0].value = me.htmlEncode(me.modifyUsingString((control[0].code.usings)));

                    $('#' + control[0].id + '_usings').accordion({
                       collapsible: true,
                       active: false,
                       heightStyle: "content"
                   });

                   var typevalue = $('#' + me.generateid('Type', prefix))[0].value.toLowerCase();
                   var namevalue = $('#' + me.generateid('Name', prefix))[0].value;

                   if (namevalue === '')
                       namevalue = '???';

                   var functionupper = '{';
                   if (typevalue === 'action') {
                       functionupper = 'void ' + namevalue + ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter) {';
                   }
                   if (typevalue === 'condition') {
                       functionupper = 'bool ' + namevalue + ' (ProcessInstance processInstance, WorkflowRuntime runtime, string parameter) {';
                   }
                   if (typevalue === 'ruleget') {
                       functionupper = 'IEnumerable&lt;string&gt; ' + namevalue + ' (Guid processId, string parameter) {';
                   }

                   if (typevalue === 'rulecheck') {
                       functionupper = 'bool ' + namevalue + ' (Guid processId, string identityId, string parameter) {';
                   }


                   $('#' + control[0].id + '_function_upper').html(functionupper);

                    var editor = ace.edit(control[0].id + '_editor');
                    editor.setTheme("ace/theme/monokai");
                    editor.getSession().setMode("ace/mode/csharp");
                    dialog.dialog("open");
                 });

            return control;
        }
        else if (p.type == 'checkbox') {
            var control = $('<input style="width: 100%;"></input>');
            control[0].type = 'checkbox';
            control[0].id = this.generateid(p.field, prefix);
            control[0].checked = value;
            control[0].name = me.getElementCode(p);
            
            if (me.isReadOnly())
                control.attr('disabled', "disabled");

            return control;
        }
        else if (p.type == 'select') {
            var control = $('<select style="width: 100%;"></select>');
            control[0].id = this.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);
            control.append($('<option></option>'));
            if (p.datasource != undefined) {
                p.datasource.forEach(function (item) {
                    var option = $('<option></option>');
                    if (p.displayfield == undefined) {
                        option[0].value = item;
                        option[0].innerHTML = item;
                    }
                    else {
                        option[0].value = item[p.displayfield];
                        option[0].innerHTML = item[p.displayfield];
                    }

                    if (option[0].value == value) {
                        option[0].selected = "selected";
                    }

                    if (me.isReadOnly())
                        control.attr('readonly', true);

                    control.append(option);
                });
            }
            

            return control;
        }
        else if (p.type == 'textarea') {
            var control = $('<textarea rows="6" style="width: 100%;"></textarea>');
            control[0].id = me.generateid(p.field, prefix);
            control[0].name = me.getElementCode(p);

            if (value != undefined)
                control[0].value = value;

            if (this.isReadOnly())
                control.attr('readonly', true);

            return control;
        }
    };

    this.modifyUsingString = function (usings) {
        var lastsymbol = usings.substring(usings.length - 1);

        if (lastsymbol === ';')
            usings = usings.substring(0, usings.length - 1);

        return usings.split(';').join(';\r\n') + ';';
    }

    this.getEasyControlValue = function (p) {
        var me = this;
        if (p.type == 'input') {
            return p.control.value;
        }
        else if (p.type == 'code') {
            return p.control.code;
        }
        else if (p.type == 'checkbox') {
            return p.control.checked;
        }
        else if (p.type == 'select') {
            return p.control.value;
        }
        else if (p.type == 'textarea') {
            return p.control.value;
        }
    };

    this.generateid = function (name, prefix) {
        if (prefix)
            return name + '_' + prefix + '_' + this.id;
        else
            return name + '_' + this.id;
    };

    this.GetValueByPropertyName = function (item, propertyName) {
        if (item == undefined)
            return undefined;

        if (propertyName.indexOf('.') < 0)
        {
            return item[propertyName];
        }
        else {
            var currValue = item;
            propertyName.split('.').forEach(function (p) {
                if (currValue != undefined)
                    currValue = currValue[p];
            });
            return currValue;
        }
    };

    this.SetValueByPropertyName = function (item, propertyName, value) {
        if (propertyName.indexOf('.') < 0) {
            return item[propertyName] = value;
        }
        else {
            var currValue = item;
            var tmp = propertyName.split('.');
            for (var i = 0; i < tmp.length; i++) {
                var p = tmp[i];
                if(i == tmp.length - 1)
                    currValue[p] = value;
                else
                {
                    if (currValue[p] == undefined)
                        currValue[p] = {};
                    currValue = currValue[p];
                }
            }
        }
    };

    this.ClearError = function () {
        var controls = this.window.find('.field-validation-error');
        controls.attr('title', '');
        controls.removeClass('field-validation-error');
    };

    this.ControlAddError = function (control, msg) {
        var c = $(control);
        c.addClass('field-validation-error');
        c.attr('title', msg);
    };
    this.CheckRequired = function (items, properties, msg) {
        var me = this;
        var isSuccess = true;
        items.forEach(function (item) {
            properties.forEach(function (p) {
                if (me.GetValueByPropertyName(item, p) == '') {
                    me.ControlAddError(item['control_' + p], msg);
                    isSuccess = false;
                }
            });
        });

        return isSuccess;
    };

    this.CheckUnique = function (items, properties, msg) {
        var me = this;
        var isSuccess = true;

        for (var i = 0; i < items.length; i++) {
            for (var j = i + 1; j < items.length; j++) {
                if(this._checkUniqueEquals(items[i], items[j], properties))
                {
                    properties.forEach(function (p) {
                        me.ControlAddError(items[i]['control_' + p], msg);
                        me.ControlAddError(items[j]['control_' + p], msg);
                    });
                    isSuccess = false;
                }
            }
        }

        return isSuccess;
    };
    this._checkUniqueEquals = function (a, b, properties) {
        for (var i = 0; i < properties.length; i++) {
            var p = properties[i];
            if (a[p] != b[p]) {
                return false;
            }
        }
        return true;
    };
    var me = this;

    this.ClearTempField = function (data, elements) {
        if (data == undefined)
            return;

        if (elements == undefined)
            elements = this.parameters.elements;

        elements.forEach(function (e) {
            if ($.isArray(data)) {
                data.forEach(function (item) {
                    me.ClearTempField(item, elements);
                });
            }
            else {
                if (data['control_' + e.field] != undefined)
                    data['control_' + e.field] = undefined;
            }

            if (e.elements) {
                me.ClearTempField(data[e.field], e.elements);
            }
        });
    };

    this.getElementCode = function(element) {
        if (element.code != undefined)
            return element.code;
        return element.field;
    };

    this.htmlEncode = function (value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    }

    this.htmlDecode = function (value) {
        return $('<div/>').html(value).text();
    }
}