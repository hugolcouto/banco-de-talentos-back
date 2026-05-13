using System;

namespace BancoDeTalentos.Application.Model;

public class CreateCompanyModel
{
    public CreateCompanyModel(string name, string document, string telephone, string email, string password, string about)
    {
        Name = name;
        Document = document;
        Telephone = telephone;
        Email = email;
        Password = password;
        About = about;
    }

    public string Name { get; set; }
    public string Document { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string About { get; set; }
}
