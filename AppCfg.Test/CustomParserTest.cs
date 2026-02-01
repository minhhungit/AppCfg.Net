using NUnit.Framework;
using System.Collections.Generic;

namespace AppCfg.Test
{
    public interface ICustomParserSettings
    {
        JsonPersonTestModel DemoRawBuilder { get; }
    }

    [TestFixture]
    [Description("Tests for custom type parser functionality with RawBuilder pattern")]
    public class CustomParserTest
    {
        private ICustomParserSettings settings;

        [SetUp]
        public void Setup()
        {
            MyAppCfg.TypeParsers.Register(new ParserWithRawBuilder<JsonPersonTestModel>());
            settings = MyAppCfg.Get<ICustomParserSettings>();
        }

        [Test]
        [Description("Verifies that custom parser correctly parses JSON into complex object model")]
        public void CustomParser_WithJsonInput_ParsesRootProperties()
        {
            Assert.AreEqual("Person", settings.DemoRawBuilder.Title, "Title property should be 'Person'");
        }

        [Test]
        [Description("Verifies that custom parser correctly parses nested object properties")]
        public void CustomParser_WithJsonInput_ParsesNestedAgeDescription()
        {
            Assert.AreEqual("Age in years", settings.DemoRawBuilder.Properties.Age.Description, "Age.Description should be 'Age in years'");
        }

        [Test]
        [Description("Verifies that custom parser correctly parses nested object type property")]
        public void CustomParser_WithJsonInput_ParsesNestedAgeType()
        {
            Assert.AreEqual("a-integer-type", settings.DemoRawBuilder.Properties.Age.Type, "Age.Type should be 'a-integer-type'");
        }

        [Test]
        [Description("Verifies that custom parser correctly parses nested object numeric property")]
        public void CustomParser_WithJsonInput_ParsesNestedAgeMinimum()
        {
            Assert.AreEqual(18, settings.DemoRawBuilder.Properties.Age.Minimum, "Age.Minimum should be 18");
        }

        [Test]
        [Description("Verifies that custom parser correctly parses array property")]
        public void CustomParser_WithJsonInput_ParsesRequiredArray()
        {
            var expected = new List<string> { "name", "age" };
            Assert.AreEqual(expected, settings.DemoRawBuilder.Required, "Required should contain 'name' and 'age'");
        }
    }
}
