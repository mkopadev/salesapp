"use strict";
app.controller("CostingController", ["$scope", "$location","CostCalculatorService",'$cacheFactory', function ($scope, $location, CostCalculatorService, $cacheFactory) {
    $scope.title = "Cost per week";
    $scope.currentCost;
    var costKey = "cost";
    var CalculatorKey = 'CalcKey';
    //
    $scope.onlyNumbers = /^[1-9][0-9]*$/;
    $scope.keys = [];
    $scope.cache;
    if(angular.isUndefined($cacheFactory.get(CalculatorKey))){
        $scope.cache = $cacheFactory(CalculatorKey);
    }else{
        $scope.cache = $cacheFactory.get(CalculatorKey);
    }

    $scope.cost = $scope.cache.get(costKey);

    $scope.currentCostPerYear = function (cost) {
        $scope.currentCost = CostCalculatorService.CalculateCurrentCost(cost);

        // Do caching to populate form when you click back button
        if (angular.isUndefined($scope.cache.get(costKey))) {
            $scope.keys.push(costKey);
        }
        $scope.cache.put(costKey, angular.isUndefined(cost) ? null : cost);

        this.go('/cost-year');
    };

    $scope.go = function (path) {
        $location.path(path);
    };
}
]);