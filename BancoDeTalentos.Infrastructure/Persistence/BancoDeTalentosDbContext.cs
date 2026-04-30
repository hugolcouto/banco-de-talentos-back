using System;
using System.Data.Common;
using BancoDeTalentos.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BancoDeTalentos.Infrastructure.Persistence;

public class BancoDeTalentosDbContext : DbContext
{
    public BancoDeTalentosDbContext(DbContextOptions<BancoDeTalentosDbContext> options)
        : base(options) { }

    // Company
    public DbSet<Company> Company { get; set; }
    public DbSet<Job> Jobs { get; set; }

    // Candidate
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Resume> Resumes { get; set; }

    // Backoffice
    public DbSet<Backoffice> Backoffice { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(
            e => e.HasKey(c => c.Id)
        );

        base.OnModelCreating(modelBuilder);
    }
}
