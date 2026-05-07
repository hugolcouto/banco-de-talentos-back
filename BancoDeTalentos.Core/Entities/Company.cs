namespace BancoDeTalentos.Core.Entities;

public class Company : BaseEntity
{
    public Company() { }

    public Company(
        string name,
        string document,
        string telephone,
        string email,
        string password,
        string about
    ) : base()
    {
        Name = name;
        Document = document;
        Telephone = telephone;
        Email = email;
        Password = password;
        About = about;
        // Jobs = new List<Job>();
    }

    public string Name { get; private set; }
    public string Document { get; private set; }
    public string Telephone { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string About { get; private set; }
    // public List<Job> Jobs { get; private set; } = new List<Job>();

    public void Update(string name, string telephone, string email, string about)
    {
        Name = name;
        Telephone = telephone;
        Email = email;
        About = about;
    }
}
