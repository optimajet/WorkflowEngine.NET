import { Component, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import {
  ChangePasswordDto,
  UserServiceProxy
} from '@shared/service-proxies/service-proxies';
import { AbpValidationError } from '@shared/components/validation/abp-validation.api';

@Component({
  templateUrl: './change-password.component.html',
  animations: [appModuleAnimation()]
})
export class ChangePasswordComponent extends AppComponentBase {
  saving = false;
  changePasswordDto = new ChangePasswordDto();
  newPasswordValidationErrors: Partial<AbpValidationError>[] = [
    {
      name: 'pattern',
      localizationKey:
        'PasswordsMustBeAtLeast8CharactersContainLowercaseUppercaseNumber',
    },
  ];
  confirmNewPasswordValidationErrors: Partial<AbpValidationError>[] = [
    {
      name: 'validateEqual',
      localizationKey: 'PasswordsDoNotMatch',
    },
  ];

  constructor(
    injector: Injector,
    private userServiceProxy: UserServiceProxy,
    private router: Router
  ) {
    super(injector);
  }

  changePassword() {
    this.saving = true;

    this.userServiceProxy
      .changePassword(this.changePasswordDto)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe((success) => {
        if (success) {
          abp.message.success('Password changed successfully', 'Success');
          this.router.navigate(['/']);
        }
      });
  }
}
