// <auto-generated />
namespace IronJS.Tests.UnitTests.Sputnik.Conformance.NativeECMAScriptObjects.NumberObjects.PropertiesOfNumberConstructor
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class NumberMAXVALUETests : SputnikTestFixture
    {
        public NumberMAXVALUETests()
            : base(@"Conformance\15_Native_ECMA_Script_Objects\15.7_Number_Objects\15.7.3_Properties_of_Number_Constructor\15.7.3.2_Number.MAX_VALUE")
        {
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 15.7.3.2")]
        [TestCase("S15.7.3.2_A1.js", Description = "Number.MAX_VALUE is approximately 1.7976931348623157e308")]
        public void NumberMAX_VALUEIsApproximately17976931348623157e308(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 15.7.3.2")]
        [TestCase("S15.7.3.2_A2.js", Description = "Number.MAX_VALUE is ReadOnly")]
        public void NumberMAX_VALUEIsReadOnly(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 15.7.3.2")]
        [TestCase("S15.7.3.2_A3.js", Description = "Number.MAX_VALUE is DontDelete")]
        public void NumberMAX_VALUEIsDontDelete(string file)
        {
            RunFile(file);
        }

        [Test]
        [Category("Sputnik Conformance")]
        [Category("ECMA 15.7.3.2")]
        [TestCase("S15.7.3.2_A4.js", Description = "Number.MAX_VALUE has the attribute DontEnum")]
        public void NumberMAX_VALUEHasTheAttributeDontEnum(string file)
        {
            RunFile(file);
        }
    }
}