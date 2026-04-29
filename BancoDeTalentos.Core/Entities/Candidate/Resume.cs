using System;

namespace BancoDeTalentos.Core.Entities;

public class Resume : BaseEntity
{
    public Resume() { }

    public Resume(List<string> scholarity, List<string> courses, List<string> experiences) : base()
    {
        Scholarity = scholarity;
        Courses = courses;
        Experiences = experiences;
    }

    public List<string> Scholarity { get; private set; }
    public List<string> Courses { get; private set; }
    public List<string> Experiences { get; private set; }
}
