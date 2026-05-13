using System;

namespace BancoDeTalentos.Application.Model;

public class CreateJobModel
{
    public CreateJobModel(string title, string description, string benefits, string requirements, string optionalRequirements, string address, string modality, decimal salary, bool showSalary, DateTime dueDate, int openedVacancies, int companyId)
    {
        Title = title;
        Description = description;
        Benefits = benefits;
        Requirements = requirements;
        OptionalRequirements = optionalRequirements;
        Address = address;
        Modality = modality;
        Salary = salary;
        ShowSalary = showSalary;
        DueDate = dueDate;
        OpenedVacancies = openedVacancies;
        CompanyId = companyId;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public string Benefits { get; set; }
    public string Requirements { get; set; }
    public string OptionalRequirements { get; set; }
    public string Address { get; set; }
    public string Modality { get; set; }
    public decimal Salary { get; set; }
    public bool ShowSalary { get; set; }
    public DateTime DueDate { get; set; }
    public int OpenedVacancies { get; set; }
    public int CompanyId { get; set; }

}
