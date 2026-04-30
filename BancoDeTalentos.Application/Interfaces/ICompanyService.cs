using BancoDeTalentos.Application.Model;
namespace BancoDeTalentos.Application.Interfaces;

public interface ICompanyService
{
    ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model);
    ResultViewModel<CompanyViewModel> GetCompany(int id);
}
