using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Func.Tests.Integration
{
    [TestClass]
    public class ExampleTimerFunctionTests
    {
        [TestMethod, TestCategory("Integration")]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var exampleService = new ExampleService();

            var function = new ExampleTimerFunction(exampleService);

            // Act
            Func<Task> act = async () => { await function.Run(null, null); };

            // Assert
            act.Should().NotThrow();
        }
    }
}