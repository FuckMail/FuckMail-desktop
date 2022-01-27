using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Newtonsoft.Json;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace FuckMail_desktop
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Main : Window
    {
        Config config = new Config();

        class SomeType
        {
            public string[] addresses { get; set; }
        }

        class AddressDataType {
            public _AddressDataType data { get; set; }
        }

        class _AddressDataType {
            public string address { get; set; }
            public string password { get; set; }
            public string proxy_url { get; set; }
        }

        public Main(string username, string sessionID)
        {
            InitializeComponent();
            getAllAddresses(config.host, username);
        }

        private void getAllAddresses(string host, string username) {
            var url = String.Format("http://{0}/api/addresses/{1}", host, username);
            var request = WebRequest.Create(url);
            request.Method = "GET";

            var webResponse = request.GetResponse();
            var webStream = webResponse.GetResponseStream();

            var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            SomeType Addresses = JsonConvert.DeserializeObject<SomeType>(data);

            foreach(string address in Addresses.addresses)
            {
                Button address_button = new Button();
                Label address_label = new Label();
                address_button.Content = address;
                address_button.Height = 35;
                address_button.Name = username;
                address_button.Click += ClickHandler;
                addressesPanel.Children.Add(address_button);
                addressesPanel.Children.Add(address_label);
            }
        }

        private void getAllMessages(string host, string username, string address) {
            var url = String.Format("http://{0}/api/address_data/{1}/{2}", host, username, address);
            var request = WebRequest.Create(url);
            request.Method = "GET";

            var webResponse = request.GetResponse();
            var webStream = webResponse.GetResponseStream();

            var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            AddressDataType address_data = JsonConvert.DeserializeObject<AddressDataType>(data);
            try
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.outlook.com", 993, true);

                    client.Authenticate(address_data.data.address, address_data.data.password);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    for (int i = 0; i < inbox.Count; i++)
                    {
                        var message = inbox.GetMessage(i);
                        Button message_button = new Button();
                        Label message_label = new Label();
                        message_button.Height = 40;
                        message_button.Content = message.Subject;
                        Console.WriteLine(message.TextBody);
                        messagesPanel.Children.Add(message_button);
                        messagesPanel.Children.Add(message_label);
                    }

                    client.Disconnect(true);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private delegate void starterdelegate(object sender, RoutedEventArgs e);

        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Application.Current.Dispatcher.Invoke(new Action(() => {
                getAllMessages(config.host, btn.Name.ToString(), btn.Content.ToString());
            }));
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            Directory.Delete("data", true);
            this.Hide();
            Auth auth_window = new Auth();
            auth_window.ShowDialog();
            this.Close();
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
