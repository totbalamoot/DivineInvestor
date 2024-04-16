using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DivineInvestorLib
{
    public class Shares
    {
        private int maxBaseChangePricePercent = 5;

        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public List<BlockOfShares> Blocks { get; set; }
        public int Quantity { get; set; }
        //public int FreeQuantity { get; set; }
        public double PriceOne { get; set; }
        public double PrevPriceOne { get; set; }
        public double PriceDiffPercent { get; set; }

        public double PriceAll { get; set; }


        public Shares() { }

        // эмиссия акций находится в конструкторе
        public Shares(Company company, int quanity, double priceOne)
        {
            Company = company;
            Quantity = quanity;
            PriceOne = priceOne;
            Blocks = new List<BlockOfShares>();
            //CalcFullPrice();
        }

        public BlockOfShares FindBlock(Player owner)
        {
            return Blocks.FirstOrDefault(block => block.Owner == owner);
        }

        public bool Buy(Player buyer, int quantity)
        {
            if (Quantity >= quantity)
            {
                double purchaseAmount = quantity * PriceOne;
                if (buyer.Account.IsEnoughMoney(purchaseAmount))
                {
                    BlockOfShares findBlock = FindBlock(buyer);
                    if (findBlock == null)
                    {
                        buyer.Account.Take(purchaseAmount);

                        BlockOfShares newBlock = new BlockOfShares(Company, buyer, quantity);
                        Blocks.Add(newBlock);
                        buyer.Blocks.Add(newBlock);

                        Quantity -= quantity;

                        //newBlock.CalcPrice(PriceOne);
                        newBlock.CalcCurrentAmount(PriceOne);
                        newBlock.AddOwnedAmount(purchaseAmount);

                        return true;
                    }
                    else
                    {
                        buyer.Account.Take(purchaseAmount);

                        findBlock.Quantity += quantity;

                        Quantity -= quantity;

                        //findBlock.CalcPrice(PriceOne);
                        findBlock.CalcCurrentAmount(PriceOne);
                        findBlock.AddOwnedAmount(purchaseAmount);

                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Sell(Player seller, int quantity)
        {
            BlockOfShares findBlock = FindBlock(seller);
            if (findBlock != null)
            {
                double sellAmount = quantity * PriceOne;
                if (findBlock.Quantity > quantity)
                {
                    seller.Account.Put(sellAmount);
                    findBlock.Quantity -= quantity;
                    Quantity += quantity;

                    findBlock.CalcCurrentAmount(PriceOne);
                    findBlock.RemoveOwnedAmount(sellAmount);

                    return true;
                }
                else if (findBlock.Quantity == quantity)
                {
                    seller.Account.Put(sellAmount);
                    findBlock.Quantity -= quantity;
                    Quantity += quantity;

                    //BlockOfShares sellerBlock = seller.Blocks.Find(block => block.Company.Shares == this);
                    //seller.Blocks.Remove(sellerBlock);
                    //Blocks.Remove(findBlock);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //public void CalcFullPrice()
        //{
        //    PriceAll = PriceOne * Quantity;
        //}

        public void ChangePrice()
        {
            Random rnd = new Random();

            Thread.Sleep(10);
            int typeChanging = rnd.Next(2);
            Thread.Sleep(10);
            int changingPercent = rnd.Next(maxBaseChangePricePercent + 1);

            double changingValue = (PriceOne / 100) * changingPercent;

            double newPrice;
            if (typeChanging == 0)
            {
                newPrice = PriceOne - changingValue;
            }
            else
            {
                newPrice = PriceOne + changingValue;
            }

            PrevPriceOne = PriceOne;
            PriceOne = newPrice;
            CalcDiffPercent();

            foreach (var block in Blocks)
            {
                //block.CalcPrice(PriceOne);
                block.CalcCurrentAmount(PriceOne);
            }
        }

        private void CalcDiffPercent()
        {
            PriceDiffPercent = ((PriceOne - PrevPriceOne) / PrevPriceOne) * 100;
        }

        public void CalcDiff()
        {
            foreach (var block in Blocks)
            {
                block.CalcDiffPercent();
            }
        }
    }
}
