// <auto-generated />
namespace IronJS.Tests.UnitTests.Sputnik.Conformance.Statement.IterationStatements
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class TheForInStatementTests : SputnikTestFixture
    {
        public TheForInStatementTests()
            : base(@"Conformance\12_Statement\12.6_Iteration_Statements\12.6.4_The_for_in_Statement")
        {
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A1.js", Description = "\"for(key in undefined)\" Statement is allowed")]
        public void ForKeyInUndefinedStatementIsAllowed(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A13_T1.js", Description = "FunctionDeclaration within a \"for-in\" Statement is not allowed", ExpectedException = typeof(Exception))]
        [TestCase("S12.6.4_A13_T2.js", Description = "FunctionDeclaration within a \"for-in\" Statement is not allowed", ExpectedException = typeof(Exception))]
        [TestCase("S12.6.4_A13_T3.js", Description = "FunctionDeclaration within a \"for-in\" Statement is not allowed", ExpectedException = typeof(Exception))]
        public void FunctionDeclarationWithinAForInStatementIsNotAllowed(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A14_T1.js", Description = "FunctionExpession within a \"for-in\" Expression is allowed")]
        [TestCase("S12.6.4_A14_T2.js", Description = "FunctionExpession within a \"for-in\" Expression is allowed")]
        public void FunctionExpessionWithinAForInExpressionIsAllowed(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A15.js", Description = "Block within a \"for-in\" Expression is not allowed", ExpectedException = typeof(Exception))]
        public void BlockWithinAForInExpressionIsNotAllowed(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A2.js", Description = "\"for(key in null)\" Expression is allowed")]
        public void ForKeyInNullExpressionIsAllowed(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A3.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A3.1.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A4.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A4.1.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A5.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A5.1.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A6.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        [TestCase("S12.6.4_A6.1.js", Description = "The production IterationStatement: \"for (var VariableDeclarationNoIn in Expression) Statement\"")]
        public void TheProductionIterationStatementForVarVariableDeclarationNoInInExpressionStatement(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 12.6.4")]
        [TestCase("S12.6.4_A7_T1.js", Description = "Properties of the object being enumerated may be deleted during enumeration")]
        [TestCase("S12.6.4_A7_T2.js", Description = "Properties of the object being enumerated may be deleted during enumeration")]
        public void PropertiesOfTheObjectBeingEnumeratedMayBeDeletedDuringEnumeration(string file)
        {
            RunFile(file);
        }
    }
}