"use strict";
app.controller("YearlySavingController", ["$scope", "$location", "productService","CostCalculatorService",  function ($scope, $location, productService, CostCalculatorService) {

     $scope.currentCostPerYear = CostCalculatorService.currentTotalCostInAYear;
     $scope.costPerYear = parseInt(CostCalculatorService.Income.Year) > 1 ? 0 : CostCalculatorService.deviceCost;
     $scope.deviceIncome = CostCalculatorService.Income.Income;
     $scope.year = yearWord(CostCalculatorService.Income.Year);
     var saving = CostCalculatorService.Income.Amount;
     $scope.yearlySaving = saving < 0 ? 0 : saving;

     $scope.SavingDetails = function (index) {
        CostCalculatorService.YearlyIncome(saving[index]);
        $location.path('/saving-per-year');
    };

     $scope.go = function (path) {
         $location.path(path);
     };


    function yearWord(year){
    	var y = parseInt(year);
    	console.log('Year '+y);
    	if(y === 1){
    		return "One";
    	}
    	else if(y === 2){
    		return "Two";
    	}
    	else if(y === 3){
    		return "Three";
    	}else if (y === 4) {
	        return "Four";
	    } else {
	        return "Five";
    	}
    }
}
]);