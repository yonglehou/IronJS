﻿var x = 1;
var y = 2;
var z = "foo";
var foo = function(a1, a2) {
    var bar = 2;

    bar = a1;
    bar = arguments

    return function() {
        a1 = "lol";

        return bar;
    };
};