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

    public ResultViewModel<CompanyViewModel> GetCompany(int id)
    {
        throw new NotImplementedException();
    }
}