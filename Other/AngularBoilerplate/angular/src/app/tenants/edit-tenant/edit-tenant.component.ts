import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { TenantServiceProxy, TenantDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'edit-tenant-modal',
  templateUrl: './edit-tenant.component.html'
})
export class EditTenantComponent extends AppComponentBase{

    @ViewChild('editTenantModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active: boolean = false;
    saving: boolean = false;
    tenant: TenantDto = null;

    constructor(
        injector: Injector,
        private _tenantService: TenantServiceProxy
    ) {
        super(injector);
    }

    show(id:number): void {
		this._tenantService.get(id)
		    .pipe(finalize(() => {
		        this.active = true;
		        this.modal.show();
		    }))
			.subscribe((result: TenantDto)=>{
				this.tenant = result;
			});
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    save(): void {
        this.saving = true;
        this._tenantService.update(this.tenant)
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
