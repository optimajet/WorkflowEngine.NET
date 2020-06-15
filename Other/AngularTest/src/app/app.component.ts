import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'AngularTest';
  schemecode = 'SimpleWF';
  processid = undefined;
  //TODO REPLACE THIS URL TO YOURS BACKEND (!!!)
  apiurl = 'https://workflowengine.io/demo/Designer/API'; //'/Designer/API'
  offsetX = 0;
  offsetY = 120;
  wfdesigner: any; 

  constructor() {
    
  }

  ngOnInit(){
    var me = this;
    me.wfdesignerRedraw();
    window.onresize = function(event) {
      if(me != undefined && me.wfdesigner != undefined){
        me.wfdesignerRedraw();
      }
    };
  }
    
  wfdesignerRedraw() {
    let data = undefined;
    if (this.wfdesigner != undefined) {
        data = this.wfdesigner.data;
        this.wfdesigner.destroy();
    }

    this.wfdesigner = new window["WorkflowDesigner"]({
      name: 'simpledesigner',
      apiurl: this.apiurl,
      renderTo: 'wfdesigner',
      imagefolder: '/assets/workflow/images/',
      graphwidth: window.innerWidth - this.offsetX,
      graphheight: window.innerHeight - this.offsetY
    });

    if (data == undefined) {
      let isreadonly = false;
      if (this.processid != undefined && this.processid != '')
          isreadonly = true;

      let p = { schemecode: this.schemecode, processid: this.processid, readonly: isreadonly };

      if (this.wfdesigner.exists(p))
        this.wfdesigner.load(p);
      else
        this.wfdesigner.create();
    }
    else {
      this.wfdesigner.data = data;
      this.wfdesigner.render();
    }
  }
}

