using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;

namespace FuckMail_desktop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Auth : Window
    {
        class SomeType {
            public bool response { get; set; }
        }

        public Auth()
        {
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
                InitializeComponent();
            }
            else {
                int count_files = Directory.GetFiles("data").Length;
                if (count_files != 1)
                {
                    Directory.Delete("data", true);
                    Directory.CreateDirectory("data");
                }
                else if (count_files == 1){

                    string[] fileName = Directory.GetFiles(@"data");
                    string data = File.ReadAllText(fileName[0]);
                    string[] metadata = data.Split(':');
                    
                    if (metadata.Length < 2) {
                        Directory.Delete("data", true);
                        Directory.CreateDirectory("data");
                    }
                    else
                    {
                        if (!CheckAuth(metadata[0], metadata[1], false))
                        {
                            Directory.Delete("data", true);
                            Directory.CreateDirectory("data");
                        }
                        else
                        {
                            string sessionID = fileName[0].Replace(@"data\", "");
                            Hide();
                            Main main_window = new Main(metadata[0], sessionID);
                            main_window.ShowDialog();
                            Close();
                        }
                    }
                }
            }
        }

        private void login_btn_Click(object sender, RoutedEventArgs e)
        {
            string username = username_txtBox.Text;
            string password = password_txtBox.Password;

            if (username == "" || password == "")
            {
                MessageBox.Show("Fields don't should be empty!");
            }
            else
            {
                if (!CheckAuth(username, password, true))
                {
                    MessageBox.Show("Incorrect data!");
                }
                else
                {
                    Random rand = new Random();
                    var newCongiFileName = CreateMD5(rand.Next(10).ToString());

                    using (FileStream fs = File.Create(String.Format("data/{0}", newCongiFileName)))
                    {
                        byte[] w_username = new UTF8Encoding(true).GetBytes(username);
                        byte[] w_password = new UTF8Encoding(true).GetBytes(CreateMD5(password));
                        byte[] w_symbol = new UTF8Encoding(true).GetBytes(":");
                        fs.Write(w_username, 0, w_username.Length);
                        fs.Write(w_symbol, 0, w_symbol.Length);
                        fs.Write(w_password, 0, w_password.Length);
                    }

                    Hide();
                    Main main_window = new Main(username, newCongiFileName);
                    main_window.ShowDialog();
                    Close();
                }
            }
        }

        public bool CheckAuth(string username, string password, bool md5) {
            string hashPass = md5 ? CreateMD5(password) : password;
            var url = String.Format("http://127.0.0.1:8000/api/auth/{0}/{1}", username, hashPass);
            var request = WebRequest.Create(url);
            request.Method = "GET";

            var webResponse = request.GetResponse();
            var webStream = webResponse.GetResponseStream();

            var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            SomeType Response = JsonConvert.DeserializeObject<SomeType>(data);
            return Response.response;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}