using AppCfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCfg.Test
{
    /// <summary>
    /// Advanced tests for computed properties - complex scenarios including URL building,
    /// price calculations, feature flag aggregation, array processing, and conditional logic.
    /// </summary>
    [TestFixture]
    public class ComputedPropertyAdvancedTest
    {
        private const string TestStoreKey = "ComputedAdvanced:Store";

        [SetUp]
        public void Setup()
        {
            // Register a store with test data
            MyAppCfg.SettingStores.RegisterStore(TestStoreKey, metadata =>
            {
                var data = new Dictionary<string, string>
                {
                    // URL building test data
                    ["Protocol"] = "https",
                    ["Domain"] = "api.example.com",
                    ["Port"] = "443",
                    ["BasePath"] = "/v2",

                    // Calculation test data
                    ["BasePrice"] = "100.00",
                    ["TaxRate"] = "0.08",
                    ["Discount"] = "10",

                    // Feature flags
                    ["FeatureA"] = "true",
                    ["FeatureB"] = "false",
                    ["FeatureC"] = "true",

                    // Array data
                    ["Hosts"] = "host1;host2;host3",
                    ["Ports"] = "80;443;8080",

                    // Conditional data
                    ["Environment"] = "production",
                    ["DebugMode"] = "false"
                };

                return data.TryGetValue(metadata.SettingKey, out var value) ? value : null;
            });

            // Reset call counter
            ComputedAdvancedHelpers.CallCount = 0;
        }

        #region Complex URL Building Tests

        /// <summary>
        /// Verifies that computed property can combine multiple properties (Protocol, Domain, Port, BasePath)
        /// into a complete URL string.
        /// </summary>
        [Test, Description("Computed property should combine Protocol, Domain, Port, BasePath into full URL")]
        public void Computed_UrlBuilder_CombinesMultipleProperties()
        {
            // Act
            var settings = MyAppCfg.Get<IUrlSettings>();

            // Assert - https://api.example.com:443/v2
            Assert.AreEqual("https://api.example.com:443/v2", settings.FullUrl,
                "FullUrl should be 'https://api.example.com:443/v2'");
        }

        /// <summary>
        /// Verifies that computed properties can build on other computed properties
        /// to generate endpoint URLs (users, products, orders).
        /// </summary>
        [Test, Description("Computed endpoints should append path segments to the base URL")]
        public void Computed_UrlWithEndpoints_GeneratesCorrectPaths()
        {
            // Act
            var settings = MyAppCfg.Get<IUrlSettings>();

            // Assert
            Assert.AreEqual("https://api.example.com:443/v2/users", settings.UsersEndpoint,
                "UsersEndpoint should append '/users' to base URL");
            Assert.AreEqual("https://api.example.com:443/v2/products", settings.ProductsEndpoint,
                "ProductsEndpoint should append '/products' to base URL");
            Assert.AreEqual("https://api.example.com:443/v2/orders", settings.OrdersEndpoint,
                "OrdersEndpoint should append '/orders' to base URL");
        }

        #endregion

        #region Calculation Tests

        /// <summary>
        /// Verifies that computed properties can perform decimal calculations:
        /// PriceAfterDiscount = BasePrice - Discount = 100 - 10 = 90
        /// TaxAmount = PriceAfterDiscount * TaxRate = 90 * 0.08 = 7.20
        /// FinalPrice = PriceAfterDiscount + TaxAmount = 90 + 7.20 = 97.20
        /// </summary>
        [Test, Description("Computed properties should calculate PriceAfterDiscount=90, TaxAmount=7.20, FinalPrice=97.20")]
        public void Computed_PriceCalculation_ComputesCorrectTotal()
        {
            // Act
            var settings = MyAppCfg.Get<IPricingSettings>();

            // Assert
            Assert.AreEqual(90.00m, settings.PriceAfterDiscount,
                "PriceAfterDiscount should be BasePrice(100) - Discount(10) = 90");
            Assert.AreEqual(7.20m, settings.TaxAmount,
                "TaxAmount should be PriceAfterDiscount(90) * TaxRate(0.08) = 7.20");
            Assert.AreEqual(97.20m, settings.FinalPrice,
                "FinalPrice should be PriceAfterDiscount(90) * (1 + TaxRate(0.08)) = 97.20");
        }

        /// <summary>
        /// Verifies that chained computed properties work correctly where FinalPrice
        /// depends on intermediate calculations.
        /// </summary>
        [Test, Description("Chained computation: FinalPrice depends on PriceAfterDiscount and TaxRate")]
        public void Computed_PriceCalculation_ChainedComputations()
        {
            // Act
            var settings = MyAppCfg.Get<IPricingSettings>();

            // Assert
            Assert.AreEqual(97.20m, settings.FinalPrice,
                "FinalPrice should correctly chain through intermediate calculations");
        }

        #endregion

        #region Feature Flag Aggregation Tests

        /// <summary>
        /// Verifies that computed property can count enabled feature flags.
        /// FeatureA=true, FeatureB=false, FeatureC=true => Count = 2
        /// </summary>
        [Test, Description("EnabledFeatureCount should count true flags: A=true, B=false, C=true => 2")]
        public void Computed_FeatureFlags_CountsEnabled()
        {
            // Act
            var settings = MyAppCfg.Get<IFeatureFlagSettings>();

            // Assert
            Assert.AreEqual(2, settings.EnabledFeatureCount,
                "Should count 2 enabled features (FeatureA and FeatureC)");
        }

        /// <summary>
        /// Verifies that computed property can return a list of enabled feature names.
        /// Should include FeatureA and FeatureC, but not FeatureB.
        /// </summary>
        [Test, Description("EnabledFeatures list should contain 'FeatureA' and 'FeatureC' but not 'FeatureB'")]
        public void Computed_FeatureFlags_ListsEnabledFeatures()
        {
            // Act
            var settings = MyAppCfg.Get<IFeatureFlagSettings>();

            // Assert
            Assert.AreEqual(2, settings.EnabledFeatures.Count, "Should have 2 enabled features");
            Assert.Contains("FeatureA", settings.EnabledFeatures, "Should contain FeatureA");
            Assert.Contains("FeatureC", settings.EnabledFeatures, "Should contain FeatureC");
            Assert.IsFalse(settings.EnabledFeatures.Contains("FeatureB"), "Should not contain FeatureB");
        }

        /// <summary>
        /// Verifies that computed property can generate a status summary string.
        /// Expected: "FeatureA:ON, FeatureB:OFF, FeatureC:ON"
        /// </summary>
        [Test, Description("FeatureStatusSummary should be 'FeatureA:ON, FeatureB:OFF, FeatureC:ON'")]
        public void Computed_FeatureFlags_AllFeaturesStatus()
        {
            // Act
            var settings = MyAppCfg.Get<IFeatureFlagSettings>();

            // Assert
            Assert.AreEqual("FeatureA:ON, FeatureB:OFF, FeatureC:ON", settings.FeatureStatusSummary,
                "Status summary should show all features with ON/OFF status");
        }

        #endregion

        #region Array/List Processing Tests

        /// <summary>
        /// Verifies that computed property can combine two lists (Hosts and Ports)
        /// into a list of "host:port" pairs using Zip operation.
        /// </summary>
        [Test, Description("HostPortPairs should zip Hosts and Ports: 'host1:80', 'host2:443', 'host3:8080'")]
        public void Computed_ArrayProcessing_CombinesHostsAndPorts()
        {
            // Act
            var settings = MyAppCfg.Get<IArrayProcessingSettings>();

            // Assert
            Assert.AreEqual(3, settings.HostPortPairs.Count, "Should have 3 host:port pairs");
            Assert.Contains("host1:80", settings.HostPortPairs, "Should contain host1:80");
            Assert.Contains("host2:443", settings.HostPortPairs, "Should contain host2:443");
            Assert.Contains("host3:8080", settings.HostPortPairs, "Should contain host3:8080");
        }

        /// <summary>
        /// Verifies that computed properties can extract first and last elements from a list.
        /// PrimaryHost = first (host1), BackupHost = last (host3)
        /// </summary>
        [Test, Description("PrimaryHost should be 'host1' (first), BackupHost should be 'host3' (last)")]
        public void Computed_ArrayProcessing_FirstAndLast()
        {
            // Act
            var settings = MyAppCfg.Get<IArrayProcessingSettings>();

            // Assert
            Assert.AreEqual("host1", settings.PrimaryHost, "PrimaryHost should be first host");
            Assert.AreEqual("host3", settings.BackupHost, "BackupHost should be last host");
        }

        /// <summary>
        /// Verifies that computed property can join list elements with a different separator.
        /// Hosts = ["host1", "host2", "host3"] => "host1,host2,host3"
        /// </summary>
        [Test, Description("HostsCommaSeparated should join hosts with comma: 'host1,host2,host3'")]
        public void Computed_ArrayProcessing_JoinedString()
        {
            // Act
            var settings = MyAppCfg.Get<IArrayProcessingSettings>();

            // Assert
            Assert.AreEqual("host1,host2,host3", settings.HostsCommaSeparated,
                "Should join hosts with comma separator");
        }

        #endregion

        #region Conditional Logic Tests

        /// <summary>
        /// Verifies that computed properties can apply conditional logic based on Environment.
        /// For production: LogLevel=ERROR, CacheTimeout=3600, ShowDetailedErrors=false
        /// </summary>
        [Test, Description("Production environment: LogLevel=ERROR, CacheTimeout=3600, ShowDetailedErrors=false")]
        public void Computed_ConditionalLogic_ProductionSettings()
        {
            // Act
            var settings = MyAppCfg.Get<IConditionalSettings>();

            // Assert
            Assert.AreEqual("ERROR", settings.LogLevel,
                "Production environment should have LogLevel=ERROR");
            Assert.AreEqual(3600, settings.CacheTimeout,
                "Production environment should have CacheTimeout=3600 seconds");
            Assert.IsFalse(settings.ShowDetailedErrors,
                "Production environment should hide detailed errors");
        }

        /// <summary>
        /// Verifies that computed property can build a formatted label based on environment.
        /// For production with Domain "api.example.com": "[PROD] api.example.com"
        /// </summary>
        [Test, Description("EnvironmentLabel should be '[PROD] api.example.com' for production")]
        public void Computed_ConditionalLogic_EnvironmentLabel()
        {
            // Act
            var settings = MyAppCfg.Get<IConditionalSettings>();

            // Assert
            Assert.AreEqual("[PROD] api.example.com", settings.EnvironmentLabel,
                "Environment label should have [PROD] prefix for production");
        }

        #endregion

        #region Computed Method Call Count Tests

        /// <summary>
        /// Verifies that computed method is called only once during Get<T>(),
        /// not on each property access (value is cached).
        /// </summary>
        [Test, Description("Computed method should be called once during Get<T>(), cached for subsequent access")]
        public void Computed_MethodCalledOnce_DuringGet()
        {
            // Arrange
            ComputedAdvancedHelpers.CallCount = 0;

            // Act
            var settings = MyAppCfg.Get<ICallCountSettings>();
            var value1 = settings.ComputedValue;
            var value2 = settings.ComputedValue;
            var value3 = settings.ComputedValue;

            // Assert
            Assert.AreEqual(1, ComputedAdvancedHelpers.CallCount,
                "Computed method should be called exactly once, result should be cached");
        }

        #endregion

        #region Null Handling in Computed Tests

        /// <summary>
        /// Verifies that computed method can handle null input gracefully
        /// by returning a fallback value ("N/A").
        /// </summary>
        [Test, Description("Computed method should handle null/empty input by returning 'N/A'")]
        public void Computed_NullInput_HandledGracefully()
        {
            // Arrange
            MyAppCfg.SettingStores.RegisterStore("ComputedNull:Store", metadata => null);

            // Act
            var settings = MyAppCfg.Get<INullHandlingSettings>();

            // Assert
            Assert.AreEqual("N/A", settings.DisplayValue,
                "Computed should return 'N/A' when input is null or empty");
        }

        #endregion

        #region Test Interfaces

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IUrlSettings
        {
            [Option(Alias = "Protocol")]
            string Protocol { get; }

            [Option(Alias = "Domain")]
            string Domain { get; }

            [Option(Alias = "Port")]
            int Port { get; }

            [Option(Alias = "BasePath")]
            string BasePath { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.BuildFullUrl))]
            string FullUrl { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.BuildUsersEndpoint))]
            string UsersEndpoint { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.BuildProductsEndpoint))]
            string ProductsEndpoint { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.BuildOrdersEndpoint))]
            string OrdersEndpoint { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IPricingSettings
        {
            [Option(Alias = "BasePrice")]
            decimal BasePrice { get; }

            [Option(Alias = "TaxRate")]
            decimal TaxRate { get; }

            [Option(Alias = "Discount")]
            decimal Discount { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.CalculatePriceAfterDiscount))]
            decimal PriceAfterDiscount { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.CalculateTaxAmount))]
            decimal TaxAmount { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.CalculateFinalPrice))]
            decimal FinalPrice { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IFeatureFlagSettings
        {
            [Option(Alias = "FeatureA")]
            bool FeatureA { get; }

            [Option(Alias = "FeatureB")]
            bool FeatureB { get; }

            [Option(Alias = "FeatureC")]
            bool FeatureC { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.CountEnabledFeatures))]
            int EnabledFeatureCount { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.GetEnabledFeatures))]
            List<string> EnabledFeatures { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.GetFeatureStatusSummary))]
            string FeatureStatusSummary { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IArrayProcessingSettings
        {
            [Option(Alias = "Hosts", Separator = ";")]
            List<string> Hosts { get; }

            [Option(Alias = "Ports", Separator = ";")]
            List<int> Ports { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.CombineHostsAndPorts))]
            List<string> HostPortPairs { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.GetPrimaryHost))]
            string PrimaryHost { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.GetBackupHost))]
            string BackupHost { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.JoinHosts))]
            string HostsCommaSeparated { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface IConditionalSettings
        {
            [Option(Alias = "Environment")]
            string Environment { get; }

            [Option(Alias = "DebugMode")]
            bool DebugMode { get; }

            [Option(Alias = "Domain")]
            string Domain { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.DetermineLogLevel))]
            string LogLevel { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.DetermineCacheTimeout))]
            int CacheTimeout { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.ShouldShowDetailedErrors))]
            bool ShowDetailedErrors { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.BuildEnvironmentLabel))]
            string EnvironmentLabel { get; }
        }

        [DefaultOption(ProfileKey = TestStoreKey)]
        public interface ICallCountSettings
        {
            [Option(Alias = "BasePrice")]
            decimal BasePrice { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.TrackCallCount))]
            string ComputedValue { get; }
        }

        [DefaultOption(ProfileKey = "ComputedNull:Store")]
        public interface INullHandlingSettings
        {
            [Option(Alias = "MissingValue")]
            string MissingValue { get; }

            [Computed(typeof(ComputedAdvancedHelpers), nameof(ComputedAdvancedHelpers.HandleNullValue))]
            string DisplayValue { get; }
        }

        #endregion
    }

    /// <summary>
    /// Helper class containing computation methods for advanced computed property tests.
    /// </summary>
    public static class ComputedAdvancedHelpers
    {
        public static int CallCount { get; set; }

        // URL Building
        public static string BuildFullUrl(ComputedPropertyAdvancedTest.IUrlSettings s)
            => $"{s.Protocol}://{s.Domain}:{s.Port}{s.BasePath}";

        public static string BuildUsersEndpoint(ComputedPropertyAdvancedTest.IUrlSettings s)
            => $"{BuildFullUrl(s)}/users";

        public static string BuildProductsEndpoint(ComputedPropertyAdvancedTest.IUrlSettings s)
            => $"{BuildFullUrl(s)}/products";

        public static string BuildOrdersEndpoint(ComputedPropertyAdvancedTest.IUrlSettings s)
            => $"{BuildFullUrl(s)}/orders";

        // Pricing Calculations
        public static decimal CalculatePriceAfterDiscount(ComputedPropertyAdvancedTest.IPricingSettings s)
            => s.BasePrice - s.Discount;

        public static decimal CalculateTaxAmount(ComputedPropertyAdvancedTest.IPricingSettings s)
            => (s.BasePrice - s.Discount) * s.TaxRate;

        public static decimal CalculateFinalPrice(ComputedPropertyAdvancedTest.IPricingSettings s)
            => (s.BasePrice - s.Discount) * (1 + s.TaxRate);

        // Feature Flags
        public static int CountEnabledFeatures(ComputedPropertyAdvancedTest.IFeatureFlagSettings s)
            => (s.FeatureA ? 1 : 0) + (s.FeatureB ? 1 : 0) + (s.FeatureC ? 1 : 0);

        public static List<string> GetEnabledFeatures(ComputedPropertyAdvancedTest.IFeatureFlagSettings s)
        {
            var features = new List<string>();
            if (s.FeatureA) features.Add("FeatureA");
            if (s.FeatureB) features.Add("FeatureB");
            if (s.FeatureC) features.Add("FeatureC");
            return features;
        }

        public static string GetFeatureStatusSummary(ComputedPropertyAdvancedTest.IFeatureFlagSettings s)
            => $"FeatureA:{(s.FeatureA ? "ON" : "OFF")}, FeatureB:{(s.FeatureB ? "ON" : "OFF")}, FeatureC:{(s.FeatureC ? "ON" : "OFF")}";

        // Array Processing
        public static List<string> CombineHostsAndPorts(ComputedPropertyAdvancedTest.IArrayProcessingSettings s)
            => s.Hosts.Zip(s.Ports, (h, p) => $"{h}:{p}").ToList();

        public static string GetPrimaryHost(ComputedPropertyAdvancedTest.IArrayProcessingSettings s)
            => s.Hosts.FirstOrDefault();

        public static string GetBackupHost(ComputedPropertyAdvancedTest.IArrayProcessingSettings s)
            => s.Hosts.LastOrDefault();

        public static string JoinHosts(ComputedPropertyAdvancedTest.IArrayProcessingSettings s)
            => string.Join(",", s.Hosts);

        // Conditional Logic
        public static string DetermineLogLevel(ComputedPropertyAdvancedTest.IConditionalSettings s)
            => s.Environment == "production" ? "ERROR" : (s.DebugMode ? "DEBUG" : "INFO");

        public static int DetermineCacheTimeout(ComputedPropertyAdvancedTest.IConditionalSettings s)
            => s.Environment == "production" ? 3600 : 60;

        public static bool ShouldShowDetailedErrors(ComputedPropertyAdvancedTest.IConditionalSettings s)
            => s.Environment != "production" || s.DebugMode;

        public static string BuildEnvironmentLabel(ComputedPropertyAdvancedTest.IConditionalSettings s)
        {
            var prefix = s.Environment == "production" ? "[PROD]" : "[DEV]";
            return $"{prefix} {s.Domain}";
        }

        // Call Count Tracking
        public static string TrackCallCount(ComputedPropertyAdvancedTest.ICallCountSettings s)
        {
            CallCount++;
            return $"Called {CallCount} times";
        }

        // Null Handling
        public static string HandleNullValue(ComputedPropertyAdvancedTest.INullHandlingSettings s)
            => string.IsNullOrEmpty(s.MissingValue) ? "N/A" : s.MissingValue;
    }
}
