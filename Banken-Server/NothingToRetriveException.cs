using System;
using System.Net.Sockets;
using System.Text;

namespace Banken_Server
{
    public class NothingToRetriveException : Exception
    {
        string message;

        public override string Message
        {
            get { return message; }
        }

        public NothingToRetriveException(Socket socket)
        {
            message = "Det fanns inget att hämta.";
            byte[]
                bSend = Encoding.UTF8
                    .GetBytes("0"); //0 används för att meddela Klienten att det inte finns någrot att skicka
            socket.Send(bSend);
        }
    }
}