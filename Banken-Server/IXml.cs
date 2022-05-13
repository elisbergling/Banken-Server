using System.Xml;

namespace Banken_Server
{
    public interface IXml
    {
        void Save(XmlDocument xmlDoc, XmlDocument xmlDocId);
        void Delete(XmlDocument xmlDoc);
    }
}