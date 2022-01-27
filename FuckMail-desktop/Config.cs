using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
