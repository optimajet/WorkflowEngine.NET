import {
  Component,
  Injector,
  OnInit,
  Output,
  EventEmitter
} from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { AppComponentBase } from '@shared/app-component-base';
import {
  TenantServiceProxy,
  TenantDto
} from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-tenant-dialog.component.html'
})
export class EditTenantDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  tenant: TenantDto = new TenantDto();
  id: number;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _tenantService: TenantServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._tenantService.get(this.id).subscribe((result: TenantDto) => {
      this.tenant = result;
    });
  }

  save(): void {
    this.saving = true;

    this._tenantService.update(this.tenant).subscribe(
      () => {
        this.notify.info(this.l('SavedSuccessfully'));
        this.bsModalRef.hide();
        this.onSave.emit();
      },
      () => {
        this.saving = false;
      }
    );
  }
}
