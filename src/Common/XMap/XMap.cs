using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Coditate.Common.Util;

namespace Coditate.Common.XMap
{
    /// <summary>
    /// Uses Xpath queries to replace element or attribute values in an Xml document.
    /// </summary>
    /// <remarks>
    /// 
    /// 
    /// </remarks>
    public class XMap
    {
        /// <summary>
        /// Character which starts a comment line: #
        /// </summary>
        public const char CommentEscapeChar = '#';

        /// <summary>
        /// Regular expression for finding properties in the value.
        /// </summary>
        internal const string ParameterFinderRegex = @"\$\(([\w\.-]+)\)";

        private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        /// <summary>
        /// Adds a property name and value for use when updating Xml node values
        /// containing an expandable property name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, string value)
        {
            Arg.CheckNullOrEmpty("name", name);
            Arg.CheckNull("value", value);

            properties.Add(name.ToLower(), value);
        }

        /// <summary>
        /// Gets the value stored for the specified property name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetProperty(string name)
        {
            Arg.CheckNullOrEmpty("name", name);

            string value;
            properties.TryGetValue(name.ToLower(), out value);
            return value;
        }

        /// <summary>
        /// Updates an Xml file in place with values from 
        /// an XMap file.
        /// </summary>
        /// <param name="inputXmlPath">The input XML file path.</param>
        /// <param name="xMapPath">The optional XMap file path.</param>
        /// <exception cref="InvalidXpathExpressionException">When an included Xpath expression is invalid or partially resolvable</exception>
        /// <exception cref="PropertyNotRegisteredException">When an xmapping includes an unregistered property</exception>
        /// <exception cref="IOException">If unable to open any of the provided file paths</exception>
        public void UpdateNodes(string inputXmlPath, string xMapPath)
        {
            UpdateNodes(inputXmlPath, inputXmlPath, xMapPath);
        }

        /// <summary>
        /// Copies and updates an Xml file with values from an XMap file.
        /// </summary>
        /// <param name="inputXmlPath">The input XML file path.</param>
        /// <param name="outputXmlPath">The output XML file path.</param>
        /// <param name="xMapPath">The optional XMap file path.</param>
        /// <exception cref="InvalidXpathExpressionException">When an included Xpath expression is invalid or partially resolvable</exception>
        /// <exception cref="PropertyNotRegisteredException">When an xmapping includes an unregistered property</exception>
        /// <exception cref="IOException">If unable to open any of the provided file paths</exception>
        public void UpdateNodes(string inputXmlPath, string outputXmlPath, string xMapPath)
        {
            Arg.CheckNullOrEmpty("inputXmlPath", inputXmlPath);
            Arg.CheckNullOrEmpty("outputXmlPath", outputXmlPath);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(inputXmlPath);

            ExpandValues(xmlDoc.DocumentElement);

            if (!string.IsNullOrEmpty(xMapPath))
            {
                using (var reader = new StreamReader(xMapPath))
                {
                    string pathLine, valueLine;
                    do
                    {
                        pathLine = ReadNextValidLine(reader, false);
                        valueLine = ReadNextValidLine(reader, true);
                        if (pathLine != null)
                        {
                            ApplyMapping(xmlDoc, pathLine, valueLine);
                        }
                    } while (pathLine != null && valueLine != null);
                }
            }

            string tempXmlPath = Path.GetTempFileName();
            using (var xmlWriter = new XmlTextWriter(tempXmlPath, null))
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlDoc.WriteContentTo(xmlWriter);
            }

            var inputSecurity = File.GetAccessControl(inputXmlPath);

            File.Delete(outputXmlPath);
            File.Move(tempXmlPath, outputXmlPath);

            var outputSecurity = new FileSecurity();
            outputSecurity.SetSecurityDescriptorBinaryForm(inputSecurity.GetSecurityDescriptorBinaryForm());

            File.SetAccessControl(outputXmlPath, outputSecurity);
        }

        /// <summary>
        /// Expands properties embedded in Xml element and attribute values.
        /// </summary>
        private void ExpandValues(XmlNode xmlNode)
        {
            if (!string.IsNullOrEmpty(xmlNode.Value))
            {
                xmlNode.Value = ExpandValue(xmlNode.Value);
            }
            foreach (XmlNode child in xmlNode.ChildNodes)
            {
                if (child is XmlElement || child is XmlText)
                {
                    ExpandValues(child);
                }
            }
            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    attribute.Value = ExpandValue(attribute.Value);
                }
            }
        }

        private string ReadNextValidLine(StreamReader reader, bool acceptEmpty)
        {
            string nextLine;
            do
            {
                nextLine = reader.ReadLine();
                nextLine = StringUtils.TrimEx(nextLine);
                if ((string.IsNullOrEmpty(nextLine) && !acceptEmpty) || IsCommentLine(nextLine))
                {
                    continue;
                }
                return nextLine;
            } while (nextLine != null);
            return null;
        }

        private bool IsCommentLine(string value)
        {
            if (!string.IsNullOrEmpty(value) && value[0] == CommentEscapeChar)
            {
                return true;
            }
            return false;
        }

        private void ApplyMapping(XmlDocument xmlDoc, string xPath, string value)
        {
            if (value == null)
            {
                string message = string.Format("No value defined for mapping \"{0}\"", xPath);
                throw new FormatException(message);
            }

            Replace(xmlDoc, xPath, value);
        }

        /// <summary>
        /// Gets the property names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetPropertyNames()
        {
            return new List<string>(properties.Keys);
        }

        /// <summary>
        /// Replaces the Xml node value found at the specified path with a new value.
        /// </summary>
        /// <param name="xmlDoc">The Xml document.</param>
        /// <param name="nodePath">The XPath query identifying the Xml node to update.</param>
        /// <param name="value">The new attribute value.</param>
        public void Replace(XmlDocument xmlDoc, string nodePath, string value)
        {
            // todo: should refactor so that mappings are parsed into a structure
            // and passed to an UpdateValues accepting an XmlDocument and the mappings structure
            // and this method should be changed to accept an Xml node plus a single mapping

            Arg.CheckNull("xmlDoc", xmlDoc);
            Arg.CheckNull("nodePath", nodePath);
            Arg.CheckNull("value", value);

            XmlNodeList nodes = null;
            try
            {
                nodes = FindNode(xmlDoc, nodePath);
            }
            catch (XPathException ex)
            {
                ThrowInvalidPathException(nodePath, ex);
            }
            string newValue = ExpandValue(value);
            foreach (XmlNode node in nodes)
            {
                if (node is XmlAttribute)
                {
                    node.Value = newValue;
                }
                else if (node is XmlElement)
                {
                    SetContent(node, newValue);
                }
            }
        }

        private XmlNodeList FindNode(XmlDocument xmlDoc, string nodePath)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes(nodePath);
            if (nodes.Count == 0)
            {
                // if the root of the expression DOES reference a node but the full
                // expression DOES NOT reference a node, throw an exception.
                // SelectSingleNode() checks this condition EXCEPT
                // when the expression contains a predicate checking node values
                string nodePathRoot = GetPathRoot(nodePath);

                nodes = xmlDoc.SelectNodes(nodePathRoot);
                if (nodes.Count > 0)
                {
                    ThrowInvalidPathException(nodePath, null);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Gets the root of an Xpath expression, which is either the whole expression or everything 
        /// before the second slash.
        /// </summary>
        internal string GetPathRoot(string nodePath)
        {
            int index = 0;
            for (; index < nodePath.Length; index++)
            {
                if (nodePath[index] != '/')
                {
                    break;
                }
            }
            index = nodePath.IndexOf('/', index);
            if (index == -1)
            {
                return nodePath;
            }
            return nodePath.Substring(0, index);
        }

        private void SetContent(XmlNode node, string content)
        {
            string trimmed = content.Trim();
            if (trimmed.StartsWith("<") && trimmed.EndsWith(">"))
            {
                node.InnerXml = content;
            }
            else
            {
                node.InnerText = content;
            }
        }

        private void ThrowInvalidPathException(string nodePath, Exception source)
        {
            string message = string.Format("XPath expression \"{0}\" does not reference an element or attribute",
                                           nodePath);
            throw new InvalidXpathExpressionException(message, nodePath, source);
        }

        /// <summary>
        /// Expands a mapped value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string ExpandValue(string value)
        {
            Arg.CheckNull("value", value);

            string expandedValue = Regex.Replace(value, ParameterFinderRegex, GetPropertyValue);
            return expandedValue;
        }

        private string GetPropertyValue(Match match)
        {
            string propertyName = match.Value.Substring(2, match.Value.Length - 3);
            string propertyValue = GetProperty(propertyName);

            if (propertyValue == null)
            {
                string message =
                    string.Format(
                        "Unable to expand property named \"{0}\". Property value was never provided.",
                        propertyName);
                throw new PropertyNotRegisteredException(message, propertyName);
            }

            return ExpandValue(propertyValue);
        }
    }
}