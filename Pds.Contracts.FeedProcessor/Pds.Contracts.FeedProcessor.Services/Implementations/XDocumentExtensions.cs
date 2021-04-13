using System;
using System.Xml.Linq;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Extensions for XDocument.
    /// </summary>
    public static class XDocumentExtensions
    {
        /// <summary>
        /// Converts element names to lowercase letters recursively.
        /// </summary>
        /// <param name="xDoc">The document to convert.</param>
        public static void LowerCaseAllElementNames(this XDocument xDoc)
        {
            if (xDoc == null)
            {
                throw new ArgumentNullException("xDoc");
            }

            foreach (XElement item in xDoc.Elements())
            {
                item.LowerCaseAllElementNames();
            }
        }

        /// <summary>
        /// Converts element names to lower case letters recursively.
        /// </summary>
        /// <param name="xElement">The element to start conversion from.</param>
        /// <returns>The converted element.</returns>
        public static XElement LowerCaseAllElementNames(this XElement xElement)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException("xElement");
            }

            xElement.Name = xElement.Name.Namespace + xElement.Name.LocalName.ToLower();
            foreach (XElement item in xElement.Elements())
            {
                LowerCaseAllElementNames(item);
            }

            return xElement;
        }
    }
}
