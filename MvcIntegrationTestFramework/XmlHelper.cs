using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MvcApplication4.Tests.IntegrationTestFramework
{
    public static class XmlHelper
    {
        public static string Trim(string xmlString)
        {
            xmlString = Regex.Replace(xmlString, @"\s+", " "); //remove redundand spaces
            xmlString = xmlString.Trim('\r', '\n'); //removing carriage return and new-line
            xmlString = xmlString.TrimStart(' '); //single space at the beginning of xml string is not allowed
            return xmlString;
        }

        public static dynamic ToDynamic(string xml)
        {
            xml = Trim(xml);
            var document = new XmlDocument();
            document.LoadXml(xml);

            return ToDynamic(document);
        }
        public static dynamic ToDynamicOrNull(string xml)
        {
            try
            {
                return ToDynamic(xml);
            }
            catch (XmlException)
            {
                return null;
            }
        }

        public static dynamic ToDynamic(XmlDocument document)
        {
            XmlElement root = document.DocumentElement;
            return ToObject(root, new ExpandoObject());
        }

        private static dynamic ToObject(XmlNode node, ExpandoObject config, int count = 1)
        {
            var parent = config as IDictionary<string, object>;
            foreach (XmlAttribute nodeAttribute in node.Attributes)
            {
                var nodeAttrName = nodeAttribute.Name;
                parent[nodeAttrName] = nodeAttribute.Value;
            }
            foreach (XmlNode nodeChild in node.ChildNodes)
            {
                string nodeText;
                if (IsTextOrCDataSection(nodeChild, out nodeText))
                {
                    parent["Value"] = nodeChild.Value;
                }
                else
                {
                    var nodeChildName = nodeChild.Name;
                    var isText = IsTextOrCDataSection(nodeChild.FirstChild, out nodeText);

                    if (parent.ContainsKey(nodeChildName))
                    {
                        parent[nodeChildName + "_" + count] =
                            isText ? nodeText : ToObject(nodeChild, new ExpandoObject(), count++);
                    }
                    else
                    {
                        parent[nodeChildName] =
                            isText ? nodeText : ToObject(nodeChild, new ExpandoObject());
                    }
                }
            }
            return config;
        }

        private static bool IsTextOrCDataSection(XmlNode node, out string nodeText)
        {
            nodeText = null;
            bool isText = (node == null || node.Name == "#text" || node.Name == "#cdata-section");
            if (node != null && isText)
            {
                nodeText = node.Value;
            }
            return isText;
        }
    }
}
