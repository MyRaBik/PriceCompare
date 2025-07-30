using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { } // Пустой конструктор для работы без DI

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Определяем таблицы
        public DbSet<Request> Request { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PriceHistory> PriceHistory { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Загружаем настройки подключения из appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // Важно!
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseNpgsql(connectionString); // Требует using Microsoft.EntityFrameworkCore;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Уникальность запроса
            modelBuilder.Entity<Request>()
                .HasIndex(r => r.Query)
                .IsUnique();

            // Связь Request -> Products (один ко многим)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Request)
                .WithMany()
                .HasForeignKey(p => p.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь Request -> Subscriptions (один ко многим)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Request)
                .WithMany()
                .HasForeignKey(s => s.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь User -> Subscriptions (один ко многим)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Уникальность email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Ограничения на длину
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(20);

            base.OnModelCreating(modelBuilder);
        }
    }
}
