using BancoDeTalentos.Application.Model;
namespace BancoDeTalentos.Application.Interfaces;

public interface ICompanyService
{
    ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model);
    ResultViewModel<List<CompanyViewModel>> GetCompanies();
    ResultViewModel<CompanyViewModel> GetCompanyById(int id);
}
