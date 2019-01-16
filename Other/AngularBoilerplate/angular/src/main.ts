import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { enableProdMode, ChangeDetectorRef } from '@angular/core';
import { environment } from './environments/environment';
import { RootModule } from './root.module';
import { hmrBootstrap } from './hmr';

import * as moment from 'moment';

import 'moment/min/locales.min';
import 'moment-timezone';

if (environment.production) {
    enableProdMode();
}

const bootstrap = () => {
    return platformBrowserDynamic().bootstrapModule(RootModule);
};

/* "Hot Module Replacement" is enabled as described on
 * https://medium.com/@beeman/tutorial-enable-hrm-in-angular-cli-apps-1b0d13b80130#.sa87zkloh
 */

if (environment.hmr) {
    if (module['hot']) {
        hmrBootstrap(module, bootstrap); //HMR enabled bootstrap
    } else {
        console.error('HMR is not enabled for webpack-dev-server!');
        console.log('Are you using the --hmr flag for ng serve?');
    }
} else {
    bootstrap(); //Regular bootstrap
}