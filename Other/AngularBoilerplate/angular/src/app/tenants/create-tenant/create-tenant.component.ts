import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { TenantServiceProxy, CreateTenantDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'create-tenant-modal',
  templateUrl: './create-tenant.component.html'
})
export class CreateTenantComponent extends AppComponentBase {

    @ViewChild('createTenantModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;
    tenant: CreateTenantDto = null;

    constructor(
        injector: Injector,
        private _tenantService: TenantServiceProxy
    ) {
        super(injector);
    }

    show(): void {
        this.active = true;
        this.modal.show();
        this.tenant = new CreateTenantDto();
        this.tenant.init({isActive:true});
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    save(): void {
        this.saving = true;
        this._tenantService.create(this.tenant)
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
