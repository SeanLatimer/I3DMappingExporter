using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public void Map(bool shortIDs = false)
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
                    mapChildren(element.Elements().ToList(), !shortIDs ? currentNodeId : "", currentNode);
                }
            }
            resolveDuplicates();
        }

        private void mapChildren(List<XElement> el, string parentNodeId, string indexPath)
        {
            bool shortID = string.IsNullOrEmpty(parentNodeId);
            for (int i = 0; i < el.Count(); i++)
            {
                var element = el[i];
                string currentNodeId = !shortID ?
                    string.Format("{0}_{1}", parentNodeId, element.Attribute("name")?.Value ?? "unkown_node")
                    : element.Attribute("name")?.Value ?? "unkown_node";

                string currentNode = indexPath.EndsWith(">") ? indexPath + i.ToString() : indexPath + "|" + i.ToString();

                Mappings.Element("i3dMappings")
                    .Add(
                    new XElement("i3dMapping",
                    new XAttribute("id", currentNodeId),
                    new XAttribute("node", currentNode)
                    ));

                if (element.HasElements)
                {
                    mapChildren(element.Elements().ToList(), !shortID ? currentNodeId : "", currentNode);
                }
            }
        }

        private void resolveDuplicates()
        {
            var dupeGroups = from dupe in Mappings.Root.Descendants()
                             group dupe by (string)dupe.Attribute("id") into grp
                             where grp.Count() > 1
                             select grp;

            foreach (var grp in dupeGroups)
            {
                var lst = grp.ToList();
                for (int i = 0; i < lst.Count; i++)
                {
                    lst[i].Attribute("id").Value = string.Format("{0}_{1}", (string)lst[i].Attribute("id"), i);
                }
            }
        }
    }
}
