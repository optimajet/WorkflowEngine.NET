// WorkflowEngineSampleCode
import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef, OnInit } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { DocumentServiceProxy, CreateDocumentDto, WorkflowSchemeServiceProxy, SchemeForDocumentOutput, WorkflowSchemeDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';
import { getTime } from 'ngx-bootstrap/chronos/utils/date-getters';

@Component({
  selector: 'create-document-modal',
  templateUrl: './create-document.component.html'
})
export class CreateDocumentComponent extends AppComponentBase implements OnInit {

    @ViewChild('createDocumentModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;
    document: CreateDocumentDto = null;
    schemes: SchemeForDocumentOutput[];

    constructor(
        injector: Injector,
        private _documentService: DocumentServiceProxy,
        private _schemeService : WorkflowSchemeServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this._documentService.getSchemes()
            .subscribe((result: SchemeForDocumentOutput[])=>{
				this.schemes = result;
		});
    }

    show(): void {
        this.active = true;
        this.document = new CreateDocumentDto();
        this.document.init({ });
        this.modal.show();
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    save(): void {
        this.saving = true;
        this._documentService.create(this.document)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}
