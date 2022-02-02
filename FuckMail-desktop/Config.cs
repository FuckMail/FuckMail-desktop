using System;
using System.Configuration;

namespace FuckMail_desktop
{
    public class Config
    {
        public string host {
            get {
                return ConfigurationManager.AppSettings.Get("host");
            }
        }
    }
}
