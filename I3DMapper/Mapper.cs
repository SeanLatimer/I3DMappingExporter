using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace I3DMapper
{
    public class Mapper
    {
        public XDocument Mappings { get; private set; }

        private XDocument xDocument;

        public Mapper(XDocument xDocument)
        {
            this.xDocument = xDocument;
        }

        public void Map()
        {
            Mappings = new XDocument();
            Mappings.Add(new XElement("i3dMappings"));

            var scene = this.xDocument.Root.Element("Scene").Elements().ToList();

            for (int i = 0; i < scene.Count(); i++)
            {
                XElement element = scene[i];
                string currentNodeId = element.Attribute("name")?.Value ?? "unkown_node";
                string currentNode = i.ToString() + ">";

                Mappings.Element("i3dMappings")
                    .Add(
                    new XElement("i3dMapping",
                    new XAttribute("id", currentNodeId),
                    new XAttribute("node", currentNode)
                    ));

                if (element.HasElements)
                {
                    mapChildren(element.Elements().ToList(), currentNodeId, currentNode);
                }
            }
        }

        private void mapChildren(List<XElement> el, string parentNodeId, string indexPath)
        {
            for (int i = 0; i < el.Count(); i++)
            {
                var element = el[i];
                string currentNodeId = string.Format("{0}_{1}", parentNodeId, element.Attribute("name")?.Value ?? "unkown_node");
                string currentNode = indexPath.EndsWith(">") ? indexPath + i.ToString() : indexPath + "|" + i.ToString();

                Mappings.Element("i3dMappings")
                    .Add(
                    new XElement("i3dMapping",
                    new XAttribute("id", currentNodeId),
                    new XAttribute("node", currentNode)
                    ));

                if (element.HasElements)
                {
                    mapChildren(element.Elements().ToList(), currentNodeId, currentNode);
                }
            }
        }
    }
}
