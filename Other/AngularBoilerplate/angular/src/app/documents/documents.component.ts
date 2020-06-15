// WorkflowEngineSampleCode
import { Component, Injector, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { DocumentServiceProxy, DocumentDto, PagedResultDtoOfDocumentDto } from '@shared/service-proxies/service-proxies';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { CreateDocumentComponent } from 'app/documents/create-document/create-document.component';
import { EditDocumentComponent } from 'app/documents/edit-document/edit-document.component';
import { finalize } from 'rxjs/operators';

@Component({
    templateUrl: './documents.component.html',
    animations: [appModuleAnimation()]
})
export class DocumentsComponent extends PagedListingComponentBase<DocumentDto> {

    @ViewChild('createDocumentModal') createDocumentModal: CreateDocumentComponent;
    @ViewChild('editDocumentModal') editDocumentModal: EditDocumentComponent;

    active: boolean = false;
    documents: DocumentDto[] = [];

    constructor(
        injector: Injector,
        private _documentService: DocumentServiceProxy
    ) {
        super(injector);
    }

    protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
        this._documentService.getAll(undefined, undefined, undefined, request.skipCount, request.maxResultCount)
            .pipe(finalize(() => {
                 finishedCallback()
            }))
            .subscribe((result: PagedResultDtoOfDocumentDto) => {
                this.documents = result.items;
                this.showPaging(result, pageNumber);
            });
    }

    protected delete(document: DocumentDto): void {
        abp.message.confirm(
            "Delete document '" + document.title + "'?",
            (result: boolean) => {
                if (result) {
                    this._documentService.delete(document.id)
                        .subscribe(() => {
                            abp.notify.info("Deleted Document: " + document.title);
                            this.refresh();
                        });
                }
            }
        );
    }

    // Show Modals
    createDocument(): void {
        this.createDocumentModal.show();
    }

    editDocument(document: DocumentDto): void {
        this.editDocumentModal.show(document.id);
    }
}
