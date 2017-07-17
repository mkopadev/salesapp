"use strict";
app.controller("IncomeController", ["$scope", "$location", "productService","CostCalculatorService",'$cacheFactory',  function ($scope, $location, productService, CostCalculatorService, $cacheFactory) {

    $scope.currentCostPerYear = CostCalculatorService.currentTotalCostInAYear;
    $scope.costPerYear = CostCalculatorService.deviceCost;
    $scope.income = {};
    $scope.IncomeKeys = [];

    var CalculatorKey = 'CalcKey';
    var incomeKey = 'incomeKey';
    $scope.cache;

    if(angular.isUndefined($cacheFactory.get(CalculatorKey))){
        $scope.cache = $cacheFactory(CalculatorKey);
    }else{
        $scope.cache = $cacheFactory.get(CalculatorKey);
    }

    $scope.income = $scope.cache.get(incomeKey);

    $scope.calcIncome = function (incomeDetails) {
        CostCalculatorService.CalculateSavings(incomeDetails);
        if (angular.isUndefined($scope.cache.get(incomeKey))) {
            $scope.IncomeKeys.push(incomeKey);
        }
        $scope.cache.put(incomeKey, angular.isUndefined(incomeDetails) ? null : incomeDetails);
        this.go('/saving');
    };

    $scope.go = function (path) {
        $location.path(path);
    };
}
]);