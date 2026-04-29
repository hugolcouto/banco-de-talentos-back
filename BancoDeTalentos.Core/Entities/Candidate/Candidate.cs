using System;

namespace BancoDeTalentos.Core.Entities;

public class Candidate : BaseEntity
{
    public Candidate() { }

    public Candidate(
        string fullName,
        DateTime birthdate,
        string address,
        string description,
        string phoneNumber,
        string email,
        string document
    ) : base()
    {
        FullName = fullName;
        Birthdate = birthdate;
        Address = address;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        Document = document;
        Resume = null;
    }

    public string FullName { get; private set; }
    public DateTime Birthdate { get; private set; }
    public string Address { get; private set; }
    public string Description { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public string Document { get; private set; }
    public Resume? Resume { get; private set; }
}
