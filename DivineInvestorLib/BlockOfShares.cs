using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivineInvestorLib
{
    //пакет акций у инвестора
    public class BlockOfShares
    {
        public int Id { get; set; }
        public Company Company { get; set; }
        public Player Owner { get; set; }
        public int Quantity { get; set; }
        //public double Price { get; set; }

        public double OwnedAmount { get; set; }
        public double CurrentAmount { get; set; }
        private double AmountDiff { get; set; }
        public double AmountDiffPercent { get; set; }


        public BlockOfShares() { }

        public BlockOfShares(Company company, Player owner, int quantity)
        {
            Company = company;
            Owner = owner;
            Quantity = quantity;
            //CalcPrice(company.Shares.PriceOne);
        }

        public void CalcCurrentAmount(double priceOne)
        {
            CurrentAmount = Quantity * priceOne;
        }

        public void AddOwnedAmount(double newAmount)
        {
            OwnedAmount += newAmount;
        }

        // как обработать отрицательный случай?
        public void RemoveOwnedAmount(double newAmount)
        {
            OwnedAmount -= newAmount;
        }

        public void CalcDiffPercent()
        {
            //?
            if (OwnedAmount > 0)
            {
                AmountDiff = CurrentAmount - OwnedAmount;
                AmountDiffPercent = (AmountDiff / OwnedAmount) * 100;
            }
        }

        //public void CalcPrice(double PriceOne)
        //{
        //    Price = Quantity * PriceOne;
        //}
    }
}
