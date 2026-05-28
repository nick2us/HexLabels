using HexLabels.Api.Core.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HexLabels.Api.Core.Data.Contexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();

            if (!Companies.Any())
            {
                SeedData();
            }
        }

        public DbSet<Company> Companies => Set<Company>();

        public DbSet<User> Users => Set<User>();

        public DbSet<ApiKey> APIKeys => Set<ApiKey>();

        public DbSet<UserRoles> UserRoles => Set<UserRoles>();


        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow; // current datetime

                if (entity.State == EntityState.Added)
                {
                    ((BaseModel)entity.Entity).CreatedAt = now;
                }
                ((BaseModel)entity.Entity).UpdatedAt = now;
            }
        }

        private void SeedData()
        {

            Company c = new()
            {
                ID = new Guid("958d223c-9ae3-4f9a-8989-51498c3ecc23"),
                Name = "Superuser Company"
            };

            User u = new()
            {
                ID = new Guid("af4d7651-0e66-4eb7-808f-c8dde2cfb1c6"),
                Email = "admin@localhost",
            };

            ApiKey key = new()
            {
                Company = c,
                Key = new Guid("b1f5f2e2-3c4d-4e5f-9a6b-7c8d9e0f1a2b"),
            };

            UserRoles.Add(new UserRoles()
            {
                Company = c,
                User = u,
                Role = UserRoleTypes.Employee
            });

            u.Companies.Add(c);
            APIKeys.Add(key);
            Companies.Add(c);
            Users.Add(u);

            SaveChanges();
        }
    }
}
