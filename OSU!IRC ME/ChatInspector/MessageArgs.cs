using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;

namespace OSU_IRC_ME.ChatInspector {
    class MessageArgs : EventArgs {

        private string rawMessage;
        private string message;
        private SendType type;

        public MessageArgs(SendType type, string message) {
            this.message = message;
            this.type = type;
        }

        public MessageArgs(SendType type, string message, string rawMessage) {
            this.message = message;
            this.type = type;
            this.rawMessage = rawMessage;
        }

        public string RawMessage {
            get {
                return rawMessage;
            }
        }

        public string Message {
            get {
                return message;
            }
        }

        public SendType Type {
            get {
                return type;
            }
        }

    }
}
