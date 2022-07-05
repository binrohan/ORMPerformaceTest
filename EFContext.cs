using EFPerformance.Models;
using EFPerformance.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace EFPerformance
{
    public class EFContext : DbContext
    {
        public EFContext(DbContextOptions option) : base(option) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<StudentViewModel> StudentViewModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentViewModel>().HasNoKey();
        }
    }
}