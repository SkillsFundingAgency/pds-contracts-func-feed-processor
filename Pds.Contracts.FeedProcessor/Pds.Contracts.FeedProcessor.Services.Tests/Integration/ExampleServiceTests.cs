using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Integration
{
    [TestClass]
    public class ExampleServiceTests
    {
        [TestMethod, TestCategory("Integration")]
        public async Task Hello_ReturnsExpectedResult()
        {
            // Arrange
            var expected = "Hello, world!";

            var exampleService = new ExampleService();

            // Act
            var actual = await exampleService.Hello();

            // Assert
            actual.Should().Be(expected);
        }
    }
}