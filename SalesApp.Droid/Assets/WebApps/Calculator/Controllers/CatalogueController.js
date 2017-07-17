"use strict";
app.controller("CatalogueController", ["$scope", "$location", function ($scope, $location) {
    $scope.go = function (path) {
        $location.path(path);
    };
}
]);