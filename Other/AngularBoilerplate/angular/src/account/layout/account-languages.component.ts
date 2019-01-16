import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';

import * as _ from 'lodash';

@Component({
    selector: 'account-languages',
    templateUrl: './account-languages.component.html',
    styleUrls: [
        './account-languages.component.less'
    ]
})
export class AccountLanguagesComponent extends AppComponentBase implements OnInit {

    languages: abp.localization.ILanguageInfo[];
    currentLanguage: abp.localization.ILanguageInfo;

    constructor(
        injector: Injector
    ) {
        super(injector);
    }

    ngOnInit() {
        this.languages = _.filter(this.localization.languages, l => !l.isDisabled);
        this.currentLanguage = this.localization.currentLanguage;
    }

    changeLanguage(languageName: string): void {
        abp.utils.setCookieValue(
            "Abp.Localization.CultureName",
            languageName,
            new Date(new Date().getTime() + 5 * 365 * 86400000), //5 year
            abp.appPath
        );

        location.reload();
    }
}