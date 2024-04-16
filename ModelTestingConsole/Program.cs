using DivineInvestorLib;
using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace ModelTestingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Company company = new Company("GazzzProm", 10000, 5);
            //company.LaunchIPO(10000, 5);

            Company company1 = new Company("MicroSold", 1000, 50);
            //company1.LaunchIPO(1000, 50);

            Player investor = new Player();// (1024);


            using (ApplicationContext db = new ApplicationContext())
            {
                db.Players.Add(investor);
                db.Companies.Add(company);

                db.SaveChanges();
            }

            // покупка акций
            using (ApplicationContext db = new ApplicationContext())
            {
                Player player = db.Players
                    .Include(p => p.Account)
                    .FirstOrDefault(player => player.Id == investor.Id);
                Company comp = db.Companies
                    .Include(c => c.Shares)
                    .ThenInclude(s => s.Blocks)
                    .ToList()
                    .FirstOrDefault(comp => comp.Id == company.Id);

                player.BuyShares(comp, 20);

                db.SaveChanges();
            }

            // продажа акций
            using (ApplicationContext db = new ApplicationContext())
            {
                Player player = db.Players
                    .Include(p => p.Account)
                    .FirstOrDefault(player => player.Id == investor.Id);
                Company comp = db.Companies
                    .Include(c => c.Shares)
                    .ThenInclude(s => s.Blocks)
                    .ToList()
                    .FirstOrDefault(comp => comp.Id == company.Id);

                player.SellShares(comp, 20);

                db.SaveChanges();
            }

            // удаление пустых blocks в БД?
            using (ApplicationContext db = new ApplicationContext())
            {
                var blocksOfShares = db.BlockOfShares
                    .Where(b => b.Quantity == 0)
                    .ToList();
                foreach (var block in blocksOfShares)
                {
                    db.BlockOfShares.Remove(block);
                }
                db.SaveChanges();
            }
        }
    }
}
