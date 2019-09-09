using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Security.Principal;
using OSU_IRC_ME.IRC;
using OSU_IRC_ME.ChatInspector;
using Meebey.SmartIrc4net;


namespace OSU_IRC_ME {
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private IRCClientConnection irc;
        private Inspector inspector; 
        private bool isValidInstallationPath = false;
        private string installationPath;
        private string targetUsername;

        public MainWindow() {
            InitializeComponent();
            SetupEventListeners();
        }

        private void SetupEventListeners() {
            Loaded += OnLoad;
            Closing += OnClosing;
        }

        private bool IsValidOsuInstallationPath(string installationPath) {
            return Directory.Exists(installationPath) && File.Exists(installationPath + "\\osu!.exe");
        }

        private string GetDefaultOsuInstallationPath() {
            return "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\osu!";
        }

        private void SetupOsuInstallationPath() {
            installationPath = GetDefaultOsuInstallationPath();

            if (IsValidOsuInstallationPath(installationPath)) {
                OsuInstallationPathTextBox.Text = GetDefaultOsuInstallationPath();
            } else {
                installationPath = SetOsuInstallationDirectory();

                if (installationPath != null) {
                    OsuInstallationPathTextBox.Text = installationPath;
                    isValidInstallationPath = true;
                }
            }
        }

        private string SetOsuInstallationDirectory() {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "Select your osu! installation path";
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) {
                string osuInstalationPath = folderBrowserDialog.SelectedPath;

                if (IsValidOsuInstallationPath(osuInstalationPath)) {
                    return osuInstalationPath;
                }
            }

            return null;
        }

        private void ConnectToIRC(string username, string password) {
            irc = new IRCClientConnection(username, password, "irc.ppy.sh", 6667);
            irc.OnConnect += OnIrcConnect;
            irc.OnConnectFail += OnIrcConnectFail;
            irc.OnConnecting += OnIrcConnecting;
            irc.Connect();
        }

        /// <summary>
        /// Force UI Update
        /// See more: https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread
        /// </summary>
        private void Update() {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter) {
                frame.Continue = false;
                return null;
            }), null);

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        private void StartChatInspector(string osuInstallationPath, string targetUsername) {
            inspector = new Inspector(osuInstallationPath, targetUsername);
            inspector.Message += OnInspectorMessage;
            inspector.Inspect();
        }

        private void OnLoad(object sender, EventArgs e) {
            SetupOsuInstallationPath();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            installationPath = SetOsuInstallationDirectory();

            if (installationPath != null) {
                isValidInstallationPath = true;
            } else {
                isValidInstallationPath = false;
                MessageBox.Show("Please select valid osu! installation directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConnectIrcButton_Click(object sender, RoutedEventArgs e) {
            if (!isValidInstallationPath) {
                MessageBox.Show("Please select valid osu! installation directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (UsernameTextBox.Text == "" || PasswordBox.Password == "") {
                MessageBox.Show("Username and password is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ConnectToIRC(UsernameTextBox.Text, PasswordBox.Password);
        }

        private void OnIrcConnecting(object sender, EventArgs e) {
            StatusTextBlock.Text = "Connecting...";
        }

        private void OnIrcConnect(object sender, EventArgs e) {
            targetUsername = UsernameTextBox.Text;
            StatusTextBlock.Text = "Connected!";
            ConnectIrcButton.IsEnabled = false;
            StartChatInspector(installationPath, targetUsername);
        }

        private void OnIrcConnectFail(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                StatusTextBlock.Text = "Connection failed!";
                ConnectIrcButton.IsEnabled = true;
            });
        }

        private void OnInspectorMessage(object sender, MessageArgs e) {
            string msg = "/msg " + targetUsername + " " + e.Message;
            bool status = irc.SendMessage(e.Type, targetUsername, e.Message);

            if (status) {
                Console.WriteLine("Send => " + msg);
            } else {
                Console.WriteLine("Fail to send => " + msg);
            }
        }
        
        private void OnClosing(object sender, EventArgs e) {
            if (inspector != null) {
                inspector.StopInspect();
            }

            if (irc != null) {
                StatusTextBlock.Text = "Closing connection...";
                Update();
                irc.Stop();
            }
        }
    }
}
