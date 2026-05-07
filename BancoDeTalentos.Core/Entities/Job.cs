namespace BancoDeTalentos.Core.Entities;

public class Job : BaseEntity
{
    public Job() { }

    public Job(
        string title,
        string description,
        string benefits,
        string requirements,
        string optionalRequirements,
        string address,
        string modality,
        decimal salary,
        DateTime dueDate,
        int openedVacancies,
        int companyId
    ) : base()
    {
        Title = title;
        Description = description;
        Benefits = benefits;
        Requirements = requirements;
        OptionalRequirements = optionalRequirements;
        Address = address;
        Modality = modality;
        Salary = salary;
        ShowSalary = true;
        DueDate = dueDate;
        OpenedVacancies = openedVacancies;
        CompanyId = companyId;
    }

    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Benefits { get; private set; }
    public string Requirements { get; private set; }
    public string OptionalRequirements { get; private set; }
    public string Address { get; private set; }
    public string Modality { get; private set; }
    public decimal Salary { get; private set; }
    public bool ShowSalary { get; private set; }
    public DateTime DueDate { get; private set; }
    public int OpenedVacancies { get; private set; }
    public int CompanyId { get; private set; }

    public void Update(
        string title,
        string description,
        string benefits,
        string requirements,
        string optionalRequirements,
        string address,
        string modality,
        decimal salary,
        bool showSalary,
        DateTime dueDate,
        int openedVacancies,
        int companyId
    )
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
}
