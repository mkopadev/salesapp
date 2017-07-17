"use strict";
app.factory('MathService', function() {
    var factory = {};

    factory.multiply = function(a, b) {
        return a * b;
    }

    return factory;
}); 