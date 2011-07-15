// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S15.8.2.9_A4;
 * @section: 15.8.2.9;
 * @assertion: If x is +Infinity, Math.floor(x) is +Infinity;
 * @description: Checking if Math.floor(x) is +Infinity, where x is +Infinity; 
 */
 

// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S15.8.2.9_A4",

path: "TestCases/15_Native/15.8_The_Math_Object/15.8.2_Function_Properties_of_the_Math_Object/15.8.2.9_floor/S15.8.2.9_A4.js",

assertion: "If x is +Infinity, Math.floor(x) is +Infinity",

description: "Checking if Math.floor(x) is +Infinity, where x is +Infinity",

test: function testcase() {
   // CHECK#1
var x = +Infinity;
if (Math.floor(x) !== +Infinity)
{
	$ERROR("#1: 'var x = +Infinity; Math.floor(x) !== +Infinity'");
}

 }
});
