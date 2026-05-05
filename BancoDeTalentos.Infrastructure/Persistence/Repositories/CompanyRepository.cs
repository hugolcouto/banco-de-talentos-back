using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;
using Microsoft.Identity.Client;

namespace BancoDeTalentos.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public CompanyRepository(BancoDeTalentosDbContext context)
     => _context = context;

    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();

        return company.Id;
    }

    public List<Company>? GetCompanies()
    {
        List<Company> companies = _context
            .Company
            .Where(c => !c.IsDeleted)
            .ToList();

        return companies;
    }

    public Company GetCompanyById(int id)
    {
        Company? company = _context
            .Company
            .FirstOrDefault(c => c.Id == id && !c.IsDeleted)!;

        return company;
    }

    public void UpdateCompany(Company company)
    {
        _context.Company.Update(company);
        _context.SaveChanges();
    }

    public void DeleteCompany(Company company)
    {
        _context.Company.Update(company);
        _context.SaveChanges();
    }
}
