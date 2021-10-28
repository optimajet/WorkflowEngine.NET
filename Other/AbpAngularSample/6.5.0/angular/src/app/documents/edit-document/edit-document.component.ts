//WorkflowEngineSampleCode
import { Component, ViewChild, Injector, Output, OnInit, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { DocumentServiceProxy, DocumentDto, WorkflowHistoryDto, WorkflowSchemeServiceProxy, WorkflowCommandDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';

@Component({
    selector: 'edit-document-modal',
    templateUrl: './edit-document.component.html'
})
export class EditDocumentComponent extends AppComponentBase implements OnInit  {

    @Output() onSave = new EventEmitter<any>();

    id: number;
    active: boolean = false;
    saving: boolean = false;

    document: DocumentDto = null;
    history: WorkflowHistoryDto[];
    commands: WorkflowCommandDto[];

    constructor(
        injector: Injector,
        private _documentService: DocumentServiceProxy,
        private _workflowService: WorkflowSchemeServiceProxy,
        public bsModalRef: BsModalRef
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this._documentService.get(this.id)
            .subscribe(
            (result) => {
                this.document = result;

                this._workflowService.getAvaliableCommands(this.document.processId).subscribe((result2) => { this.commands = result2;});
                this._workflowService.getHistories(this.document.processId).subscribe((result3) => { this.history = result3; });
                this.active = true;
            });
    }

    executeCommand(command: WorkflowCommandDto): void {
        this.active = false;
        this._workflowService.executeCommand(this.document.processId, command.name).subscribe(
            (result: boolean) => {
                if(result){
                    this.bsModalRef.hide();
                    this.onSave.emit(null);
                }
                else{
                    this.active = true;
                    this.notify.error("Error the command execution!");
                }
            });
    }

    save(): void {
        this.saving = true;
        this._documentService.update(this.document)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.onSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.bsModalRef.hide();
    }
}
