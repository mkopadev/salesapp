"use strict";
app.controller("SavingsController", ["$scope", "$location", "productService","CostCalculatorService",  function ($scope, $location, productService, CostCalculatorService) {

    $scope.savings = CostCalculatorService.totalSavings;
    $scope.savingsDetails = function (index) {
        CostCalculatorService.YearlyIncome(this.savings[index]);
        this.go('/saving-per-year');
    };

    $scope.go = function (path) {
        $location.path(path);
    };
}
]);