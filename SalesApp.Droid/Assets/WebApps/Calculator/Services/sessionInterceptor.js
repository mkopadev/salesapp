app.factory('sessionInjector', [function () {
    var sessionInjector = {
        request: function (config) {
                config.headers['x-session-token'] = 'martin-luggaliki';
            return config;
        }
    };
    return sessionInjector;
}]);