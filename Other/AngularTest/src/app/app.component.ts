import { Component } from '@angular/core';

declare let WorkflowDesigner: any;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'AngularTest';
  
  wfdesigner: any; 

  constructor() {
    var wfdesigner = new WorkflowDesigner({
      name: 'simpledesigner',
      apiurl: '/Designer/API',
      renderTo: 'wfdesigner',
      imagefolder: '/Images/',
      graphwidth: 1200,
      graphheight: 600
  });

  var schemecode = 'SimpleWF';
  var processid = '';
  var p = { schemecode: schemecode, processid: processid};
  if (wfdesigner.exists(p))
    wfdesigner.load(p);
  else
    wfdesigner.create();
    }

}

