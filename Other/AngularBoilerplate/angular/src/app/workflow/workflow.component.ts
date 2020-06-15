// WorkflowEngineSampleCode
import { Component, Injector, ViewChild } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { WorkflowSchemeServiceProxy, WorkflowSchemeDto, GetWorkflowSchemesOutput } from 'shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
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
		private workflowSchemeService: WorkflowSchemeServiceProxy,
		private router: Router
	) {
		super(injector);
	}
    
	list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
		this.workflowSchemeService.getSchemes()
            .pipe(finalize(() => { finishedCallback() }))
            .subscribe((result: GetWorkflowSchemesOutput)=>{
				this.schemes = result.schemes;
		});
	}

	delete(scheme: WorkflowSchemeDto): void {
		this.workflowSchemeService.delete(scheme.code)
                        .pipe(finalize(() => {
                            abp.notify.info("Deleted Scheme: " + scheme.code);
                            this.refresh();
                        }))
						.subscribe(() => { });
	}

	// Show Modals
	createScheme(): void {
		this.createWorkflowModal.show();
	}

	editScheme(scheme:WorkflowSchemeDto): void {
		this.router.navigate(['/app/workflowedit', scheme.code]);
	}
}
