import { Component, Injector, ViewChild } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { RoleServiceProxy, RoleDto, PagedResultDtoOfRoleDto } from 'shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CreateRoleComponent } from 'app/roles/create-role/create-role.component';
import { EditRoleComponent } from 'app/roles/edit-role/edit-role.component';
import { finalize } from 'rxjs/operators';

@Component({
  templateUrl: './roles.component.html',
  animations: [appModuleAnimation()]
})
export class RolesComponent extends PagedListingComponentBase<RoleDto> {

	@ViewChild('createRoleModal') createRoleModal: CreateRoleComponent;
	@ViewChild('editRoleModal') editRoleModal: EditRoleComponent;
	
	roles: RoleDto[] = [];

	constructor(
		private injector:Injector,
		private rolesService: RoleServiceProxy
	) {
		super(injector);
	}
    
	list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
        this.rolesService.getAll(request.skipCount, request.maxResultCount)
            .pipe(finalize(() => { finishedCallback() }))
            .subscribe((result: PagedResultDtoOfRoleDto)=>{
				this.roles = result.items;
				this.showPaging(result, pageNumber);
		});
	}

	delete(role: RoleDto): void {
		abp.message.confirm(
			"Remove Users from Role and delete Role '"+ role.displayName +"'?",
			"Permanently delete this Role",
			(result:boolean) =>{
				if(result)
				{
                    this.rolesService.delete(role.id)
                        .pipe(finalize(() => {
                            abp.notify.info("Deleted Role: " + role.displayName);
                            this.refresh();
                        }))
						.subscribe(() => { });
				}
			}
		);
	}

	// Show Modals
	createRole(): void {
		this.createRoleModal.show();
	}

	editRole(role:RoleDto): void {
		this.editRoleModal.show(role.id);
	}
}
