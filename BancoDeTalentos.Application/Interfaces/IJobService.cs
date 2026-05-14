using System;
using BancoDeTalentos.Application.Model;

namespace BancoDeTalentos.Application.Interfaces;

public interface IJobService
{
    // Create
    ResultViewModel<JobViewModel> CreateJob(CreateJobModel model);

    // Read
    ResultViewModel<List<JobViewModel>> GetJobs();
    ResultViewModel<JobViewModel> GetJobById(int id);
    ResultViewModel<List<JobViewModel>> GetJobsByCompanyId(int companyId);

    // Update
    // TODO: Alterar o model para UpdateJobModel
    ResultViewModel UpdateJob(int id, UpdateJobModel model);

    // Delete
    ResultViewModel DeleteJob(int id);
}
