using System.Xml;

namespace Banken_Server
{
    public class Account : IXml
    {
        string? name;
        string? balance;
        string? id;
        string? rate;
        string? ownerId;
        
        public Account(string text)
        {
            string[] splitedString = text.Split('@');

            if (splitedString.Length == 6)
            {
                name = splitedString[1];
                balance = splitedString[2];
                id = splitedString[3];
                rate = splitedString[4];
                ownerId = splitedString[5];
            }
            else
            {
                id = splitedString[1];
                ownerId = splitedString[2];
            }
        }

        public Account(string balance, string name, string id, string rate)
        {
            this.name = name;
            this.balance = balance;
            this.id = id;
            this.rate = rate;
        }

        public void Save(XmlDocument xmlDoc, XmlDocument xmlDocId)
        {
            //Sparar id-nummret som kan användas senare för att se till att inte två stycken har samma nummer
            xmlDocId.SelectSingleNode("ids/accountId")!
                .InnerText = id;
            xmlDocId.Save("ids.xml");
            
            //Sparar Kontoinformation
            XmlElement accountXml = xmlDoc.CreateElement("account");
            xmlDoc.SelectSingleNode($"costumers/costumer[userId={ownerId}]/accounts")?.AppendChild(accountXml);

            XmlElement nameNode = xmlDoc.CreateElement("name");
            nameNode.InnerText = name;
            accountXml.AppendChild(nameNode);

            XmlElement balanceNode = xmlDoc.CreateElement("balance");
            balanceNode.InnerText = balance;
            accountXml.AppendChild(balanceNode);

            XmlElement idNode = xmlDoc.CreateElement("id");
            idNode.InnerText = id;
            accountXml.AppendChild(idNode);

            XmlElement rateNode = xmlDoc.CreateElement("rate");
            rateNode.InnerText = rate;
            accountXml.AppendChild(rateNode);

            xmlDoc.Save("costumers.xml");
        }


        public void Delete(XmlDocument xmlDoc)
        {
            //Tar bort kontot
            XmlNode accountsNode = xmlDoc.SelectSingleNode($"costumers/costumer[userId={ownerId}]/accounts")!;
            XmlNode accountNode = accountsNode.SelectSingleNode($"account[id={id}]")!;
            accountsNode.RemoveChild(accountNode);
            xmlDoc.Save("costumers.xml");
        }

        public void ChangeBalance(XmlDocument xmlDoc)
        {
            xmlDoc.SelectSingleNode($"costumers/costumer[userId={ownerId}]/accounts/account[id={id}]/balance")!
                .InnerText = balance;

            xmlDoc.Save("costumers.xml");
        }

        public override string ToString()
        {
            return name + '@' + balance + '@' + id + '@' + rate;
        }
    }
}