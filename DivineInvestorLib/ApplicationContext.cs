using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DivineInvestorLib
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<BlockOfShares> BlockOfShares => Set<BlockOfShares>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Shares> Shares => Set<Shares>();


        public ApplicationContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=divine_investor.db");
        }
    }
}
