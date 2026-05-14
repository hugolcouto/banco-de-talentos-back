using System;
using System.Net;
using System.Runtime.ConstrainedExecution;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;

namespace BancoDeTalentos.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
        => _jobRepository = jobRepository;

    // Create
    public ResultViewModel<JobViewModel> CreateJob(CreateJobModel model)
    {
        Job job = new Job(
            model.Title,
            model.Description,
            model.Benefits,
            model.Requirements,
            model.OptionalRequirements,
            model.Address,
            model.Modality,
            model.Salary,
            model.DueDate,
            model.OpenedVacancies,
            model.CompanyId
        );

        _jobRepository.CreateJob(job);

        JobViewModel? viewModel = JobViewModel.FromEntity(job);

        return ResultViewModel<JobViewModel>.Success(viewModel!);
    }

    // Read
    public ResultViewModel<JobViewModel> GetJobById(int id)
    {
        Job? job = _jobRepository.GetJobById(id);

        if (job is null) return ResultViewModel<JobViewModel>
            .Error(
                "Vaga não encontrada",
                HttpStatusCode.NotFound,
                null
            );

        return ResultViewModel<JobViewModel>.Success(
            JobViewModel.FromEntity(job)!
        );
    }

    public ResultViewModel<List<JobViewModel>> GetJobs()
    {
        List<Job> jobs = _jobRepository.GetJobs()!;

        List<JobViewModel>? model = jobs!.Select(
            JobViewModel.FromEntity
        ).ToList()!;

        return ResultViewModel<List<JobViewModel>>.Success(model);
    }

    // TODO: Implementar get de jobs por CompanyId
    // public ResultViewModel<JobViewModel> GetJobsByCompany(int CompanyId)
    // {
    //     return ;
    // }

    // Update
    public ResultViewModel UpdateJob(int id, UpdateJobModel model)
    {
        Job? job = _jobRepository.GetJobById(id);

        if (job is null) return ResultViewModel<JobViewModel>
            .Error(
                "Vaga não encontrada",
                HttpStatusCode.NotFound,
                null
            );

        job.Update(
            model.Title,
            model.Description,
            model.Benefits,
            model.Requirements,
            model.OptionalRequirements,
            model.Address,
            model.Modality,
            model.Salary,
            model.ShowSalary,
            model.DueDate,
            model.OpenedVacancies,
            model.CompanyId
        );

        _jobRepository.UpdateJob(job);

        return ResultViewModel.Sucess();

    }

    // Delete
    public ResultViewModel DeleteJob(int id)
    {
        Job? job = _jobRepository.GetJobById(id);

        if (job is null) return ResultViewModel<JobViewModel>
            .Error(
                "Vaga não encontrada",
                HttpStatusCode.NotFound
            );

        job.SetAsDeleted();
        _jobRepository.DeleteJob(job);

        return ResultViewModel.Sucess();
    }

    public ResultViewModel<List<JobViewModel>> GetJobsByCompanyId(int companyId)
    {
        // Correção: nome do método e mapeamento
        List<Job>? jobs = _jobRepository.GetJobsByCompanyid(companyId);

        if (jobs == null || !jobs.Any())
            return ResultViewModel<List<JobViewModel>>.Success(new List<JobViewModel>());

        List<JobViewModel> viewModels = jobs
            .Select(JobViewModel.FromEntity)
            .ToList()!;

        return ResultViewModel<List<JobViewModel>>.Success(viewModels);
    }
}
