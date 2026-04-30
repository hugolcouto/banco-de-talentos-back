using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Core.Interfaces;

public interface ICompanyRepository
{
    int CreateCompany(Company company);
}
