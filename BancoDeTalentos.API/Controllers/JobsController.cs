using BancoDeTalentos.API.Extensions;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

[Route("api/vagas")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    // create
    [HttpPost]
    public IActionResult Create(CreateJobModel model)
    {
        ResultViewModel<JobViewModel> jobResult = _jobService.CreateJob(model);

        return CreatedAtAction(
            nameof(GetById),
            new { id = jobResult.Data?.Id },
            jobResult
        );
    }

    // read
    [HttpGet]
    public IActionResult Get()
    {
        ResultViewModel<List<JobViewModel>> jobs = _jobService.GetJobs();
        return jobs.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        ResultViewModel<JobViewModel> job = _jobService.GetJobById(id);

        return job.ToActionResult(this);
    }

    // update
    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateJobModel model)
    {
        _jobService.UpdateJob(id, model);
        return Ok();
    }

    // delete
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _jobService.DeleteJob(id);
        return NoContent();
    }
}
