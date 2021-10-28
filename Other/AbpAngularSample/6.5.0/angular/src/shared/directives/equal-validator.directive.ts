import { Directive, forwardRef, Attribute } from '@angular/core';
import { Validator, AbstractControl, NG_VALIDATORS } from '@angular/forms';

@Directive({
  selector:
    // tslint:disable-next-line:directive-selector
    '[validateEqual][formControlName],[validateEqual][formControl],[validateEqual][ngModel]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => EqualValidator),
      multi: true
    }
  ]
})
export class EqualValidator implements Validator {
  constructor(
    @Attribute('validateEqual') public validateEqual: string,
    @Attribute('reverse') public reverse: string
  ) {}

  private get isReverse() {
    if (!this.reverse) {
      return false;
    }
    return this.reverse === 'true' ? true : false;
  }

  validate(control: AbstractControl): { [key: string]: any } {
    // self value
    const value = control.value;

    // second control
    const control2 = control.root.get(this.validateEqual);

    // value not equal
    if (control2 && value !== control2.value && !this.isReverse) {
      return {
        validateEqual: true
      };
    }

    // value equal and reverse
    if (control2 && value === control2.value && this.isReverse) {
      delete control2.errors['validateEqual'];
      if (!Object.keys(control2.errors).length) {
        control2.setErrors(null);
      }
    }

    // value not equal and reverse
    if (control2 && value !== control2.value && this.isReverse) {
      control2.setErrors({ validateEqual: true });
    }

    return null;
  }
}
