//WorkflowEngineSampleCode
import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { DocumentServiceProxy, DocumentDto, WorkflowHistoryDto, WorkflowSchemeServiceProxy, WorkflowCommandDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'edit-document-modal',
    templateUrl: './edit-document.component.html'
})
export class EditDocumentComponent extends AppComponentBase {

    @ViewChild('editDocumentModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;

    document: DocumentDto = null;
    history: WorkflowHistoryDto[];
    commands: WorkflowCommandDto[];

    constructor(
        injector: Injector,
        private _documentService: DocumentServiceProxy,
        private _workflowService: WorkflowSchemeServiceProxy
    ) {
        super(injector);
    }

    show(id: number): void {
        this._documentService.get(id)
            .subscribe(
            (result) => {
                this.document = result;

                this._workflowService.getAvaliableCommands(this.document.processId).subscribe((result2) => { this.commands = result2; });
                this._workflowService.getHistories(this.document.processId).subscribe((result3) => { this.history = result3; });

                this.active = true;
                this.modal.show();
            });
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    executeCommand(command: WorkflowCommandDto): void {
        this.active = false;
        this._workflowService.executeCommand(this.document.processId, command.name).subscribe(
            (result: boolean) => {
                if(result){
                    this.modal.hide();
                    this.show(this.document.id);
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
                this.modalSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}
