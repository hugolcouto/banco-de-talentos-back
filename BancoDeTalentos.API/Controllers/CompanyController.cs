using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

[Route("api/empresa")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
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

        return Ok(companies);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        ResultViewModel<CompanyViewModel> company = _companyService.GetCompanyById(id);

        return Ok(company);
    }

    // update
    [HttpPatch("{id}")]
    public IActionResult Update(int id)
    {
        return Ok();
    }

    // delete
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return Ok();
    }
}

