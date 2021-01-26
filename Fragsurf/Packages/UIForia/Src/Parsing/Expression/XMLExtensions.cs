using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace UIForia.Parsing.Expressions {

    public static class XMLExtensions {

        public static XAttribute GetAttribute(this XElement element, string attrName) {
            return element.Attributes(attrName).FirstOrDefault();
        }

        public static XCData GetCDataChild(this XElement element) {
            XNode ptr = element.FirstNode;
            while (ptr != null) {
                if (ptr.NodeType == XmlNodeType.CDATA) {
                    return ptr as XCData;
                }
                ptr = ptr.NextNode;
            }

            return null;
        }
        
        public static XElement GetChild(this XElement element, string tagName) {
            return element.Elements(tagName).FirstOrDefault();
        }

        public static IEnumerable<XElement> GetChildren(this XElement element, string tagName) {
            return element.Elements(tagName);
        }

        public static IEnumerable<XElement> GetChildren(this XElement element) {
            return element.Elements();
        }

        public static int GetValueAsInt(this XAttribute attr) {
            return int.Parse(attr.Value);
        }

        public static void MergeTextNodes(this XContainer element) {
            List<XNode> nodes = element.Nodes()
                .Where((n) => n.NodeType == XmlNodeType.Element || n.NodeType == XmlNodeType.Text || n.NodeType == XmlNodeType.CDATA).ToList();

            if (nodes.Count == 0) {
                return;
            }

            List<XNode> output = new List<XNode>();
            output.Add(nodes[0]);

            for (int i = 1; i < nodes.Count; i++) {
                XNode lastOutput = output[output.Count - 1];
                if (lastOutput.NodeType == XmlNodeType.Text && nodes[i].NodeType == XmlNodeType.Text) {
                    ((XText) lastOutput).Value += ((XText) nodes[i]).Value;
                }
                else {
                    output.Add(nodes[i]);
                }
            }

            for (int i = 0; i < output.Count; i++) {
                if (output[i].NodeType == XmlNodeType.Element) {
                    MergeTextNodes((XElement) output[i]);
                }
            }
            
            element.ReplaceNodes(output);

        }

    }

}