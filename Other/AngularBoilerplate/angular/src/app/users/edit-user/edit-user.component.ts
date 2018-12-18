import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { UserServiceProxy, UserDto, RoleDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'edit-user-modal',
    templateUrl: './edit-user.component.html'
})
export class EditUserComponent extends AppComponentBase {

    @ViewChild('editUserModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;

    user: UserDto = null;
    roles: RoleDto[] = null;

    constructor(
        injector: Injector,
        private _userService: UserServiceProxy
    ) {
        super(injector);
    }

    userInRole(role: RoleDto, user: UserDto): string {
        if (user.roleNames.indexOf(role.normalizedName) !== -1) {
            return "checked";
        }
        else {
            return "";
        }
    }

    show(id: number): void {
        this._userService.getRoles()
            .subscribe((result) => {
                this.roles = result.items;
            });

        this._userService.get(id)
            .subscribe(
            (result) => {
                this.user = result;
                this.active = true;
                this.modal.show();
            }
            );
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    save(): void {
        var roles = [];
        $(this.modalContent.nativeElement).find("[name=role]").each(function (ind: number, elem: Element) {
            if ($(elem).is(":checked")) {
                roles.push(elem.getAttribute("value").valueOf());
            }
        });

        this.user.roleNames = roles;

        this.saving = true;
        this._userService.update(this.user)
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
