import { Injectable } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { AppSessionService } from '../session/app-session.service';

@Injectable()
export class AppUrlService {

    static tenancyNamePlaceHolder = '{TENANCY_NAME}';

    constructor(
        private readonly _appSessionService: AppSessionService
    ) {

    }

    get appRootUrl(): string {
        if (this._appSessionService.tenant) {
            return this.getAppRootUrlOfTenant(this._appSessionService.tenant.tenancyName);
        } else {
            return this.getAppRootUrlOfTenant(null);
        }
    }

    /**
     * Returning url ends with '/'.
     */
    getAppRootUrlOfTenant(tenancyName?: string): string {
        let baseUrl = this.ensureEndsWith(AppConsts.appBaseUrl, '/');

        if (baseUrl.indexOf(AppUrlService.tenancyNamePlaceHolder) < 0) {
            return baseUrl;
        }

        if (baseUrl.indexOf(AppUrlService.tenancyNamePlaceHolder + '.') >= 0) {
            baseUrl = baseUrl.replace(AppUrlService.tenancyNamePlaceHolder + '.', AppUrlService.tenancyNamePlaceHolder);
            if (tenancyName) {
                tenancyName = tenancyName + '.';
            }
        }

        if (!tenancyName) {
            return baseUrl.replace(AppUrlService.tenancyNamePlaceHolder, '');
        }

        return baseUrl.replace(AppUrlService.tenancyNamePlaceHolder, tenancyName);
    }

    private ensureEndsWith(str: string, c: string) {
        if (str.charAt(str.length - 1) !== c) {
            str = str + c;
        }

        return str;
    }

    private removeFromEnd(str: string, c: string) {
        if (str.charAt(str.length - 1) === c) {
            str = str.substr(0, str.length - 1);
        }

        return str;
    }
}
