using IEMS.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace IEMS.Data
{
    public class IEMSDbContext : DbContext
    {
        public IEMSDbContext(DbContextOptions<IEMSDbContext> DbContext) : base(DbContext) { }

        public DbSet<Income> Income { get; set; }

        public DbSet<Expense> Expense { get; set; }
    }
}
