using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivineInvestorLib
{
    public enum Industry
    {
        Building,
        Agricultural,
        Mining
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Shares Shares { get; set; }
        public string Info { get; set; }
        //public string IndustryType { get; set; }


        public Company() { }

        public Company(string name, int quantity, double priceOne)
        {
            Name = name;
            Shares = new Shares(this, quantity, priceOne);
        }

        //public void LaunchIPO(int quanity, double priceOne)
        //{
        //    Shares = new Shares(this, quanity, priceOne);
        //}
    }
}
