using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCfg.Test
{
    public interface ICustomParserSettings
    {
        [Option(Separator = "^")]
        List<int> Numbers { get; }
        JsonPersonTestModel DemoRawBuilder { get; }
    }

    [TestFixture]
    public class CustomParserTest
    {
        [SetUp]
        public void Setup()
        {
            MyAppCfg.TypeParserFactory.AddParser(new ListIntParser());
            MyAppCfg.TypeParserFactory.AddParser(new ParserWithRawBuilder<JsonPersonTestModel>());

            settings = MyAppCfg.Get<ICustomParserSettings>();
        }

        ICustomParserSettings settings = null;

        [Test]
        public void Test_CustomParser()
        {
            Assert.AreEqual(settings.Numbers, new List<int> { 1, 99, 123456789 });

            Assert.AreEqual(settings.DemoRawBuilder.Title, "Person");
            Assert.AreEqual(settings.DemoRawBuilder.Properties.Age.Description, "Age in years");
            Assert.AreEqual(settings.DemoRawBuilder.Properties.Age.Type, "a-integer-type");
            Assert.AreEqual(settings.DemoRawBuilder.Properties.Age.Minimum, 18);
            Assert.AreEqual(settings.DemoRawBuilder.Required, new List<string> { "name", "age" });
        }
    }
}
