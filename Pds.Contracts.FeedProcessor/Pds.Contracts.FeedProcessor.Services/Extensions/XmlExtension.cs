using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pds.Contracts.FeedProcessor.Services.Extensions
{
    /// <summary>
    /// Xml extensions.
    /// </summary>
    public static class XmlExtension
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="xmlElement">The XML element.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="ns">The ns.</param>
        /// <param name="isOptional">if set to <c>true</c> [is optional].</param>
        /// <returns>Inner text of xml element converted to type of <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">Required node at '[{xpath}]' is missing.</exception>
        public static T GetValue<T>(this XmlElement xmlElement, string xpath, XmlNamespaceManager ns, bool isOptional = false, T defaultValue = default)
        {
            var node = xmlElement.SelectSingleNode(xpath, ns);

            //This is a workaround to support Mock system that will be used for testing as the Mock system did not follow XSD and created elements in all lowercase.
            node ??= xmlElement.SelectSingleNode(xpath.ToLower(), ns);

            if (node is null && !isOptional)
            {
                throw new InvalidOperationException($"Required node at '[{xpath}]' is missing.");
            }

            return string.IsNullOrEmpty(node?.InnerText) ? defaultValue : (T)Convert.ChangeType(node.InnerText, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
        }

        /// <summary>
        /// Selects the nodes using xpath ignores case.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="ns">The ns.</param>
        /// <returns>A <see cref="XmlNodeList"/> of all nodes for xpath.</returns>
        public static XmlNodeList SelectNodesIgnoreCase(this XmlElement element, string xpath, XmlNamespaceManager ns)
        {
            var nodes = element.SelectNodes(xpath, ns);
            return nodes?.Count > 0 ? nodes : element.SelectNodes(xpath.ToLower(), ns);
        }
    }
}
