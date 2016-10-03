using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LucidWrapper.Classes
{

    [Serializable, XmlRoot("document"), XmlType("document")]
    public class LucidDocument
    {
        [XmlElement("documentId")]
        public string DocumentId { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("blocks")]
        public LucidBlocks Blocks { get; set; }

        [XmlElement("lines")]
        public LucidLines Lines { get; set; }

    }

    [Serializable]
    public class LucidBlocks
    {
        [XmlElement("block")]
        public List<LucidBlock> Blocks { get; set; }
    }


    [Serializable]
    public class LucidBlock
    {
        [XmlElement("id")]

        public string Id { get; set; }

        [XmlElement("class")]

        public string Class { get; set; }

        [XmlElement("text")]

        public string Text { get; set; }
    }
    [Serializable]
    public class LucidLines
    {
        [XmlElement("line")]
        public List<LucidLine> Lines { get; set; }
    }

    [Serializable]
    public class LucidLine
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("endpoint1")]
        public LucidEndPoint EndPoint1 { get; set; }

        [XmlElement("endpoint2")]
        public LucidEndPoint EndPoint2 { get; set; }
    }


    public class LucidEndPoint
    {
        [XmlElement("style")]
        public string Style { get; set; }
        [XmlElement("block")]
        public string BlockId { get; set; }
    }
}
