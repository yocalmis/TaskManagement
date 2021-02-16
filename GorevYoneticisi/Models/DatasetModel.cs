using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class DatasetModel
    {
        public List<int> data = new List<int>();
        public string label;
        public string borderColor;
        public bool fill = false;
    }
}