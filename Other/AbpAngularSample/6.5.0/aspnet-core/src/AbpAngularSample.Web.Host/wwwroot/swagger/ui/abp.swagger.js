var abp = abp || {};
(function () {

    /* Swagger */

    abp.swagger = abp.swagger || {};

    abp.swagger.addAuthToken = function () {
        var authToken = abp.auth.getToken();
        if (!authToken) {
            return false;
        }

        var cookieAuth = new SwaggerClient.ApiKeyAuthorization(abp.auth.tokenHeaderName, 'Bearer ' + authToken, 'header');
        swaggerUi.api.clientAuthorizations.add('bearerAuth', cookieAuth);
        return true;
    }

    abp.swagger.addCsrfToken = function () {
        var csrfToken = abp.security.antiForgery.getToken();
        if (!csrfToken) {
            return false;
        }
        var csrfCookieAuth = new SwaggerClient.ApiKeyAuthorization(abp.security.antiForgery.tokenHeaderName, csrfToken, 'header');
        swaggerUi.api.clientAuthorizations.add(abp.security.antiForgery.tokenHeaderName, csrfCookieAuth);
        return true;
    }

    function addAntiForgeryTokenToXhr(xhr) {
        var antiForgeryToken = abp.security.antiForgery.getToken();
        if (antiForgeryToken) {
            xhr.setRequestHeader(abp.security.antiForgery.tokenHeaderName, antiForgeryToken);
        }
    }

    function loginUserInternal(tenantId, callback) {
        var usernameOrEmailAddress = document.getElementById('userName').value;
        if (!usernameOrEmailAddress) {
            alert('Username or Email Address is required, please try with a valid value !');
            return false;
        }

        var password = document.getElementById('password').value;
        if (!password) {
            alert('Password is required, please try with a valid value !');
            return false;
        }

        var xhr = new XMLHttpRequest();

        xhr.onreadystatechange = function () {
            if (xhr.readyState === XMLHttpRequest.DONE) {
                if (xhr.status === 200) {
                    var responseJSON = JSON.parse(xhr.responseText);
                    var result = responseJSON.result;
                    var expireDate = new Date(Date.now() + (result.expireInSeconds * 1000));
                    abp.auth.setToken(result.accessToken, expireDate);
                    callback();   
                } else {
                    alert('Login failed !');
                }
            }
        };

        xhr.open('POST', '/api/TokenAuth/Authenticate', true);
        xhr.setRequestHeader('Abp.TenantId', tenantId);
        xhr.setRequestHeader('Content-type', 'application/json');
        addAntiForgeryTokenToXhr(xhr);
        xhr.send(
            JSON.stringify(
                { usernameOrEmailAddress: usernameOrEmailAddress, password: password }
            )
        );
    };

    abp.swagger.login = function (callback) {
        //Get TenantId first
        var tenancyName = document.getElementById('tenancyName').value;

        if (tenancyName) {
            var xhrTenancyName = new XMLHttpRequest();
            xhrTenancyName.onreadystatechange = function () {
                if (xhrTenancyName.readyState === XMLHttpRequest.DONE && xhrTenancyName.status === 200) {
                    var responseJSON = JSON.parse(xhrTenancyName.responseText);
                    var result = responseJSON.result;
                    if (result.state === 1) { // Tenant exists and active.
                        loginUserInternal(result.tenantId, callback); // Login for tenant    
                    } else {
                        alert('There is no such tenant or tenant is not active !');
                    }
                }
            };

            xhrTenancyName.open('POST', '/api/services/app/Account/IsTenantAvailable', true);
            xhrTenancyName.setRequestHeader('Content-type', 'application/json');
            addAntiForgeryTokenToXhr(xhrTenancyName);
            xhrTenancyName.send(
                JSON.stringify({ tenancyName: tenancyName })
            );
        } else {
            loginUserInternal(null, callback); // Login for host
        }
    };

    abp.swagger.logout = function () {
        abp.auth.clearToken();
    }

    abp.swagger.closeAuthDialog = function () {
        if (document.getElementById('abp-auth-dialog')) {
            document.getElementsByClassName("swagger-ui")[1].removeChild(document.getElementById('abp-auth-dialog'));
        }
    }

    abp.swagger.openAuthDialog = function (loginCallback) {
        abp.swagger.closeAuthDialog();

        var abpAuthDialog = document.createElement('div');
        abpAuthDialog.className = 'dialog-ux';
        abpAuthDialog.id = 'abp-auth-dialog';

        document.getElementsByClassName("swagger-ui")[1].appendChild(abpAuthDialog);

        // -- backdrop-ux
        var backdropUx = document.createElement('div');
        backdropUx.className = 'backdrop-ux';
        abpAuthDialog.appendChild(backdropUx);

        // -- modal-ux
        var modalUx = document.createElement('div');
        modalUx.className = 'modal-ux';
        abpAuthDialog.appendChild(modalUx);

        // -- -- modal-dialog-ux
        var modalDialogUx = document.createElement('div');
        modalDialogUx.className = 'modal-dialog-ux';
        modalUx.appendChild(modalDialogUx);

        // -- -- -- modal-ux-inner
        var modalUxInner = document.createElement('div');
        modalUxInner.className = 'modal-ux-inner';
        modalDialogUx.appendChild(modalUxInner);

        // -- -- -- -- modal-ux-header
        var modalUxHeader = document.createElement('div');
        modalUxHeader.className = 'modal-ux-header';
        modalUxInner.appendChild(modalUxHeader);

        var modalHeader = document.createElement('h3');
        modalHeader.innerText = 'Authorize';
        modalUxHeader.appendChild(modalHeader);

        // -- -- -- -- modal-ux-content
        var modalUxContent = document.createElement('div');
        modalUxContent.className = 'modal-ux-content';
        modalUxInner.appendChild(modalUxContent);

        modalUxContent.onkeydown = function (e) {
            if (e.keyCode === 13) {
                //try to login when user presses enter on authorize modal
                abp.swagger.login(loginCallback);
            }
        };

        //Inputs
        createInput(modalUxContent, 'tenancyName', 'Tenancy Name (Leave empty for Host)');
        createInput(modalUxContent, 'userName', 'Username or email address');
        createInput(modalUxContent, 'password', 'Password', 'password');

        //Buttons
        var authBtnWrapper = document.createElement('div');
        authBtnWrapper.className = 'auth-btn-wrapper';
        modalUxContent.appendChild(authBtnWrapper);

        //Close button
        var closeButton = document.createElement('button');
        closeButton.className = 'btn modal-btn auth btn-done button';
        closeButton.innerText = 'Close';
        closeButton.style.marginRight = '5px';
        closeButton.onclick = abp.swagger.closeAuthDialog;
        authBtnWrapper.appendChild(closeButton);

        //Authorize button
        var authorizeButton = document.createElement('button');
        authorizeButton.className = 'btn modal-btn auth authorize button';
        authorizeButton.innerText = 'Login';
        authorizeButton.onclick = function() {
            abp.swagger.login(loginCallback);
        };
        authBtnWrapper.appendChild(authorizeButton);
    }

    function createInput(container, id, title, type) {
        var wrapper = document.createElement('div');
        wrapper.className = 'wrapper';
        container.appendChild(wrapper);

        var label = document.createElement('label');
        label.innerText = title;
        wrapper.appendChild(label);

        var section = document.createElement('section');
        section.className = 'block-tablet col-10-tablet block-desktop col-10-desktop';
        wrapper.appendChild(section);

        var input = document.createElement('input');
        input.id = id;
        input.type = type ? type : 'text';
        input.style.width = '100%';

        section.appendChild(input);
    }

})();