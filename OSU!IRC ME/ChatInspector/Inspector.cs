using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.IO;
using Meebey.SmartIrc4net;

namespace OSU_IRC_ME.ChatInspector {

    class Inspector {
        
        private string osuInstallationPath;
        private string osuChatLogsPath;
        private string targetUser;
        public delegate void OnChatLogHandler(object sender, ChatLogArgs e);
        public delegate void OnMessageHandler(object sender, MessageArgs e);
        public event OnChatLogHandler ChatLog;
        public event OnMessageHandler Message;

        public Inspector(string osuInstallationPath, string targetUser) {
            this.osuInstallationPath = osuInstallationPath;
            this.targetUser = targetUser;
            this.osuChatLogsPath = osuInstallationPath + "\\Chat";
        }

        private string GetRecentChatLog() {
            DateTime mostRecentLogFile = DateTime.MinValue;
            string recentChatLogFile = "";

            try {
                string[] chatFiles = Directory.GetFiles(osuChatLogsPath, targetUser + "*");
                
                foreach (string chatFile in chatFiles) {
                    DateTime datetime = File.GetLastWriteTime(chatFile);

                    if (datetime.Millisecond > mostRecentLogFile.Millisecond) {
                        mostRecentLogFile = datetime;
                        recentChatLogFile = chatFile;
                    }
                }
            } catch (IOException e) {
                Console.WriteLine(e.Message);
            }

            return recentChatLogFile;
        }

        private string GetRecentMessage(string chatLogFile) {
            string message = "";

            try {
                FileStream fileStream = File.OpenRead(chatLogFile);
                StreamReader reader = new StreamReader(fileStream);
                List<string> messages = new List<string>();
                string line;

                while ((line = reader.ReadLine()) != null) {
                    messages.Add(line);
                }

                message = messages.Last();

                reader.DiscardBufferedData();
                fileStream.Dispose();

                reader.Close();
                fileStream.Close();
            } catch (IOException e) {
                Console.WriteLine(e.Message);
            }

            return message;
        }

        private MessageArgs CleanRawMessage(string rawMessage) {
            string message = rawMessage
                .Split(new string[] { targetUser + ":", targetUser }, StringSplitOptions.None)
                .Last()
                .Trim();

            SendType type;

            if(rawMessage.Contains("*" + targetUser)) {
                type = SendType.Action;
            } else {
                type = SendType.Message;
            }

            return new MessageArgs(type, message, rawMessage);
        }

        public void Inspect() {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = osuChatLogsPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = targetUser + "*";
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e) {
            string rawMessage = GetRecentMessage(e.FullPath);
            Message(this, CleanRawMessage(rawMessage));
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e) {
            string rawMessage = GetRecentMessage(e.FullPath);
            ChatLog(this, new ChatLogArgs(e.FullPath));
            Message(this, CleanRawMessage(rawMessage));
        }

        public void StopInspect() {
            // deprecated
        }

    }
}
