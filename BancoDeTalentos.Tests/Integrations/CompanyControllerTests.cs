using System.Net;
using System.Net.Http.Json;
using BancoDeTalentos.Application.Model;
using Bogus;
using Bogus.Extensions.Brazil;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BancoDeTalentos.Tests.Integrations;

public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Faker _faker;

    public CompanyControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task Create_Company()
    {
        // --- ARRANGE ---
        var newCompanyObject = new
        {
            name = _faker.Company.CompanyName(),
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash()
        };

        // --- ASSERT ---
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/empresa", newCompanyObject);

        // --- ACTION ---
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ResultViewModel<CompanyViewModel?>? result = await response.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(newCompanyObject.name, result.Data!.Name);
        Assert.True(result.Data!.Id > 0);
    }
}
