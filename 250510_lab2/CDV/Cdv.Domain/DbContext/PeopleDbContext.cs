using Cdv.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cdv.Domain.DbContext;

public class PeopleDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public PeopleDbContext(DbContextOptions<PeopleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigurePersonEntity(modelBuilder.Entity<PersonEntity>());
    }

    private void ConfigurePersonEntity(EntityTypeBuilder<PersonEntity> entity)
    {
        entity.ToTable("Person");

        entity.Property(e => e.FirstName).HasMaxLength(250);
        entity.Property(e => e.LastName).HasMaxLength(250);
    }

    public DbSet<PersonEntity> People { get; set; }
}