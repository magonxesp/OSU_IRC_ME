using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Meebey.SmartIrc4net;

namespace OSU_IRC_ME.IRC {

    public class IRCClientConnection {

        private string nickname;
        private string password;
        private string host;
        private int port;
        private IrcClient client;
        private Thread ircTask;
        public event EventHandler OnConnect;
        public event EventHandler OnConnecting;
        public event EventHandler OnConnectFail;
        public event CtcpEventHandler OnMessage;

        public IrcClient Client {
            get {
                return client;
            }
        }
        
        public IRCClientConnection(string nickname, string password, string host, int port) {
            this.nickname = nickname;
            this.password = password;
            this.host = host;
            this.port = port;
            client = new IrcClient();
        }

        public void Connect() {
            RegisterEventHandlers();
            client.Connect(host, port);
            client.Login(nickname, nickname, 0, nickname, password);
            ircTask = new Thread(new ThreadStart(() => client.Listen()));
            ircTask.Start();
        }

        private void RegisterEventHandlers() {
            client.OnConnected += OnConnect;
            client.OnConnecting += OnConnecting;
            client.OnConnectionError += OnConnectFail;
            client.OnCtcpReply += OnMessage;
        }

        public bool SendMessage(SendType type, string destination, string msg) {
            try {
                client.SendMessage(type, destination, msg);
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        public void Stop() {
            if (ircTask != null) {
                if (client.IsConnected) {
                    client.Disconnect();
                }
                
                ircTask.Abort();
            }
        }

    }
}
