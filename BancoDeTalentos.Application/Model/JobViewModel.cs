using System;
using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Application.Model;

public class JobViewModel
{
    public JobViewModel(
        string title,
        string description,
        string benefits,
        string requirements,
        string optionalRequirements,
        string address,
        string modality,
        decimal salary,
        DateTime dueDate,
        int openedVacancies
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
        DueDate = dueDate;
        OpenedVacancies = openedVacancies;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public string Benefits { get; set; }
    public string Requirements { get; set; }
    public string OptionalRequirements { get; set; }
    public string Address { get; set; }
    public string Modality { get; set; }
    public decimal Salary { get; set; }
    public DateTime DueDate { get; set; }
    public int OpenedVacancies { get; set; }

    public static JobViewModel? FromEntity(Job entity)
        => entity is null
            ? null
            : new JobViewModel(
                entity.Title,
                entity.Description,
                entity.Benefits,
                entity.Requirements,
                entity.OptionalRequirements,
                entity.Address,
                entity.Modality,
                entity.Salary,
                entity.DueDate,
                entity.OpenedVacancies
            );
}
