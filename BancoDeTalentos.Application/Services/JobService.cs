using System;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;

namespace BancoDeTalentos.Application.Services;

public class JobService : IJobService
{
    // Create
    public ResultViewModel<JobViewModel> CreateJob(JobViewModel model)
    {
        throw new NotImplementedException();
    }

    // Read
    public ResultViewModel<JobViewModel> GetJobById(int id)
    {
        throw new NotImplementedException();
    }

    public ResultViewModel<JobViewModel> GetJobs()
    {
        throw new NotImplementedException();
    }

    // Update
    public ResultViewModel<JobViewModel> UpdateJob(int id, CreateJobModel model)
    {
        throw new NotImplementedException();
    }

    // Delete
    public ResultViewModel<JobViewModel> DeleteJob(int id)
    {
        throw new NotImplementedException();
    }


}
