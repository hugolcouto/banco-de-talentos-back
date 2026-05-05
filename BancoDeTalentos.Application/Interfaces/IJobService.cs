using System;
using BancoDeTalentos.Application.Model;

namespace BancoDeTalentos.Application.Interfaces;

public interface IJobService
{
    // Create
    ResultViewModel<JobViewModel> CreateJob(JobViewModel model);

    // Read
    ResultViewModel<JobViewModel> GetJobs();
    ResultViewModel<JobViewModel> GetJobById(int id);

    // Update
    // TODO: Alterar o model para UpdateJobModel
    ResultViewModel<JobViewModel> UpdateJob(int id, CreateJobModel model);

    // Delete
    ResultViewModel<JobViewModel> DeleteJob(int id);
}
