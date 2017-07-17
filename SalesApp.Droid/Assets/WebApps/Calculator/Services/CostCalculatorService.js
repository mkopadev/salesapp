"use strict";

app.service('CostCalculatorService', function() {
    var self = this;
    self.currentTotalCostInAYear =0;
    self.deviceCost =0;
    self.totalSavings =[];
    self.Income ={};

    self.CalculateCurrentCost = function(cost) {
        var weeksInAYear = 52;

        var keroseneCostPerYear = cost.kerosene == null ? 0 : cost.kerosene * weeksInAYear;

        var batteriesCostPerYear = cost.batteries == null ? 0 : cost.batteries * weeksInAYear;

        var chargingCostPerYear = cost.chargingPhone == null ? 0 :  cost.chargingPhone * weeksInAYear;

        self.currentTotalCostInAYear = keroseneCostPerYear + batteriesCostPerYear + chargingCostPerYear;
        return self.currentTotalCostInAYear;
    };


    self.CalculateDeviceCost =  function(product){
        var deposit =  product.Deposit;
        var dailyCost = product.CostPerDay;
        var numberOfInstallments = 365;
        //var numberOfInstallments = product.NumberOfInstallments;
        var name = product.Name;

        self.deviceCost = product.TotalPrice; //  deposit + (dailyCost * numberOfInstallments);
        return self.deviceCost;
    };


    self.CalculateSavings =  function(income){
        var savingObj =[];
        var months = 12;
        var prevAmount = 0;
        for(var i=1; i<=5; i++){
            var json = [];
            var amount = (angular.isUndefined(income)) ? 0 : (income == null ? 0 : (income.charging == null ? 0 : income.charging));
            var incomeAmount = parseInt(amount) * parseInt(months);

            if(i === 1){
                var amountSaved = ((parseInt(self.currentTotalCostInAYear) - parseInt(self.deviceCost)) + parseInt(incomeAmount));
                json["Year"] = i;
                prevAmount = amountSaved < 0 ? 0 : amountSaved;
                json["Amount"] = prevAmount;
                json["Income"] = incomeAmount;
                savingObj.push(json);
                continue;
            }

            prevAmount = prevAmount + (parseInt(self.currentTotalCostInAYear) + parseInt(incomeAmount));
            json["Year"] =i;
            json["Amount"] = prevAmount;
            json["Income"] = incomeAmount;
            savingObj.push(json);
        }

        self.totalSavings = savingObj;
        return self.totalSavings;
    };

    self.YearlyIncome = function (income) {
        return self.Income = income;
    }
});