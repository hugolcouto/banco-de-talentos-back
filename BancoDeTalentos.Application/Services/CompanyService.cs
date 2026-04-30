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

        if (company is null) ResultViewModel.Error("Empresa não encontrada");

        return ResultViewModel<CompanyViewModel>.Success(
            CompanyViewModel.FromEntity(company)!
        );
    }
}