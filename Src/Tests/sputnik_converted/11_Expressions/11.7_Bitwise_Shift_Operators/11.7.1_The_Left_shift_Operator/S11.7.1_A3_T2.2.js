// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S11.7.1_A3_T2.2;
 * @section: 11.7.1;
 * @assertion: Operator x << y returns ToNumber(x) << ToNumber(y); 
 * @description: Type(x) is different from Type(y) and both types vary between Number (primitive or object) and String (primitive and object);
*/


// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S11.7.1_A3_T2.2",

path: "TestCases/11_Expressions/11.7_Bitwise_Shift_Operators/11.7.1_The_Left_shift_Operator/S11.7.1_A3_T2.2.js",

assertion: "Operator x << y returns ToNumber(x) << ToNumber(y)",

description: "Type(x) is different from Type(y) and both types vary between Number (primitive or object) and String (primitive and object)",

test: function testcase() {
   //CHECK#1
if ("1" << 1 !== 2) {
  $ERROR('#1: "1" << 1 === 2. Actual: ' + ("1" << 1));
}

//CHECK#2
if (1 << "1" !== 2) {
  $ERROR('#2: 1 << "1" === 2. Actual: ' + (1 << "1"));
}

//CHECK#3
if (new String("1") << 1 !== 2) {
  $ERROR('#3: new String("1") << 1 === 2. Actual: ' + (new String("1") << 1));
}

//CHECK#4
if (1 << new String("1") !== 2) {
  $ERROR('#4: 1 << new String("1") === 2. Actual: ' + (1 << new String("1")));
}

//CHECK#5
if ("1" << new Number(1) !== 2) {
  $ERROR('#5: "1" << new Number(1) === 2. Actual: ' + ("1" << new Number(1)));
}

//CHECK#6
if (new Number(1) << "1" !== 2) {
  $ERROR('#6: new Number(1) << "1" === 2. Actual: ' + (new Number(1) << "1"));
}

//CHECK#7
if (new String("1") << new Number(1) !== 2) {
  $ERROR('#7: new String("1") << new Number(1) === 2. Actual: ' + (new String("1") << new Number(1)));
}

//CHECK#8
if (new Number(1) << new String("1") !== 2) {
  $ERROR('#8: new Number(1) << new String("1") === 2. Actual: ' + (new Number(1) << new String("1")));
}

//CHECK#9
if ("x" << 1 !== 0) {
  $ERROR('#9: "x" << 1 === 0. Actual: ' + ("x" << 1));
}

//CHECK#10
if (1 << "x" !== 1) {
  $ERROR('#10: 1 << "x" === 1. Actual: ' + (1 << "x"));
}

 }
});
