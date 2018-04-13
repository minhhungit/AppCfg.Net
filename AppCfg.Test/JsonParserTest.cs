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
    public class JsonParserTest
    {
        [SetUp]
        public void Setup()
        {
            // setup json serializer settings
            MyAppCfg.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "dd+MM+yyyy" // ex: setup default format
            };

            settings = MyAppCfg.Get<IJsonItemSetting>();
        }

        IJsonItemSetting settings = null;

        [Test]
        public void Test_JsonParser()
        {
            Assert.AreEqual(settings.CuteAnimal.CanSwim, true);
            Assert.AreEqual(settings.CuteAnimal.Legs, 2);
            Assert.AreEqual(settings.CuteAnimal.Name, "Duck");
            Assert.AreEqual(settings.CuteAnimal.SampleDay, new DateTime(2018, 11, 26));

            Assert.AreEqual(settings.Optimus.DayWithNewFormat, new DateTime(2019, 12, 26));
        }
    }
}
