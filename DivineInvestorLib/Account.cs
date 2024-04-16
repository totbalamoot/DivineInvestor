using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivineInvestorLib
{
    public class Account
    {
        public int Id { get; set; }
        public double Capital { get; private set; }
        public long PlayerId { get; set; }


        public Account()
        {
            Capital = 0;
        }

        public Account(double capital)
        {
            Capital = capital;
        }

        public void Put(double amount)
        {
            if (amount > 0)
            {
                Capital += amount;
            }
        }

        public double Take(double amount)
        {
            if (IsEnoughMoney(amount))
            {
                Capital -= amount;
                return amount;
            }
            else
            {
                return 0;
            }
        }

        public double Take()
        {
            return Take(Capital);
        }

        public void SendMoneyTo(Account account, double amount)
        {
            if (IsEnoughMoney(amount))
            {
                account.Put(Take(amount));
            }
        }

        public bool IsEnoughMoney(double amount)
        {
            if (Capital >= amount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
