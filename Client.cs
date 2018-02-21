using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace irc
{
    public class Client
    {
        public string Nick { get; private set; }
        public Client(string username)
        {
            Nick = username;
        }

        public void Connect(ServerParameters parameters, bool? autoConnect = null)
        {
            var server = new Server(this, parameters);
        }
        
        public delegate void ChannelMessageReceived(Client sender, Server server, Channel channel, Person person, Message message);
        public delegate void PrivateMessageReceived(Client sender, Server server, Person person, Message message);
    }

    public  class Server 
    {
        public ServerParameters Parameters { get; set; }
        private Stream Stream { get; set; }
        private Client Client { get; set; }

        public Server(Client client, ServerParameters parameters) 
        {
            Parameters = parameters;
            Client = client;
        }

        public async void Connect()
        {
            var client = new TcpClient();
            await client.ConnectAsync(Parameters.Host, Parameters.Port ?? 6667);

            if(Parameters.useSSL == true)
            {
                Stream = new SslStream(client.GetStream());
            }
            else
            {
                Stream = client.GetStream();
            }
            var nick =Parameters.OverrideNick ?? Client.Nick;
            SendRaw($"NICK {nick}");
            SendRaw($"USER {nick} 0 * :{nick}");
        }

        private async void ListenToMessages()
        {

        }

        public void Send(string channel, string message)
        {
            SendRaw($"PRIVMSG {channel} :{message}");
        }

        public void Send(Channel channel, string message)
        {
            SendRaw($"PRIVMSG {channel.Name} :{message}");
        }

        public void Send(Person person, string message)
        {
            SendRaw($"PRIVMSG {person.Nick} :{message}");
        }

        public void SendRaw(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str + "\r\n");
            Stream.Write(bytes, 0, bytes.Length);
        }
    }

    public class Person
    {
        public string Nick { get; set; }
        public string Host { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
        public bool IsAction { get; set; }
        public DateTime ReceivedOn { get; set; }
    }

    public class Channel 
    {
        public string Name { get; set; }
    }

    public class ServerParameters
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public bool? useSSL { get; set; }
        public string OverrideNick { get; set; }
        public string Password { get; set; }
    }
}
