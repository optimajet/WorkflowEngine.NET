// WorkflowEngineSampleCode
import { Component, Injector, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { DocumentServiceProxy, DocumentDto, PagedResultDtoOfDocumentDto } from '@shared/service-proxies/service-proxies';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { CreateDocumentComponent } from 'app/documents/create-document/create-document.component';
import { EditDocumentComponent } from 'app/documents/edit-document/edit-document.component';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';

@Component({
    templateUrl: './documents.component.html',
    animations: [appModuleAnimation()]
})
export class DocumentsComponent extends PagedListingComponentBase<DocumentDto> {
    active: boolean = false;
    documents: DocumentDto[] = [];

    constructor(
        injector: Injector,
        private _documentService: DocumentServiceProxy,
        private _modalService: BsModalService,
        private router: Router,
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
            "qwerty",
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
        let createDocument: BsModalRef;
		createDocument = this._modalService.show(
			CreateDocumentComponent,
			{
                class: 'modal-lg'
			}
		  );

        createDocument.content.onSave.subscribe(() => {
			this.refresh();
		});
    }

    editDocument(document: DocumentDto): void {
        let editDocument: BsModalRef;
		editDocument = this._modalService.show(
			EditDocumentComponent,
			{
                class: 'modal-lg',
                initialState: {
                    id: document.id,
                },
			}
		  );

          editDocument.content.onSave.subscribe(() => {
			this.refresh();
		});
    }

    showScheme(document: DocumentDto): void {
        this.router.navigate(['/app/workflowedit/process', document.processId]);
    }
}
