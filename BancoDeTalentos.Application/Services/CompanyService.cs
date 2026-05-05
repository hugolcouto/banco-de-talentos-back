using System.Net;
using BancoDeTalentos.Application.Exceptions;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;
namespace BancoDeTalentos.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        Company company = new Company(
            model.Name,
            model.Document,
            model.Telephone,
            model.Email,
            model.Password
        );

        _companyRepository.CreateCompany(company);

        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }

    public ResultViewModel<List<CompanyViewModel>> GetCompanies()
    {
        List<Company>? companies = _companyRepository.GetCompanies();

        List<CompanyViewModel>? model = companies!.Select(
            CompanyViewModel.FromEntity
        ).ToList()!;

        return ResultViewModel<List<CompanyViewModel>>.Success(model);
    }

    public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
    {
        Company? company = _companyRepository.GetCompanyById(id);

        if (company is null) return ResultViewModel<CompanyViewModel>
            .Error(
                "Empresa não encontrada",
                HttpStatusCode.NotFound,
                null
            );

        return ResultViewModel<CompanyViewModel>.Success(
            CompanyViewModel.FromEntity(company)!
        );
    }

    public ResultViewModel UpdateCompany(int id, UpdateCompanyModel model)
    {
        Company? company = _companyRepository.GetCompanyById(id);

        if (company is null) return ResultViewModel<CompanyViewModel>
            .Error(
                "Empresa não encontrada",
                HttpStatusCode.NotFound,
                null
            );

        _companyRepository.UpdateCompany(company!);

        return ResultViewModel.Sucess();
    }

    public ResultViewModel DeleteCompany(int id)
    {
        Company? company = _companyRepository.GetCompanyById(id);

        if (company is null) return ResultViewModel.Error(
            "Empresa não encontrada",
            HttpStatusCode.NotFound
        );

        company!.SetAsDeleted();

        _companyRepository.DeleteCompany(company);

        return ResultViewModel.Sucess();
    }
}