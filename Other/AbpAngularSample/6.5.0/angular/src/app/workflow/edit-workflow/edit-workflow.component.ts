// WorkflowEngineSampleCode
import { Component, ViewChild} from '@angular/core';
import {ActivatedRoute} from "@angular/router";

import { WorkflowDesignerComponent } from '@optimajet/workflow-designer-angular';

@Component({
  selector: 'edit-workflow-modal',
  templateUrl: 'edit-workflow.component.html'
})
export class EditWorkflowComponent {

  @ViewChild(WorkflowDesignerComponent) workflowDesigner?: WorkflowDesignerComponent = undefined;

  schemeCode = undefined;
  processId = undefined;
  designerConfig = {
    renderTo: 'wf-designer',
    uploadFormId: 'wf-uploadFormId',
    uploadFileId:'wf-uploadFileId',
    apiurl: 'http://localhost:21021/designer/api',
    widthDiff: 0,
    heightDiff: 0
  };

  constructor(
    private route: ActivatedRoute
	) {
    
    var me = this;
    route.params.subscribe( params => {
      me.schemeCode = params["code"];
      me.processId = params["processid"];
    });
  }

  save(){
    this.workflowDesigner.schemeCode = this.schemeCode;
    var err = this.workflowDesigner.innerDesigner.validate();
    if (err != undefined && err.length > 0) {
        abp.notify.error("ERROR")
        console.log(err)
    }
    else {
        this.workflowDesigner.save(function () {
          abp.notify.info('The scheme is saved!')
        }, function(){ abp.notify.error("ERROR")});
    }
    this.workflowDesigner.innerDesigner.render();
  }

  download(){
    this.workflowDesigner.downloadScheme();
  }

  upload(){
    var me = this;
    var uploadCallback = function(){
      var url = me.workflowDesigner.innerDesigner.createurl('uploadscheme');
      var form = window.document.getElementById(me.designerConfig.uploadFormId) as HTMLFormElement;
      var formData = new FormData(form);
      var request = new XMLHttpRequest();
      request.open("POST", url);
      request.onload = function(e) {
          me.schemeupload(e.currentTarget);
      };
      request.send(formData);       
    }

    this.workflowDesigner.upload("schema", uploadCallback)
  }

  schemeupload(request){
    var data = {};
    var content = request.responseText;
    try
    {
        data = JSON.parse(content);
    }
    catch (exception) {
      abp.notify.error(content);
        return;
    }

    if (data["isError"]){
      abp.notify.error(data["errorMessage"]);
      return;
    }

    this.workflowDesigner.innerDesigner.data = data;
    this.workflowDesigner.innerDesigner.render();
    abp.notify.info('The scheme is uploaded!');
  }    

  ngOnDestroy(){
    if (this.workflowDesigner != undefined) {
      this.workflowDesigner.innerDesigner.destroy();
    }

  }
}
