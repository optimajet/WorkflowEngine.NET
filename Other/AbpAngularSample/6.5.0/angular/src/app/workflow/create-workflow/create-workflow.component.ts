// WorkflowEngineSampleCode
import { Component, Injector, Output, EventEmitter} from '@angular/core';
import { WorkflowSchemeDto} from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { BsModalRef } from 'ngx-bootstrap/modal';
import {Router} from "@angular/router"

@Component({
    selector: 'create-workflow-modal',
    templateUrl: './create-workflow.component.html'
})
export class CreateWorkflowComponent extends AppComponentBase{

    saving: boolean = false;
    scheme: WorkflowSchemeDto =  new WorkflowSchemeDto();

    @Output() onSave: EventEmitter<any> = new EventEmitter<any>();
    constructor(
        injector: Injector,
        private router: Router,
        public bsModalRef: BsModalRef,
    ) {
        super(injector);
    }

    save(): void {
        this.bsModalRef.hide()
        this.router.navigate(['/app/workflowedit', this.scheme.code]);
    }
}
