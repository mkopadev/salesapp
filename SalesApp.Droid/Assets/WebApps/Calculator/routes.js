"use strict";
app.config(["$stateProvider", "$urlRouterProvider", "$locationProvider", function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise('cost');

    $stateProvider.state('cost', {
        url: "/cost",
        templateUrl: "Templates/costing.html",
        controller: "CostingController"
    }).state('/cost-year', {
        url: "/cost-year",
        templateUrl: "Templates/cost-year.html",
        controller: "CostPerYearController"
    }).state('/income', {
        url: "/income",
        templateUrl: "Templates/cost-income.html",
        controller: "IncomeController"
    }).state('/saving', {
        url: "/saving",
        templateUrl: "Templates/saving.html",
        controller: "SavingsController"
    }).state('/saving-per-year', {
        url: "/saving-per-year",
        templateUrl: "Templates/yearly-saving.html",
        controller: "YearlySavingController"
    }).state('/catalogue', {
        url: "/catalogue",
        templateUrl: "Templates/catalogue.html",
        controller: "CatalogueController"
    });
}]);

app.run(["$window", "$state", "$location", "$rootScope", "$timeout", function($window, $state, $location, $rootScope, $timeout) {

    /**
     * This function grabs the state name and navigates to the state
     * @param state
     */
    var changeState = function (state) {
        $timeout(function () {
            $state.go(state);
        });
    };

    $rootScope.today = new Date();
    $rootScope.config = {
        projectVersion: "1.0",
        dateFormat: "dd.MM.yyyy"
    };

}]);

