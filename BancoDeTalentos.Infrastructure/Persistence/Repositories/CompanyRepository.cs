using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;

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
        Company? company = _context.Company.FirstOrDefault(c => c.Id == id)!;

        return company;
    }
}
