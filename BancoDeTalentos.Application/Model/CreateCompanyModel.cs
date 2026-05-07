using System;

namespace BancoDeTalentos.Application.Model;

public class CreateCompanyModel
{
    public string Name { get; set; }
    public string Document { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string About { get; set; }
}
