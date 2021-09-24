import { Injectable, Injector } from '@angular/core';
import { PlatformLocation, registerLocaleData } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import * as moment from 'moment-timezone';
import { filter as _filter, merge as _merge } from 'lodash-es';
import { AppConsts } from '@shared/AppConsts';
import { AppSessionService } from '@shared/session/app-session.service';
import { environment } from './environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AppInitializer {
  constructor(
    private _injector: Injector,
    private _platformLocation: PlatformLocation,
    private _httpClient: HttpClient
  ) { }

  init(): () => Promise<boolean> {
    return () => {
      abp.ui.setBusy();
      return new Promise<boolean>((resolve, reject) => {
        AppConsts.appBaseHref = this.getBaseHref();
        const appBaseUrl = this.getDocumentOrigin() + AppConsts.appBaseHref;
        this.getApplicationConfig(appBaseUrl, () => {
          this.getUserConfiguration(() => {
            abp.event.trigger('abp.dynamicScriptsInitialized');
            // do not use constructor injection for AppSessionService
            const appSessionService = this._injector.get(AppSessionService);
            appSessionService.init().then(
              (result) => {
                abp.ui.clearBusy();
                if (this.shouldLoadLocale()) {
                  const angularLocale = this.convertAbpLocaleToAngularLocale(
                    abp.localization.currentLanguage.name
                  );
                  import(`@angular/common/locales/${angularLocale}.js`).then(
                    (module) => {
                      registerLocaleData(module.default);
                      resolve(result);
                    },
                    reject
                  );
                } else {
                  resolve(result);
                }
              },
              (err) => {
                abp.ui.clearBusy();
                reject(err);
              }
            );
          });
        });
      });
    };
  }

  private getBaseHref(): string {
    const baseUrl = this._platformLocation.getBaseHrefFromDOM();
    if (baseUrl) {
      return baseUrl;
    }

    return '/';
  }

  private getDocumentOrigin(): string {
    if (!document.location.origin) {
      const port = document.location.port ? ':' + document.location.port : '';
      return (
        document.location.protocol + '//' + document.location.hostname + port
      );
    }

    return document.location.origin;
  }

  private shouldLoadLocale(): boolean {
    return (
      abp.localization.currentLanguage.name &&
      abp.localization.currentLanguage.name !== 'en-US'
    );
  }

  private convertAbpLocaleToAngularLocale(locale: string): string {
    if (!AppConsts.localeMappings) {
      return locale;
    }

    const localeMapings = _filter(AppConsts.localeMappings, { from: locale });
    if (localeMapings && localeMapings.length) {
      return localeMapings[0]['to'];
    }

    return locale;
  }

  private getCurrentClockProvider(
    currentProviderName: string
  ): abp.timing.IClockProvider {
    if (currentProviderName === 'unspecifiedClockProvider') {
      return abp.timing.unspecifiedClockProvider;
    }

    if (currentProviderName === 'utcClockProvider') {
      return abp.timing.utcClockProvider;
    }

    return abp.timing.localClockProvider;
  }

  private getUserConfiguration(callback: () => void): void {
    const cookieLangValue = abp.utils.getCookieValue(
      'Abp.Localization.CultureName'
    );
    const token = abp.auth.getToken();

    const requestHeaders = {
      'Abp.TenantId': `${abp.multiTenancy.getTenantIdCookie()}`,
      '.AspNetCore.Culture': `c=${cookieLangValue}|uic=${cookieLangValue}`,
    };

    if (token) {
      requestHeaders['Authorization'] = `Bearer ${token}`;
    }

    this._httpClient
      .get<any>(
        `${AppConsts.remoteServiceBaseUrl}/AbpUserConfiguration/GetAll`,
        {
          headers: requestHeaders,
        }
      )
      .subscribe((response) => {
        const result = response.result;

        _merge(abp, result);

        abp.clock.provider = this.getCurrentClockProvider(
          result.clock.provider
        );

        moment.locale(abp.localization.currentLanguage.name);

        if (abp.clock.provider.supportsMultipleTimezone) {
          moment.tz.setDefault(abp.timing.timeZoneInfo.iana.timeZoneId);
        }

        callback();
      });
  }

  private getApplicationConfig(appRootUrl: string, callback: () => void) {
    this._httpClient
      .get<any>(`${appRootUrl}assets/${environment.appConfig}`, {
        headers: {
          'Abp.TenantId': `${abp.multiTenancy.getTenantIdCookie()}`,
        },
      })
      .subscribe((response) => {
        AppConsts.appBaseUrl = response.appBaseUrl;
        AppConsts.remoteServiceBaseUrl = response.remoteServiceBaseUrl;
        AppConsts.localeMappings = response.localeMappings;

        callback();
      });
  }
}
