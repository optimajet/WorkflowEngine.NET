import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { RoleServiceProxy, GetRoleForEditOutput, RoleDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'edit-role-modal',
    templateUrl: './edit-role.component.html'
})
export class EditRoleComponent extends AppComponentBase {
    @ViewChild('editRoleModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    active: boolean = false;
    saving: boolean = false;

    model: GetRoleForEditOutput = null;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    constructor(
        injector: Injector,
        private _roleService: RoleServiceProxy
    ) {
        super(injector);
    }

    show(id: number): void {
        this._roleService.getRoleForEdit(id)
            .pipe(finalize(() => {
                this.active = true;
                this.modal.show();
            }))
            .subscribe((result: GetRoleForEditOutput) => {
                this.model = result;
            });
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    checkPermission(permissionName: string): string {
        if (this.model.grantedPermissionNames.indexOf(permissionName) != -1) {
            return "checked";
        }
        else {
            return "";
        }
    }

    save(): void {
        const role = this.model.role;

        var permissions = [];
        $(this.modalContent.nativeElement).find("[name=permission]").each(
            function (index: number, elem: Element) {
                if ($(elem).is(":checked") == true) {
                    permissions.push(elem.getAttribute("value").valueOf());
                }
            }
        )

        this.saving = true;
        var input = new RoleDto();

        input.name = role.name;
        input.displayName = role.displayName;
        input.description = role.description;
        input.id = role.id;
        input.isStatic = role.isStatic;
        input.permissions = permissions;


        this._roleService.update(input)
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
