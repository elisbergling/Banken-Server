using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace Banken_Server
{
    class Program
    {
        static TcpListener? tcpListener;

        // =======================================================================
        // Main(), lyssnar efter trafik. Loopar till dess att ctrl-C trycks. I
        // varje varv i loopen väntar servern på en ny anslutning.
        // =======================================================================
        public static void Main()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlDocId = new XmlDocument();

            //Skapar filer om de inte finns, annars laddar enbart in dem
            if (!File.Exists("costumers.xml"))
            {
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                XmlElement costumers = xmlDoc.CreateElement("costumers");
                xmlDoc.AppendChild(costumers);

                XmlElement ids = xmlDocId.CreateElement("ids");
                xmlDocId.AppendChild(ids);

                XmlElement accountId = xmlDocId.CreateElement("accountId");
                accountId.InnerText = "0";
                ids.AppendChild(accountId);

                XmlElement costumerId = xmlDocId.CreateElement("costumerId");
                costumerId.InnerText = "0";

                ids.AppendChild(costumerId);

                xmlDoc.Save("costumers.xml");
                xmlDocId.Save("ids.xml");
            }
            else
            {
                xmlDoc.Load("costumers.xml");
                xmlDocId.Load("ids.xml");
            }


            Console.CancelKeyPress += CancelKeyPress;
            // Skapa ett TcpListener-objekt, börja lyssna och vänta på anslutning
            IPAddress myIp = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(myIp, 8001);
            tcpListener.Start();
            Console.WriteLine("Väntar på anslutning...");
            // Någon försöker ansluta. Acceptera anslutningen
            Socket socket = tcpListener.AcceptSocket();
            Console.WriteLine("Anslutning accepterad från " +
                              socket.RemoteEndPoint);
            bool flag = true;
            while (flag)
            {
                try
                {
                    // Tag emot meddelandet
                    byte[] bMessage = new byte[256];
                    int messageSize = socket.Receive(bMessage);
                    Console.WriteLine("Meddelandet mottogs...");
                    // Spara meddelandet i XML
                    string message = "";
                    for (int i = 0; i < messageSize; i++)
                        message += Convert.ToChar(bMessage[i]);
                    Console.WriteLine(message);
                    //Ser vilken kod som är angiven. Detta bestämmer vad klienten vill göra på servern
                    switch (message[0])
                    {
                        case '0':
                            SaveCostumer(message, xmlDoc, xmlDocId);
                            break;
                        case '1':
                            SaveAccount(message, xmlDoc, xmlDocId);
                            break;
                        case '2':
                            DeleteCostumer(message, xmlDoc);
                            break;
                        case '3':
                            DeleteAccount(message, xmlDoc);
                            break;
                        case '4':
                            RetriveAndSendCostumers(xmlDoc, socket);
                            break;
                        case '5':
                            RetriveAndSendAccounts(message, xmlDoc, socket);
                            break;
                        case '6':
                            ChangeBalance(message, xmlDoc);
                            break;
                        case '7':
                            RetriveAndSendIds(xmlDocId, socket);
                            break;
                        case '8':
                            // Sluta lyssna efter trafik:
                            tcpListener.Stop();
                            socket.Close();
                            Console.WriteLine("Servern stängdes av!");
                            flag = false;
                            break;
                    }
                }
                catch (Exception e)
                {
                    tcpListener.Stop();
                    socket.Close();
                    Console.WriteLine("Servern kopplades bort");
                    Console.WriteLine("Error Message: " + e.Message);
                    Console.WriteLine("Error: " + e);
                    break;
                }
            }

            Console.WriteLine("Servern säger hejdå");
        }

        static void RetriveAndSendCostumers(XmlDocument xmlDoc, Socket socket)
        {
            try
            {
                //Hämtar från XML fil
                XmlNodeList costumers =
                    xmlDoc.SelectNodes("costumers/costumer") ?? throw new NothingToRetriveException(socket);

                if (costumers.Count == 0) throw new NothingToRetriveException(socket);

                string ms = "";

                //Alla meddelanden lägs tillsammans på en sträng
                foreach (XmlNode costumer in costumers)
                {
                    string name = costumer.SelectSingleNode("name")!.InnerText;
                    string userId = costumer.SelectSingleNode("userId")!.InnerText;

                    ms += new Costumer(name, userId) + "$";
                }
                
                //Skickar meddelanderna till Klienten
                byte[] bSend = Encoding.UTF8.GetBytes(ms);
                socket.Send(bSend);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void RetriveAndSendAccounts(string text, XmlDocument xmlDoc, Socket socket)
        {
            try
            {
                string[] list = text.Split('@');
                //Hämtar från XML fil
                XmlNodeList accounts = xmlDoc.SelectNodes($"costumers/costumer[userId={list[1]}]/accounts/account") ??
                                       throw new NothingToRetriveException(socket);

                if (accounts.Count == 0) throw new NothingToRetriveException(socket);

                string ms = "";

                //Alla meddelanden lägs tillsammans på en sträng
                foreach (XmlNode account in accounts)
                {
                    string name = account.SelectSingleNode("name")!.InnerText;
                    string balance = account.SelectSingleNode("balance")!.InnerText;
                    string id = account.SelectSingleNode("id")!.InnerText;
                    string rate = account.SelectSingleNode("rate")!.InnerText;

                    ms += new Account(balance, name, id, rate) + "$";
                }

                //Skickar meddelanderna till Klienten
                byte[] bSend = Encoding.UTF8.GetBytes(ms);
                socket.Send(bSend);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void RetriveAndSendIds(XmlDocument xmlDocId, Socket socket)
        {
            try
            {
                //Hämtar från XML fil
                XmlNode aId = xmlDocId.SelectSingleNode("ids/accountId")!;
                XmlNode cId = xmlDocId.SelectSingleNode("ids/costumerId")!;

                string ms = aId.InnerText + '$' + cId.InnerText;
                
                Console.WriteLine(ms);

                //Skickar meddelanderna till Klienten
                byte[] bSend = Encoding.UTF8.GetBytes(ms);
                socket.Send(bSend);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void SaveCostumer(string text, XmlDocument xmlDoc, XmlDocument xmlDocId)
        {
            //Sparar kunden
            Costumer c = new Costumer(text);
            c.Save(xmlDoc, xmlDocId);
        }

        static void SaveAccount(string text, XmlDocument xmlDoc, XmlDocument xmlDocId)
        {
            //Sparar kontot
            Account a = new Account(text);
            a.Save(xmlDoc, xmlDocId);
        }

        static void ChangeBalance(string text, XmlDocument xmlDoc)
        {
            //Sparar kontot
            Account a = new Account(text);
            a.ChangeBalance(xmlDoc);
        }

        static void DeleteCostumer(string text, XmlDocument xmlDoc)
        {
            //Tar bort kunden
            Costumer c = new Costumer(text);
            c.Delete(xmlDoc);
        }

        static void DeleteAccount(string text, XmlDocument xmlDoc)
        {
            //Tar bort kontot
            Account a = new Account(text);
            a.Delete(xmlDoc);
        }

        // =======================================================================
        // CancelKeyPress(), anropas då användaren trycker på Ctrl-C.
        // =======================================================================
        static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Sluta lyssna efter trafik
            tcpListener?.Stop();
            Console.WriteLine("Servern stängdes av!");
        }
    }
}