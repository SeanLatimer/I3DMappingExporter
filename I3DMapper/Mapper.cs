using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace I3DMapper
{
  /// <summary>Takes an I3D XML and generates a set of I3D Mapings.</summary>
  public class Mapper
  {
    private XDocument xDocument;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mapper"/> class.
    /// </summary>
    /// <param name="xDocument">XML file to generate mappings from.</param>
    public Mapper(XDocument xDocument)
    {
      this.xDocument = xDocument;
    }

    /// <summary>Gets the I3D Mappings.</summary>
    public XDocument Mappings { get; private set; }

    /// <summary>Generates a set of I3D Mappings.</summary>
    /// <param name="shortIDs">Generate with shorter IDs.</param>
    public void Map(bool shortIDs = false)
    {
      this.Mappings = new XDocument();
      this.Mappings.Add(new XElement("i3dMappings"));

      var scene = this.xDocument.Root.Element("Scene").Elements().ToList();

      for (int i = 0; i < scene.Count(); i++)
      {
        XElement element = scene[i];
        string currentNodeId = element.Attribute("name")?.Value ?? "unkown_node";
        string currentNode = i.ToString() + ">";

        this.Mappings.Element("i3dMappings")
            .Add(
            new XElement(
                "i3dMapping",
                new XAttribute("id", currentNodeId),
                new XAttribute("node", currentNode)));

        if (element.HasElements)
        {
          this.MapChildren(element.Elements().ToList(), !shortIDs ? currentNodeId : string.Empty, currentNode);
        }
      }

      this.ResolveDuplicates();
    }

    private void MapChildren(List<XElement> el, string parentNodeId, string indexPath)
    {
      bool shortID = string.IsNullOrEmpty(parentNodeId);
      for (int i = 0; i < el.Count(); i++)
      {
        var element = el[i];
        string currentNodeId = !shortID ?
            string.Format("{0}_{1}", parentNodeId, element.Attribute("name")?.Value ?? "unkown_node")
            : element.Attribute("name")?.Value ?? "unkown_node";

        string currentNode = indexPath.EndsWith(">") ? indexPath + i.ToString() : indexPath + "|" + i.ToString();

        this.Mappings.Element("i3dMappings")
            .Add(
                new XElement(
                    "i3dMapping",
                    new XAttribute("id", currentNodeId),
                    new XAttribute("node", currentNode)));

        if (element.HasElements)
        {
          this.MapChildren(element.Elements().ToList(), !shortID ? currentNodeId : string.Empty, currentNode);
        }
      }
    }

    private void ResolveDuplicates()
    {
      var dupeGroups = from dupe in this.Mappings.Root.Descendants()
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
