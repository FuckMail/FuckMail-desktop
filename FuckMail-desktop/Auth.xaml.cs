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
using System.Configuration;

namespace FuckMail_desktop
{
    /// <summary>
    /// Logic for MainWindow.xaml
    /// </summary>
    public partial class Auth : Window
    {
        Config config = new Config(); // Init config class.
        ErrorType error_type = new ErrorType(); // Init error type.

        class ErrorType {
            /// <summary>
            /// Error type
            /// message (string):
            ///     This is error message.
            /// </summary>

            public string message { get; set; }
        }

        class ResponseType {
            /// <summary>
            /// Response type
            /// response (bool):
            ///     This is response from server. Default value: false or true.
            /// </summary>

            public bool response { get; set; }
        }

        public Auth()
        {
            // Check 'data' folder.
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
            else {
                int count_files = Directory.GetFiles("data").Length; // Count files in 'data' folder.
                // In the 'data' folder must be one file.
                if (count_files != 1)
                {
                    Directory.Delete("data", true);
                    Directory.CreateDirectory("data");
                }
                else if (count_files == 1){

                    string[] fileName = Directory.GetFiles(@"data"); // Get all files from data.
                    string data = File.ReadAllText(fileName[0]); // Read main file.
                    string[] metadata = data.Split(':'); // Separation string with ':'.
                    string sessionID = fileName[0].Replace(@"data\", ""); // After separation to set sessionID.

                    // In metadata must be only two values after separation.
                    if (metadata.Length < 2) {
                        Directory.Delete("data", true);
                        Directory.CreateDirectory("data");
                    }
                    else
                    {
                        /// <summary>
                        /// Call CheckAuth function with params
                        /// config.host (string):
                        ///     This is param from Config class. host only return value from App.config file.
                        ///
                        /// metadata[0] (string):
                        ///     This is username param.
                        ///
                        /// metadata[1] (string):
                        ///     This is password param.
                        ///
                        /// sessionID (string):
                        ///     This is session id.
                        ///
                        /// false (bool):
                        ///     This is param for check md5 convert. Default value: false.
                        /// </summary>

                        if (!CheckAuth(config.host, metadata[0], metadata[1], sessionID, false))
                        {
                            Directory.Delete("data", true);
                            Directory.CreateDirectory("data");
                        }
                        else
                        {
                            Hide(); // Hide process is Auth Window.
                            Main main_window = new Main(metadata[0], sessionID); // Init Main Windows.
                            main_window.ShowDialog(); // Call Main Window.
                            Close(); // Close process is Auth Window.
                        }
                    }
                }
            }
            InitializeComponent();
        }

        private void login_btn_Click(object sender, RoutedEventArgs e)
        {
            string username = username_txtBox.Text; // Get username from username TextBox how content.
            string password = password_txtBox.Password; // Get username from password TextBox how content.

            // Check username and password for correct input.
            if (username == "" || password == "")
            {
                MessageBox.Show("Fields don't should be empty!");
            }
            else
            {
                Random rand = new Random(); // Init Random class.
                var newCongiFileName = CreateMD5(rand.Next(10).ToString()); // Call MD5 function for generate name of Config File.

                // Check auth user.
                if (!CheckAuth(config.host, username, password, newCongiFileName, true))
                {
                    if (error_type.message == "")
                    {
                        MessageBox.Show("Incorrect data!");
                    }
                    else {
                        MessageBox.Show(error_type.message);
                    }
                }
                else
                {
                    // Create new Config File.
                    using (FileStream fs = File.Create(String.Format("data/{0}", newCongiFileName)))
                    {
                        byte[] w_username = new UTF8Encoding(true).GetBytes(username); // Write username.
                        byte[] w_password = new UTF8Encoding(true).GetBytes(CreateMD5(password)); // Write password.
                        byte[] w_symbol = new UTF8Encoding(true).GetBytes(":"); // Write symbol which between username and password.
                        fs.Write(w_username, 0, w_username.Length);
                        fs.Write(w_symbol, 0, w_symbol.Length);
                        fs.Write(w_password, 0, w_password.Length);
                    }

                    Hide(); // Hide process is Auth Window.
                    Main main_window = new Main(username, newCongiFileName); // Init Main Windows.
                    main_window.ShowDialog(); // Call Main Window.
                    Close(); // Close process is Auth Window.
                }
            }
        }

        public bool CheckAuth(string host, string username, string password, string sessionid, bool md5) {
            /// <summary>
            /// All get params
            /// host (string):
            ///     This is param from Config class. host only return value from App.config file.
            ///
            /// username (string):
            ///     This is username param.
            ///
            /// password (string):
            ///     This is password param.
            ///
            /// sessionid (string):
            ///     This is session id.
            ///
            /// md5 (bool):
            ///     This is param for check md5 convert. Default value: false.
            ///
            /// return (bool): boolean value from ResponseType. true or false
            /// </summary>

            try
            {
                string hashPass = md5 ? CreateMD5(password) : password; // Check md5.
                var url = String.Format("http://{0}/api/auth/{1}/{2}/{3}", host, username, hashPass, sessionid); // Format correct url
                var request = WebRequest.Create(url); // Create Web Requests.
                request.Method = "GET"; // Set GET method.

                var webResponse = request.GetResponse(); // Get Response.
                var webStream = webResponse.GetResponseStream(); // Get Stream Response.

                var reader = new StreamReader(webStream); // Read stream response.
                var data = reader.ReadToEnd(); // Read end data.

                error_type.message = ""; // Set message for ErrorType of classs.
                ResponseType Response = JsonConvert.DeserializeObject<ResponseType>(data); // Deserialize data from response.
                return Response.response; // Return deserialize response.
            }
            catch (Exception e) {
                error_type.message = e.Message; // Set error message.
                return false; // Return 'false' bool object.
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash.
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string.
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