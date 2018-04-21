using NUnit.Framework;
using System.Collections.Generic;

namespace AppCfg.Test
{
    public interface ICustomParserSettings
    {
        JsonPersonTestModel DemoRawBuilder { get; }
    }

    [TestFixture]
    public class CustomParserTest
    {
        [SetUp]
        public void Setup()
        {
            MyAppCfg.TypeParsers.Register(new ParserWithRawBuilder<JsonPersonTestModel>());

            settings = MyAppCfg.Get<ICustomParserSettings>();
        }

        ICustomParserSettings settings = null;

        [Test]
        public void Test_CustomParser()
        {
            Assert.AreEqual(settings.DemoRawBuilder.Title, "Person");
            Assert.AreEqual(settings.DemoRawBuilder.Properties.Age.Description, "Age in years");
            Assert.AreEqual(settings.DemoRawBuilder.Properties.Age.Type, "a-integer-type");
            Assert.AreEqual(settings.DemoRawBuilder.Properties.Age.Minimum, 18);
            Assert.AreEqual(settings.DemoRawBuilder.Required, new List<string> { "name", "age" });
        }
    }
}
