using Pds.Contracts.FeedProcessor.Services.Extensions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Pds.Contracts.FeedProcessor.Services.Extensions.Tests
{
    [TestClass, TestCategory("Unit")]
    public class XmlExtensionTests
    {
        private static IEnumerable<object[]> GetValue_ExpectedTestScenarios
        {
            get
            {
                yield return new object[] { "test", false, "//c:nodevalue", "test" };
                yield return new object[] { "1", false, "//c:nodevalue", 1 };
                yield return new object[] { "1", false, "//c:nodevalue", 1M };
                yield return new object[] { "2020-12-01", false, "//c:nodevalue", (DateTime?)DateTime.Parse("2020-12-01") };
                yield return new object[] { string.Empty, true, "//c:nonExistentNodevalue", 0 };
                yield return new object[] { string.Empty, true, "//c:nonExistentNodevalue", decimal.Zero };
                yield return new object[] { "default(string)", true, "//c:nonExistentNodevalue", default(string) };
                yield return new object[] { "default(DateTime?)", true, "//c:nonExistentNodevalue", default(DateTime?) };
            }
        }

        private static IEnumerable<object[]> GetValue_ExceptionTestScenarios
        {
            get
            {
                yield return new object[] { string.Empty, false, "//c:nonExistentNodevalue", 0 };
                yield return new object[] { string.Empty, false, "//c:nonExistentNodevalue", decimal.Zero };
                yield return new object[] { "default(string)", false, "//c:nonExistentNodevalue", default(string) };
                yield return new object[] { "default(DateTime?)", false, "//c:nonExistentNodevalue", default(DateTime?) };
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(GetValue_ExpectedTestScenarios))]
        public void GetValueTest_ReturnsExpectedResult(string innerText, bool isOptional, string xpath, object expectedValue)
        {
            switch (expectedValue)
            {
                case string exp:
                    Action act = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, exp);
                    act.Should().NotThrow();
                    break;
                case int exp:
                    Action act1 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, exp);
                    act1.Should().NotThrow();
                    break;
                case decimal exp:
                    Action act2 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, exp);
                    act2.Should().NotThrow();
                    break;
                case DateTime exp:
                    Action act3 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, (DateTime?)exp);
                    act3.Should().NotThrow();
                    break;
                case null when innerText.Equals("default(string)"):
                    Action act4 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, default(string));
                    act4.Should().NotThrow();
                    break;
                case null when innerText.Equals("default(DateTime?)"):
                    Action act5 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, default(DateTime?));
                    act5.Should().NotThrow();
                    break;

                default:
                    throw new NotImplementedException($"Test not implemented for type {expectedValue?.GetType().Name}");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(GetValue_ExceptionTestScenarios))]
        public void GetValueTest_ThrowsException(string innerText, bool isOptional, string xpath, object expectedValue)
        {
            switch (expectedValue)
            {
                case string exp:
                    Action act = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, exp);
                    act.Should().Throw<InvalidOperationException>();
                    break;
                case int exp:
                    Action act1 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, exp);
                    act1.Should().Throw<InvalidOperationException>();
                    break;
                case decimal exp:
                    Action act2 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, exp);
                    act2.Should().Throw<InvalidOperationException>();
                    break;
                case null when innerText.Equals("default(string)"):
                    Action act3 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, default(string));
                    act3.Should().Throw<InvalidOperationException>();
                    break;
                case null when innerText.Equals("default(DateTime?)"):
                    Action act4 = () => Internal_GetValueTest_ReturnsExpectedResult(innerText, isOptional, xpath, default(DateTime?));
                    act4.Should().Throw<InvalidOperationException>();
                    break;
                default:
                    throw new NotImplementedException($"Test not implemented for type {expectedValue.GetType().Name}");
            }
        }

        [DataTestMethod]
        [DataRow("//c:nestedNodeValue")]
        [DataRow("//c:nestednodevalue")]
        [DataRow("//c:NESTEDNODEVALUE")]
        [DataRow("//c:NestedNodeValue")]
        public void SelectNodesIgnoreCaseTest(string xpath)
        {
            // Arrange
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("c", "urn:sfa:schemas:contract");

            var document = new XmlDocument();
            document.LoadXml(GetXmlWithInnerText("test"));
            XmlElement element = document.DocumentElement;

            var expected = document.SelectNodes("//c:nestednodevalue", ns);

            // Act
            var result = element.SelectNodesIgnoreCase(xpath, ns);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        private void Internal_GetValueTest_ReturnsExpectedResult<T>(string xmlElement, bool isOptional, string xpath, T expectedValue)
        {
            // Arrange
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("c", "urn:sfa:schemas:contract");

            var document = new XmlDocument();
            document.LoadXml(GetXmlWithInnerText(xmlElement));
            XmlElement element = document.DocumentElement;

            // Act
            var result = element.GetValue<T>(xpath, ns, isOptional);

            // Assert
            result.Should().BeEquivalentTo(expectedValue);
        }

        private string GetXmlWithInnerText(string innerText)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"<contract schemaVersion=""11.03"" xmlns=""urn:sfa:schemas:contract"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">");
            builder.AppendLine($"<nodevalue>{innerText}</nodevalue>");
            builder.AppendLine($"<nestednode>");
            builder.AppendLine($"<nestednodevalue>{innerText}</nestednodevalue>");
            builder.AppendLine($"<nestednodevalue>{innerText}</nestednodevalue>");
            builder.AppendLine($"<nestednodevalue>{innerText}</nestednodevalue>");
            builder.AppendLine($"</nestednode>");
            builder.AppendLine("</contract>");

            return builder.ToString();
        }
    }
}