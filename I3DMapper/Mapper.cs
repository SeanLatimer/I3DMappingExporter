using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace I3DMapper
{
    public class Mapper
    {
        public XmlDocument Mappings { get; private set; }

        private XmlDocument xmlDocument;

        public Mapper(XmlDocument xmlDocument)
        {
            this.xmlDocument = xmlDocument;
        }

        public void Map()
        {
            Mappings = new XmlDocument();
            var mappingsNode = Mappings.CreateElement("i3dMappings");

            XmlNode scene = xmlDocument.SelectSingleNode("/i3D/Scene");


            for (int i = 0; i < scene.ChildNodes.Count; i++)
            {
                XmlNode node = scene.ChildNodes[i];
                XmlElement mapping = Mappings.CreateElement("i3dMapping");
                mapping.SetAttribute("id", node.Attributes["name"].Value);
                mapping.SetAttribute("node", i.ToString());
                mappingsNode.AppendChild(mapping);

                if (node.HasChildNodes)
                {
                    mapChildren(node.ChildNodes, i.ToString() + ">", ref mappingsNode);
                }
            }

            Mappings.AppendChild(mappingsNode);
        }

        private void mapChildren(XmlNodeList nl, string indexPath, ref XmlElement mappingsNode)
        {
            for (int i = 0; i < nl.Count; i++)
            {
                XmlNode node = nl[i];
                string currentPath = indexPath.EndsWith(">") ? indexPath + i.ToString() : indexPath + "|" + i.ToString();

                XmlElement mapping = Mappings.CreateElement("i3dMapping");
                mapping.SetAttribute("id", node.Attributes["name"].Value);
                mapping.SetAttribute("node", currentPath);
                mappingsNode.AppendChild(mapping);

                if (node.HasChildNodes)
                {
                    mapChildren(node.ChildNodes, currentPath, ref mappingsNode);
                }
            }
        }
    }
}
