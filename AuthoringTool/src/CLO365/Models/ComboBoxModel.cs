using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLO365.Models
{
    public class ComboBoxModel
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }

    public class ProductionComparisonModel
    {
        public string Tag { get; set; }
        public bool IsAcceptedorRejected { get; set; }
    }
}
