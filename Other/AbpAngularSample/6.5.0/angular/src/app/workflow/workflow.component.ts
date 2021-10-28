// WorkflowEngineSampleCode
import { Component, Injector, ViewChild } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { WorkflowSchemeServiceProxy, WorkflowSchemeDto, GetWorkflowSchemesOutput } from 'shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import {Router} from "@angular/router"

import { CreateWorkflowComponent } from 'app/workflow/create-workflow/create-workflow.component'

@Component({
  templateUrl: './workflow.component.html',
  animations: [appModuleAnimation()]
})
export class WorkflowComponent extends PagedListingComponentBase<WorkflowSchemeDto> {
	@ViewChild('createWorkflowModal') createWorkflowModal: CreateWorkflowComponent;
	
	schemes: WorkflowSchemeDto[] = [];
	constructor(
		private injector:Injector,
		private _workflowSchemeService: WorkflowSchemeServiceProxy,
		private router: Router,
		private _modalService: BsModalService
	) {
		super(injector);
	}
    
	list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
		this._workflowSchemeService.getSchemes()
            .pipe(finalize(() => { finishedCallback() }))
            .subscribe((result: GetWorkflowSchemesOutput)=>{
				this.schemes = result.schemes;
		});
	}

	delete(scheme: WorkflowSchemeDto): void {
		this._workflowSchemeService.delete(scheme.code)
                        .pipe(finalize(() => {
                            abp.notify.info("Deleted Scheme: " + scheme.code);
                            this.refresh();
                        }))
						.subscribe(() => { });
	}

	createScheme(): void {
		let createSchemeDialog: BsModalRef;
		createSchemeDialog = this._modalService.show(
			CreateWorkflowComponent,
			{
				class: 'modal-lg'
			}
		  );

		createSchemeDialog.content.onSave.subscribe(() => {
			this.refresh();
		});
	}
	
	editScheme(scheme:WorkflowSchemeDto): void {
		this.router.navigate(['/app/workflowedit', scheme.code]);
	}
}
