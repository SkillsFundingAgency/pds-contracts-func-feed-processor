using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.FeedProcessor.Services.AutoMapperProfiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.FeedProcessor.Services.AutoMapperProfiles.Tests
{
    [TestClass, TestCategory("Unit")]
    public class SyndicationToFeedEntryMapperProfileTests
    {
        [TestMethod, TestCategory("Unit")]
        public void SyndicationToFeedEntryMapperProfileTest()
        {
            // Arrange
            var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<SyndicationToFeedEntryMapperProfile>());

            // Assert
            config.AssertConfigurationIsValid<SyndicationToFeedEntryMapperProfile>();
        }
    }
}