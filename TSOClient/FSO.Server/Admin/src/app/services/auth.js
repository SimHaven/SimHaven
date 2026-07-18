'use strict';

angular.module('admin').service('Auth', function ($window, Api, Token, $rootScope, $q, $location) {

    var self = this;

    this.loggedIn = false;

    function bootstrapUser(token, deferred, onComplete) {
        Token.setToken(token);
        Api.one('users', 'current').get().then(function (val) {
            $rootScope.currentUser = val;
            self.loggedIn = true;
            $rootScope.$broadcast('auth:restored');
            if (onComplete) onComplete();
            deferred.resolve();
        }, function (err) {
            self.logout();
            if (onComplete) onComplete();
            deferred.reject();
        });
    }

    var restorePromise = null;

    this.logout = function () {
        Token.clear();
        $rootScope.currentUser = null;
        self.loggedIn = false;
        restorePromise = null;
    };

    this.restore = function () {
        if (restorePromise != null) {
            return restorePromise;
        }

        var deferred = $q.defer();
        var currentRestore = deferred.promise;
        restorePromise = currentRestore;

        var finishRestore = function () {
            if (restorePromise === currentRestore) {
                restorePromise = null;
            }
        };

        var authToken = $window.sessionStorage.getItem('authToken');
        if (authToken != null) {
            try {
                authToken = JSON.parse(authToken);
                if (authToken.expires > new Date().getTime()) {
                    /** Still active **/
                    Api.setBaseUrl(authToken.api);
                    if (Token.getTokenImmediately() == null) {
                        bootstrapUser(authToken.access_token, deferred, finishRestore);
                    } else {
                        finishRestore();
                        deferred.resolve();
                    }
                    return currentRestore;
                }
            } catch (err) {
                // Invalid stored session data is handled like an expired session.
            }
        }

        self.logout();
        finishRestore();
        deferred.reject();
        return currentRestore;
    };

    this.login = function (apiUrl, username, password) {
        apiUrl += "/admin";
        apiUrl = apiUrl.split("//admin").join("/admin");

        Api.setBaseUrl(apiUrl);

        var date_encoded = "grant_type=password&username=" + escape(username) + "&password=" + escape(password);

        var deferred = $q.defer();

        Api.all("oauth/token").customPOST(
            date_encoded,
            undefined, // put your path here
            undefined, // params here, e.g. {format: "json"}
            { 'Content-Type': "application/x-www-form-urlencoded; charset=UTF-8" }
        ).then(function (result) {
            if (result.access_token) {
                
                $window.sessionStorage.setItem('authToken', JSON.stringify({
                    api: apiUrl,
                    access_token: result.access_token,
                    expires: new Date().getTime() + (result.expires_in * 1000)
                }));
                restorePromise = null;
                bootstrapUser(result.access_token, deferred);
            } else {
                deferred.reject();
            }
        }, function (err) {
            deferred.reject(err);
        });

        return deferred.promise;
    };

});
