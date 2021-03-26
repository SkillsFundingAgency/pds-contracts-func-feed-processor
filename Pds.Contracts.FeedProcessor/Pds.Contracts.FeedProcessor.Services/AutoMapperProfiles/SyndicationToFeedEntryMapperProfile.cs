using AutoMapper;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.ServiceModel.Syndication;

namespace Pds.Contracts.FeedProcessor.Services.AutoMapperProfiles
{
    /// <summary>
    /// Automapper profile for mapping syndication item to feed entry.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class SyndicationToFeedEntryMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationToFeedEntryMapperProfile"/> class.
        /// </summary>
        public SyndicationToFeedEntryMapperProfile()
        {
            CreateMap<SyndicationItem, FeedEntry>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id.Replace("uuid:", string.Empty))))
                .ForMember(dst => dst.Updated, opt => opt.MapFrom(src => src.LastUpdatedTime.UtcDateTime))
                .ForMember(dst => dst.Content, opt => opt.ConvertUsing(new SyndicationContentToXmlStringConverter()));
        }
    }
}