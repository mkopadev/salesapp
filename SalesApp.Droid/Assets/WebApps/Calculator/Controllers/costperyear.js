"use strict";
app.controller("CostPerYearController", ["$scope", "$location",'$cacheFactory','productService','CostCalculatorService',  function ($scope, $location, $cacheFactory, productService, CostCalculatorService) {

    $scope.currentCostPerYear = CostCalculatorService.currentTotalCostInAYear;
    var offlineProducts = [{ 'ProductTypeId': '16732cb0-b418-e411-9439-000c29921969', 'Name': 'Product 3', 'Deposit': 2999.00, 'CostPerDay': 40.00, 'TotalPrice': 17599.00, 'OptimalLoanDuration': 365 }, { 'ProductTypeId': 'eaf05020-675c-e511-80c0-000d3a219a86', 'Name': 'Product 4', 'Deposit': 3500.00, 'CostPerDay': 50.00, 'TotalPrice': 21750.00, 'OptimalLoanDuration': 365 }, { 'ProductTypeId': 'c87f42e3-8ca8-e511-80c3-000d3a219a86', 'Name': 'Product 400', 'Deposit': 7999.00, 'CostPerDay': 125.00, 'TotalPrice': 53624.00, 'OptimalLoanDuration': 365 }, { 'ProductTypeId': '91e97523-8dba-e511-80c8-000d3a22f4e7', 'Name': 'Product 4', 'Deposit': 3500.00, 'CostPerDay': 50.00, 'TotalPrice': 40000.00, 'OptimalLoanDuration': 730 }];
    $scope.productKeys = [];
    var calculatorKey = 'CalcKey';
    $scope.productIndex = -1;
    var indexKey = 'indexKey';
    $scope.cache = null;
    $scope.onlyNumbers = /^[1-9][0-9]*$/;

    if (angular.isUndefined($cacheFactory.get(calculatorKey)))
    {
        $scope.cache = $cacheFactory(calculatorKey);
    }
    else
    {
        $scope.cache = $cacheFactory.get(calculatorKey);
    }

    $scope.productIndex = $scope.cache.get(indexKey) == null ? -1 : $scope.cache.get(indexKey);

    productService.success(function(data) {
        $scope.products = data.Products;
    }).error(function(data){
        $scope.products = offlineProducts;
    });

    $scope.checkProduct = function(index){
        $scope.productIndex = index;
        // cache selected product index
        if (angular.isUndefined($scope.cache.get(indexKey))) {
            $scope.productKeys.push(indexKey);
        }
        $scope.cache.put(indexKey, angular.isUndefined(index) ? null : index);
    }

    $scope.calculatePrice = function(product){
        CostCalculatorService.CalculateDeviceCost(product);
        this.go('/income');
    }

    $scope.go = function (path) {
        $location.path(path);
    };
}
]);