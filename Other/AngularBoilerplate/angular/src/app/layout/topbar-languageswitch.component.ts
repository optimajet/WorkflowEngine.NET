import { Component, OnInit, Injector, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { UserServiceProxy, ChangeUserLanguageDto } from '@shared/service-proxies/service-proxies';

import * as _ from 'lodash';

@Component({
  templateUrl: './topbar-languageswitch.component.html',
  selector: 'topbar-languageswitch',
  encapsulation: ViewEncapsulation.None
})
export class TopBarLanguageSwitchComponent extends AppComponentBase implements OnInit {

  languages: abp.localization.ILanguageInfo[];
  currentLanguage: abp.localization.ILanguageInfo;
  
  constructor(
      injector: Injector,
      private _userService: UserServiceProxy
  ) {
    super(injector);
  }

  ngOnInit() {
    this.languages = _.filter(this.localization.languages, l => !l.isDisabled);
    this.currentLanguage = this.localization.currentLanguage;
  }

  changeLanguage(languageName: string): void {
      const input = new ChangeUserLanguageDto();
      input.languageName = languageName;

      this._userService.changeLanguage(input).subscribe(() => {
          abp.utils.setCookieValue(
              'Abp.Localization.CultureName',
              languageName,
              new Date(new Date().getTime() + 5 * 365 * 86400000), //5 year
              abp.appPath
          );

          window.location.reload();
      });
  }
}
