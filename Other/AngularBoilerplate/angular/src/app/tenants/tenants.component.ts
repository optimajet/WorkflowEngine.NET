import { Component, Injector, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { TenantServiceProxy, TenantDto, PagedResultDtoOfTenantDto } from '@shared/service-proxies/service-proxies';

import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { EditTenantComponent } from 'app/tenants/edit-tenant/edit-tenant.component';
import { CreateTenantComponent } from 'app/tenants/create-tenant/create-tenant.component';
import { finalize } from 'rxjs/operators';

@Component({
    templateUrl: './tenants.component.html',
    animations: [appModuleAnimation()]
})
export class TenantsComponent extends PagedListingComponentBase<TenantDto> {

    @ViewChild('createTenantModal') createTenantModal: CreateTenantComponent;
    @ViewChild('editTenantModal') editTenantModal: EditTenantComponent;

    tenants: TenantDto[] = [];

    constructor(
        injector: Injector,
        private _tenantService: TenantServiceProxy
    ) {
        super(injector);
    }

    list(request:PagedRequestDto, pageNumber:number, finishedCallback: Function): void {
        this._tenantService.getAll(request.skipCount, request.maxResultCount)
            .pipe(finalize(() => { finishedCallback() }))
            .subscribe((result:PagedResultDtoOfTenantDto)=>{
				this.tenants = result.items;
				this.showPaging(result, pageNumber);
            });
    }

    delete(tenant: TenantDto): void {
		abp.message.confirm(
			"Delete tenant '"+ tenant.name +"'?",
			(result:boolean) => {
				if(result) {
                    this._tenantService.delete(tenant.id)
                        .pipe(finalize(() => {
                            abp.notify.info("Deleted tenant: " + tenant.name);
                            this.refresh();
                        }))
						.subscribe(() => { });
				}
			}
		);
    }

    // Show modals
    createTenant(): void {
        this.createTenantModal.show();
    }

    editTenant(tenant:TenantDto): void{
        this.editTenantModal.show(tenant.id);
    }
}
