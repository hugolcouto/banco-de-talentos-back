using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Core.Interfaces;

public interface IJobRepository
{
    // Create
    int CreateJob(Job job);

    // Read
    List<Job>? GetJobs();
    Job? GetJobById(int id);
    List<Job>? GetJobsByCompanyid(int companyId);

    // Update
    void UpdateJob(Job job);

    // Delete
    void DeleteJob(Job job);
}
