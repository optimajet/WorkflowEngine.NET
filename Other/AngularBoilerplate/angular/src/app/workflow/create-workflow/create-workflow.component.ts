// WorkflowEngineSampleCode
import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef, OnInit } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { WorkflowSchemeDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';
import {Router} from "@angular/router"

@Component({
    selector: 'create-workflow-modal',
    templateUrl: './create-workflow.component.html'
})
export class CreateWorkflowComponent extends AppComponentBase implements OnInit {
    @ViewChild('createWorkflowModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    active: boolean = false;
    saving: boolean = false;

    scheme: WorkflowSchemeDto = null;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    constructor(
        injector: Injector,
        private router: Router
    ) {
        super(injector);
    }

    ngOnInit(): void {
    
    }

    show(): void {
        this.active = true;
        this.scheme = new WorkflowSchemeDto();
        this.scheme.init({ });

        this.modal.show();
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    save(): void {
        this.router.navigate(['/app/workflowedit', this.scheme.code]);
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}
