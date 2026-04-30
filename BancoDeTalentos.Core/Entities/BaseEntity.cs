using System;

namespace BancoDeTalentos.Core.Entities;

public class BaseEntity
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
