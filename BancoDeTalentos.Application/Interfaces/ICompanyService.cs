using BancoDeTalentos.Application.Model;
namespace BancoDeTalentos.Application.Interfaces;

public interface ICompanyService
{
    // Create
    ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model);

    // Read
    ResultViewModel<List<CompanyViewModel>> GetCompanies();
    ResultViewModel<CompanyViewModel> GetCompanyById(int id);

    // Update
    ResultViewModel UpdateCompany(int id, UpdateCompanyModel model);

    // Delete
    ResultViewModel DeleteCompany(int id);

}
