using BancoDeTalentos.API.Extensions;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

[Route("api/empresa")]
[ApiController]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IJobService _jobService;

    public CompaniesController(ICompanyService companyService, IJobService jobService)
    {
        _companyService = companyService;
        _jobService = jobService;
    }

    // create
    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        ResultViewModel<CompanyViewModel> companyResult = _companyService.CreateCompany(model);

        return CreatedAtAction(
            nameof(GetById),
            new { id = companyResult.Data?.Id },
            companyResult
        );
    }

    // read
    [HttpGet]
    public IActionResult Get()
    {
        ResultViewModel<List<CompanyViewModel>> companies = _companyService.GetCompanies();

        return companies.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        ResultViewModel<CompanyViewModel> company = _companyService.GetCompanyById(id);

        return company.ToActionResult(this);
    }

    [HttpGet("{id}/vagas")]
    public IActionResult GetJobsByCompany(int id)
    {
        ResultViewModel<List<JobViewModel>>? result = _jobService.GetJobsByCompanyId(id);
        return result.ToActionResult(this);
    }

    // update
    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateCompanyModel model)
    {
        _companyService.UpdateCompany(id, model);

        return Ok();
    }

    // delete
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _companyService.DeleteCompany(id);

        return NoContent();
    }
}

