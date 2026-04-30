using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Core.Interfaces;

public interface ICompanyRepository
{
    // Create
    int CreateCompany(Company company);

    // Read
    List<Company>? GetCompanies();
    Company GetCompanyById(int id);

    // Update
    void UpdateCompany(Company company);

    // Delete
    void DeleteCompany(Company company);
}
