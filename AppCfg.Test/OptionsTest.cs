using NUnit.Framework;
using System;
using System.ComponentModel;

namespace AppCfg.Test
{
    public interface ITestOptionItem
    {
        [Option(Alias = "test-options", DefaultValue = "2", InputFormat = "3", Separator = "4", RawValue = "5")]
        string TestOptions { get; }
    }

    [TestFixture]
    [NUnit.Framework.Description("Tests for OptionAttribute properties and behavior")]
    public class OptionsTest
    {
        private ITestOptionItem settings;

        [SetUp]
        public void Setup()
        {
            settings = MyAppCfg.Get<ITestOptionItem>();
        }

        [Test]
        [NUnit.Framework.Description("Verifies that OptionAttribute.Alias property is correctly set")]
        public void OptionAttribute_Alias_IsSetCorrectly()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];
            OptionAttribute optionAttr = null;

            foreach (Attribute attr in propertyDescriptor.Attributes)
            {
                if (attr is OptionAttribute opt)
                {
                    optionAttr = opt;
                    break;
                }
            }

            Assert.IsNotNull(optionAttr, "OptionAttribute should be present on TestOptions property");
            Assert.AreEqual("test-options", optionAttr.Alias, "Alias should be 'test-options'");
        }

        [Test]
        [NUnit.Framework.Description("Verifies that OptionAttribute.DefaultValue property is correctly set")]
        public void OptionAttribute_DefaultValue_IsSetCorrectly()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];
            OptionAttribute optionAttr = null;

            foreach (Attribute attr in propertyDescriptor.Attributes)
            {
                if (attr is OptionAttribute opt)
                {
                    optionAttr = opt;
                    break;
                }
            }

            Assert.IsNotNull(optionAttr, "OptionAttribute should be present on TestOptions property");
            Assert.AreEqual("2", optionAttr.DefaultValue.ToString(), "DefaultValue should be '2'");
        }

        [Test]
        [NUnit.Framework.Description("Verifies that OptionAttribute.InputFormat property is correctly set")]
        public void OptionAttribute_InputFormat_IsSetCorrectly()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];
            OptionAttribute optionAttr = null;

            foreach (Attribute attr in propertyDescriptor.Attributes)
            {
                if (attr is OptionAttribute opt)
                {
                    optionAttr = opt;
                    break;
                }
            }

            Assert.IsNotNull(optionAttr, "OptionAttribute should be present on TestOptions property");
            Assert.AreEqual("3", optionAttr.InputFormat, "InputFormat should be '3'");
        }

        [Test]
        [NUnit.Framework.Description("Verifies that OptionAttribute.Separator property is correctly set")]
        public void OptionAttribute_Separator_IsSetCorrectly()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];
            OptionAttribute optionAttr = null;

            foreach (Attribute attr in propertyDescriptor.Attributes)
            {
                if (attr is OptionAttribute opt)
                {
                    optionAttr = opt;
                    break;
                }
            }

            Assert.IsNotNull(optionAttr, "OptionAttribute should be present on TestOptions property");
            Assert.AreEqual("4", optionAttr.Separator, "Separator should be '4'");
        }

        [Test]
        [NUnit.Framework.Description("Verifies that OptionAttribute.RawValue property is correctly set")]
        public void OptionAttribute_RawValue_IsSetCorrectly()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];
            OptionAttribute optionAttr = null;

            foreach (Attribute attr in propertyDescriptor.Attributes)
            {
                if (attr is OptionAttribute opt)
                {
                    optionAttr = opt;
                    break;
                }
            }

            Assert.IsNotNull(optionAttr, "OptionAttribute should be present on TestOptions property");
            Assert.AreEqual("5", optionAttr.RawValue, "RawValue should be '5'");
        }

        [Test]
        [NUnit.Framework.Description("Verifies that RawValue takes precedence and is returned as the setting value")]
        public void OptionAttribute_RawValue_TakesPrecedenceInValue()
        {
            Assert.AreEqual("5", settings.TestOptions, "Setting value should be '5' (RawValue takes precedence)");
        }

        [Test]
        [NUnit.Framework.Description("Verifies that OptionAttribute is present on the property")]
        public void OptionAttribute_IsPresent_OnProperty()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];
            bool hasOptionAttribute = false;

            foreach (Attribute attr in propertyDescriptor.Attributes)
            {
                if (attr is OptionAttribute)
                {
                    hasOptionAttribute = true;
                    break;
                }
            }

            Assert.IsTrue(hasOptionAttribute, "OptionAttribute should be present on TestOptions property");
        }
    }
}
