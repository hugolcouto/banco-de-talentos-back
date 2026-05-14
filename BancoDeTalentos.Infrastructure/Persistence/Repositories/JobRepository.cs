using BancoDeTalentos.Core.Interfaces;
using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public JobRepository(BancoDeTalentosDbContext context)
        => _context = context;

    // Create
    public int CreateJob(Job job)
    {
        _context.Jobs.Add(job);
        _context.SaveChanges();

        return job.Id;
    }

    // Read
    public Job? GetJobById(int id)
    {
        return _context
            .Jobs
            .SingleOrDefault(j => j.Id == id && !j.IsDeleted);
    }

    public List<Job>? GetJobs()
    {
        return _context
            .Jobs
            .Where(j => !j.IsDeleted)
            .ToList();
    }

    // Update
    public void UpdateJob(Job job)
    {
        _context.Jobs.Update(job);
        _context.SaveChanges();
    }

    // Delete
    public void DeleteJob(Job job)
    {
        _context.Jobs.Update(job);
        _context.SaveChanges();
    }

    public List<Job>? GetJobsByCompanyid(int companyId)
    {
        return _context.Jobs
            .Where(j => j.CompanyId == companyId)
            .ToList();
    }
}
