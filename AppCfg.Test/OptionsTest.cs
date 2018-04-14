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
    public class OptionsTest
    {
        ITestOptionItem settings = MyAppCfg.Get<ITestOptionItem>();

        [Test]
        public void CollectOptionValues()
        {
            var attrs = TypeDescriptor.GetProperties(typeof(ITestOptionItem))["TestOptions"];

            var hasAttr = false;
            foreach (Attribute attrib in attrs.Attributes)
            {
                if (attrib is OptionAttribute opt)
                {
                    hasAttr = true;
                    Assert.True(opt.Alias == "test-options");
                    Assert.True(opt.DefaultValue.ToString() == "2");
                    Assert.True(opt.InputFormat == "3");
                    Assert.True(opt.Separator == "4");
                    Assert.True(opt.RawValue == "5");

                    break;
                }                
            }

            Assert.IsTrue(hasAttr);

            Assert.AreEqual(settings.TestOptions, "5");
        }
    }
}
