// <auto-generated />
namespace IronJS.Tests.UnitTests.IE9.chapter07._7_6._7_6_1
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class _7_6_1_2_Tests : IE9TestFixture
    {
        public _7_6_1_2_Tests() : base(@"chapter07\7.6\7.6.1\7.6.1.2") { }

        [Test(Description = "Strict Mode - SyntaxError isn\'t thrown when \'IMPLEMENTS\' occurs in strict mode code")] public void _7_6_1_2__10__s() { RunFile(@"7.6.1.2-10-s.js"); }
        [Test(Description = "Strict Mode - SyntaxError isn\'t thrown when \'Implements\' occurs in strict mode code")] public void _7_6_1_2__11__s() { RunFile(@"7.6.1.2-11-s.js"); }
        [Test(Description = "Strict Mode - SyntaxError isn\'t thrown when \'implement\' occurs in strict mode code")] public void _7_6_1_2__12__s() { RunFile(@"7.6.1.2-12-s.js"); }
        [Test(Description = "Strict Mode - SyntaxError isn\'t thrown when \'implementss\' occurs in strict mode code")] public void _7_6_1_2__13__s() { RunFile(@"7.6.1.2-13-s.js"); }
        [Test(Description = "Strict Mode - SyntaxError isn\'t thrown when \'implements0\' occurs in strict mode code")] public void _7_6_1_2__14__s() { RunFile(@"7.6.1.2-14-s.js"); }
        [Test(Description = "Strict Mode - SyntaxError isn\'t thrown when \'_implements\' occurs in strict mode code")] public void _7_6_1_2__16__s() { RunFile(@"7.6.1.2-16-s.js"); }
        [Test(Description = "Strict Mode - SyntaxError is thrown when FutureReservedWord \'implements\' occurs in strict mode code")] public void _7_6_1_2__1__s() { RunFile(@"7.6.1.2-1-s.js"); }
        [Test(Description = "7.6 - SyntaxError expected: reserved words used as Identifier Names in UTF8: \\u0069mplements (implements)")] public void _7_6_1__17__s() { RunFile(@"7.6.1-17-s.js"); }
    }
}