using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class UserMailModel
    {
        public string tomail { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
    }
}