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
        Job? job = _context
            .Jobs
            .SingleOrDefault(j => j.Id == id);

        return job;
    }

    public List<Job>? GetJobs()
    {
        List<Job>? jobs = _context
            .Jobs
            .ToList();

        return jobs;
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
}
