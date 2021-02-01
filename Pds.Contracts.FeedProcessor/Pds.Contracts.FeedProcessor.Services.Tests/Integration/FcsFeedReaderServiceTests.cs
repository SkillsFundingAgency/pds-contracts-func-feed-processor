using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class FcsFeedReaderServiceTests
    {
        [TestMethod]
        public void GetContractEvents_ReturnsExpectedResult()
        {
            // Arrange
            var expected = new List<ContractEvent>
            {
                new ContractEvent
                {
                    ExampleFeedTime = DateTime.Now,
                    ExampleMessage = "Hello, world!",
                    ExampleSequenceId = 0
                },
                new ContractEvent
                {
                    ExampleFeedTime = DateTime.Now,
                    ExampleMessage = "Hello, world!",
                    ExampleSequenceId = 1
                },
                new ContractEvent
                {
                    ExampleFeedTime = DateTime.Now,
                    ExampleMessage = "Hello, world!",
                    ExampleSequenceId = 2
                },
                new ContractEvent
                {
                    ExampleFeedTime = DateTime.Now,
                    ExampleMessage = "Hello, world!",
                    ExampleSequenceId = 3
                },
                new ContractEvent
                {
                    ExampleFeedTime = DateTime.Now,
                    ExampleMessage = "Hello, world!",
                    ExampleSequenceId = 4
                },
            };

            var exampleService = new FcsFeedReaderService();

            // Act
            var actual = exampleService.GetContractEvents(expected[0].ExampleMessage);

            // Assert
            actual.Should().BeEquivalentTo(expected, opt => opt.Excluding(c => c.ExampleFeedTime));
        }
    }
}