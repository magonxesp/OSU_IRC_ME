using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OSU_IRC_ME.ChatInspector {
    class ChatLogArgs : EventArgs {

        private string path;

        public ChatLogArgs(string path) {
            this.path = path;
        }

        public string FileName {
            get {
                return Path.GetFileName(path);
            }
        }

        public string FilePath {
            get {
                return path;
            }
        }

    }
}
