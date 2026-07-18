'use strict';

angular.module('admin').factory('Api', function (Restangular, Token, $rootScope) {
    var handlingUnauthorized = false;

    $rootScope.$on('auth:restored', function () {
        handlingUnauthorized = false;
    });

    return Restangular.withConfig(function (RestangularConfigurer) {

        RestangularConfigurer.addFullRequestInterceptor(function (element, operation, what, url, headers, params) {
            var token = Token.getTokenImmediately();
            if (token != null) {
                headers['Authorization'] = "bearer " + token;
            }

            return {
                headers: headers,
                params: params,
                element: element
            };
        });

        RestangularConfigurer.addResponseInterceptor(function (data, operation, what, url, response, deferred) {
            if (data.error) {
                deferred.reject(data.error);
                return;
            }

            if (operation === "getList") {
                data.total = response.headers('X-Total-Count');
                if (!data.total) {
                    data.total = data.length;
                }
            }

            return data;
        });

        RestangularConfigurer.setErrorInterceptor(function (response) {
            if (response.status === 401 && !handlingUnauthorized) {
                handlingUnauthorized = true;
                Token.clear();
                $rootScope.$broadcast('auth:expired');
            }

            return true;
        });


    });
});
