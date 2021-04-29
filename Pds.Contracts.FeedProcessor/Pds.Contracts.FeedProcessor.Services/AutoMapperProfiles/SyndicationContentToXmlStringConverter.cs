using AutoMapper;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace Pds.Contracts.FeedProcessor.Services.AutoMapperProfiles
{
    /// <summary>
    /// A value converter to conver syndication content to xml string.
    /// </summary>
    /// <seealso cref="AutoMapper.IValueConverter{System.ServiceModel.Syndication.SyndicationContent, string}" />
    public class SyndicationContentToXmlStringConverter : IValueConverter<SyndicationContent, string>
    {
        /// <summary>
        /// Perform conversion from source member value to destination member value.
        /// </summary>
        /// <param name="sourceMember">Source member object.</param>
        /// <param name="context">Resolution context.</param>
        /// <returns>
        /// Destination member value.
        /// </returns>
        public string Convert(SyndicationContent sourceMember, ResolutionContext context)
        {
            var contentBuilder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(contentBuilder, settings);
            sourceMember.WriteTo(writer, "Content", string.Empty);
            writer.Flush();
            return contentBuilder.ToString();
        }
    }
}