using System;

namespace BancoDeTalentos.Application.Model;

public class UpdateJobModel
{
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
