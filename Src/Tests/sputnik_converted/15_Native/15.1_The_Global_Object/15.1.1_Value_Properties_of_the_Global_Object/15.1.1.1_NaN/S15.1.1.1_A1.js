// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S15.1.1.1_A1;
 * @section: 15.1.1.1, 11.4.3, 15.1.2.4, 15.1.2.5;
 * @assertion: The initial value of NaN is NaN;
 * @description: Use typeof, isNaN, isFinite;
*/


// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S15.1.1.1_A1",

path: "TestCases/15_Native/15.1_The_Global_Object/15.1.1_Value_Properties_of_the_Global_Object/15.1.1.1_NaN/S15.1.1.1_A1.js",

assertion: "The initial value of NaN is NaN",

description: "Use typeof, isNaN, isFinite",

test: function testcase() {
   // CHECK#1
if (typeof(NaN) !== "number") {
	$ERROR('#1: typeof(NaN) === "number". Actual: ' + (typeof(NaN))); 
}

// CHECK#2
if (isNaN(NaN) !== true) {
	$ERROR('#2: NaN === Not-a-Number. Actual: ' + (NaN)); 
}

// CHECK#3
if (isFinite(NaN) !== false) {
	$ERROR('#3: NaN === Not-a-Finite. Actual: ' + (NaN)); 
}


 }
});
