using System;
using BancoDeTalentos.Application.Model;

namespace BancoDeTalentos.Application.Interfaces;

public interface IJobService
{
    // Create
    ResultViewModel<JobViewModel> CreateJob(JobViewModel model);

    // Read
    ResultViewModel<List<JobViewModel>> GetJobs();
    ResultViewModel<JobViewModel> GetJobById(int id);

    // Update
    // TODO: Alterar o model para UpdateJobModel
    ResultViewModel UpdateJob(int id, UpdateJobModel model);

    // Delete
    ResultViewModel DeleteJob(int id);
}
