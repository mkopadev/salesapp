"use strict";
app.factory('productService', ['$http', function ($http) {
    var hash = CSharp.GetHash();
    var userId = CSharp.GetUserId();
    var baseUrl = CSharp.GetUrl();
    var url = baseUrl + 'calculator/getproducts?userId='+userId+'&lang=EN&loc=';
    delete $http.defaults.headers.common['User-Agent'];

    return $http({
        method: 'GET',
        url: url,
        timeout: 5,
        headers: {
                'Content-Type' : undefined,
                'Authorization' : 'Basic ' + hash,
                'X-Requested-With' : undefined,
                'User-Agent': undefined,
                'Accept-Encoding': undefined,
                'Accept': undefined,
                'Accept-Language': undefined
              }
        })
        .success(function (data) {
            return data;
        })
        .error(function (err) {
            return err;
        });
}]);
