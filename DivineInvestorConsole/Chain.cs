using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivineInvestorConsole
{
    public class Chain
    {
        public string CompanyName { get; set; }
        public string CompanyInfo { get; set; }
        public string Action { get; set; }
        public string Value { get; set; }

        public void Clear()
        {
            CompanyName = string.Empty;
            Action = string.Empty;
            Value = string.Empty;
        }

        //?
        public override string ToString()
        {
            StringBuilder result = new StringBuilder()
            .Append(CompanyName + " ")
            .Append(Action + " ")
            .Append(Value);
            return result.ToString();
        }
    }
}
