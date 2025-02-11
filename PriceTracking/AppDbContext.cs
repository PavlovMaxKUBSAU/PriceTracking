using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PriceTracking.Models;

namespace PriceTracking
{
    public class AppDbContext : DbContext
    {
        //public static DbContextOptions<AppDbContext> options_db = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Filename=priceTracking.db").Options;
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=priceTracking.db");
        }

        public AppDbContext()
        {
            Database.EnsureCreated();
        }
    }
}
