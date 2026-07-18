'use strict';

angular.module('admin').service('Token', function ($window) {

    var token = null;

    this.setToken = function (value) {
        $window.sessionStorage.setItem('lastAuth', JSON.stringify({ token: value }));
        token = value;
    };

    this.clear = function () {
        token = null;
        $window.sessionStorage.removeItem('lastAuth');
        $window.sessionStorage.removeItem('authToken');
    };

    var tokenPromise = null;
    this.getTokenImmediately = function () {
        return token;
    };

});
