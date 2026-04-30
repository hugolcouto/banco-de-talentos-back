using System.Reflection.Metadata;
using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Application.Model;

public class CompanyViewModel
{
    public CompanyViewModel(int id, string name, string telephone, string email, string about)
    {
        Id = id;
        Name = name;
        Telephone = telephone;
        Email = email;
        About = about;
    }

    public int Id { get; set; }
    public string Name { get; private set; }
    public string Telephone { get; private set; }
    public string Email { get; private set; }
    public string About { get; private set; }
    // public List<Job> Jobs { get; private set; } = new List<Job>();

    public static CompanyViewModel? FromEntity(Company? entity)
        => new(
            entity!.Id,
            entity.Name,
            entity.Telephone,
            entity.Email,
            entity.About
        );
}
