// WorkflowEngineSampleCode
import { Component, Injector, Output, EventEmitter, OnInit } from '@angular/core';
import { DocumentServiceProxy, CreateDocumentDto, SchemeForDocumentOutput } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'create-document-modal',
  templateUrl: './create-document.component.html'
})
export class CreateDocumentComponent extends AppComponentBase implements OnInit {

    @Output() onSave: EventEmitter<any> = new EventEmitter<any>();
    saving: boolean = false;
    document: CreateDocumentDto = new CreateDocumentDto();
    schemes: SchemeForDocumentOutput[];
    constructor(
        injector: Injector,
        private _documentService: DocumentServiceProxy,
        public bsModalRef: BsModalRef
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this._documentService.getSchemes()
            .subscribe((result: SchemeForDocumentOutput[])=>{
				this.schemes = result;
		});
    }

    save(): void {
        this.saving = true;
        this._documentService.create(this.document)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.onSave.emit(null);
            });
    }

    close(): void {
        this.bsModalRef.hide()
    }
}
