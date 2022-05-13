using System.Xml;


namespace Banken_Server
{
    public class Costumer : IXml
    {
        string name;
        string userId;

        public string Name
        {
            get { return name; }
        }

        public string UserId
        {
            get { return userId; }
        }

        public Costumer(string text)
        {
            string[] splitedString = text.Split('@');

            name = splitedString[1];
            userId = splitedString[2];
        }

        public Costumer(string name, string userId)
        {
            this.name = name;
            this.userId = userId;
        }

        public void Save(XmlDocument xmlDoc, XmlDocument xmlDocId)
        {
            //Sparar id-nummret som kan användas senare för att se till att inte två stycken har samma nummer
            xmlDocId.SelectSingleNode("ids/costumerId")!
                .InnerText = userId;
            xmlDocId.Save("ids.xml");

            //Sparar kundinformation
            XmlElement costumerXml = xmlDoc.CreateElement("costumer");
            xmlDoc.SelectSingleNode("costumers")?.AppendChild(costumerXml);

            XmlElement nameNode = xmlDoc.CreateElement("name");
            nameNode.InnerText = name;
            costumerXml.AppendChild(nameNode);

            XmlElement userIdNode = xmlDoc.CreateElement("userId");
            userIdNode.InnerText = userId;
            costumerXml.AppendChild(userIdNode);

            XmlElement accounts = xmlDoc.CreateElement("accounts");
            costumerXml.AppendChild(accounts);

            xmlDoc.Save("costumers.xml");
        }

        public void Delete(XmlDocument xmlDoc)
        {
            //Tar bort kontot
            XmlNode costumersNode = xmlDoc.SelectSingleNode("costumers")!;
            XmlNode costumerNode = costumersNode.SelectSingleNode($"costumer[userId={userId}]")!;
            costumerNode.RemoveAll();
            costumersNode.RemoveChild(costumerNode);
            xmlDoc.Save("costumers.xml");
        }

        public override string ToString()
        {
            return name + '@' + userId;
        }
    }
}