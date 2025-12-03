using Expence.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure
{
    public class ExpenceDbContext : DbContext
    {
        public ExpenceDbContext(DbContextOptions<ExpenceDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<EmailDomainInfo> EmailDomainInfo { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).IsRequired().HasMaxLength(255);
                b.Property(u => u.PasswordHash).IsRequired();
                b.HasMany(u =>u.Transactions).WithOne(t => t.User).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.TransactionReference).IsRequired();
                b.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
                b.Property(t => t.Category).HasMaxLength(100);
                b.Property(t => t.Description).HasMaxLength(100);
                b.Property(t => t.CreatedAt).IsRequired();
                b.Property(t => t.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
                b.HasIndex(t => new { t.UserId, t.Type, t.TransactionReference});
            });

            modelBuilder.Entity<EmailDomainInfo>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasIndex(e => e.Domain);
            });
        }
    }
}