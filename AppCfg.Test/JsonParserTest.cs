using NUnit.Framework;
using System;

namespace AppCfg.Test
{
    public interface IJsonItemSetting
    {
        [Option(Alias = "cute_animal")]
        JsonAnimalTestModel CuteAnimal { get; }

        [Option(Alias = "test_setting_with_js_config")]
        JsonMachineTestModel Optimus { get; }
    }

    [TestFixture]
    [Description("Tests for JSON type parser with custom serializer settings")]
    public class JsonParserTest
    {
        private IJsonItemSetting settings;

        [SetUp]
        public void Setup()
        {
            // Setup JSON serializer settings with custom date format
            MyAppCfg.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "dd+MM+yyyy"
            };

            settings = MyAppCfg.Get<IJsonItemSetting>();
        }

        [Test]
        [Description("Verifies that JSON parser correctly parses boolean property from JSON")]
        public void JsonParser_WithJsonInput_ParsesBooleanProperty()
        {
            Assert.AreEqual(true, settings.CuteAnimal.CanSwim, "CanSwim property should be true");
        }

        [Test]
        [Description("Verifies that JSON parser correctly parses integer property from JSON")]
        public void JsonParser_WithJsonInput_ParsesIntegerProperty()
        {
            Assert.AreEqual(2, settings.CuteAnimal.Legs, "Legs property should be 2");
        }

        [Test]
        [Description("Verifies that JSON parser correctly parses string property from JSON")]
        public void JsonParser_WithJsonInput_ParsesStringProperty()
        {
            Assert.AreEqual("Duck", settings.CuteAnimal.Name, "Name property should be 'Duck'");
        }

        [Test]
        [Description("Verifies that JSON parser correctly parses DateTime with custom format (dd+MM+yyyy)")]
        public void JsonParser_WithCustomDateFormat_ParsesDateTimeCorrectly()
        {
            var expected = new DateTime(2018, 11, 26);
            Assert.AreEqual(expected, settings.CuteAnimal.SampleDay, "SampleDay should be parsed as 2018-11-26 using format dd+MM+yyyy");
        }

        [Test]
        [Description("Verifies that JSON parser respects model-specific date format override")]
        public void JsonParser_WithModelDateFormat_ParsesDateTimeCorrectly()
        {
            var expected = new DateTime(2019, 12, 26);
            Assert.AreEqual(expected, settings.Optimus.DayWithNewFormat, "DayWithNewFormat should be parsed as 2019-12-26");
        }
    }
}
