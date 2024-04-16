using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivineInvestorLib
{
    public class Player
    {
        public static double startCapital = 1000;

        public long Id { get; set; }
        public List<BlockOfShares> Blocks { get; set; }
        public Account Account { get; set; }

        public Player()
        {
            Account = new Account(startCapital);
            Blocks = new List<BlockOfShares>();
        }

        public Player(long chatId)
        {
            Id = chatId;
            Account = new Account(startCapital);
            Blocks = new List<BlockOfShares>();
        }

        public bool BuyShares(Company company, int quantity)
        {
            return company.Shares.Buy(this, quantity);
        }

        public bool SellShares(Company company, int quantity)
        {
            return company.Shares.Sell(this, quantity);
        }
    }
}
