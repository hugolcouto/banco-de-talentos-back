using System;

namespace BancoDeTalentos.Core.ValueObjects;

public record Address(
    string CEP,
    string Street,
    string District,
    string City,
    string State,
    string? County = "Brasil",
    string? Number = "S/N"
)
{
    public string GetFullAddress()
        => $"{Street}, {Number}, {District}, {City} - {State}. CEP {CEP}";
}
