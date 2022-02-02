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
    /// Logic for Window1.xaml
    /// </summary>
    public partial class Main : Window
    {
        Config config = new Config(); // Init Config class

        class SomeType
        {
            /// <summary>
            /// addresses (string[]):
            ///     This is param equal all got addresses from user.
            /// </summary>

            public string[] addresses { get; set; }
        }

        class AddressDataType {
            /// <summary>
            /// data (WrappedAddressDataType):
            ///     This is response from server.
            /// </summary>

            public WrappedAddressDataType data { get; set; }
        }

        class WrappedAddressDataType {
            /// <summary>
            /// address (string):
            ///     This is user mail address.
            /// password (string):
            ///     This is user mail password.
            /// proxy_url (string)
            ///     This is user mail proxy url.
            /// </summary>

            public string address { get; set; }
            public string password { get; set; }
            public string proxy_url { get; set; }
        }

        class AddressState {
            public string address;
        }

        AddressState address_state = new AddressState(); // Init address state

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
                Button address_button = new Button{ Content=address, Height=35, Name=username, Cursor=Cursors.Hand};
                Label address_label = new Label();
                address_button.Click += ClickHandler;
                addressesPanel.Children.Add(address_button);
                addressesPanel.Children.Add(address_label);
            }
        }

        private void getAllMessages(string host, string username, string address) {
            address_state.address = address;
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
                        Button message_button = new Button { DataContext = String.Format("{0}user{1}", username, message.MessageId),
                            Height=40, Width=355, Content= message.Subject};
                        Label message_label = new Label();
                        message_button.Click += ShowBodyContent;
                        messagesTree.Items.Add(message_button);
                        messagesTree.Items.Add(message_label);
                    }

                    client.Disconnect(true);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Application.Current.Dispatcher.Invoke(new Action(() => {
                getAllMessages(config.host, btn.Name.ToString(), btn.Content.ToString());
            }));
        }

        private void ShowBodyContent(object sender, RoutedEventArgs e) {
            var message_button = sender as Button;
            string message_id = message_button.DataContext.ToString();
            htmlPage.Visibility = Visibility;
            htmlPage.Navigate(String.Format("http://{0}/api/show_message/{1}", config.host, String.Format("{0}address{1}", message_id, address_state.address)));
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
