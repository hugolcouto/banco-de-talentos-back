using System;

namespace BancoDeTalentos.Core.Entities;

public class BaseEntity
{
    public Guid Guid { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
