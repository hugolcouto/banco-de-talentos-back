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

    public CompanyControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public static IEnumerable<object[]> GetCompanyTestData()
    {
        Faker faker = new Faker("pt_BR");

        for (int i = 0; i <= 3; i++)
        {
            yield return new object[]
            {
                new
                {
                    name = faker.Company.CompanyName(),
                    document = faker.Company.Cnpj(),
                    telephone = faker.Phone.PhoneNumber(),
                    email = faker.Internet.Email(),
                    password = faker.Random.Hash()
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(GetCompanyTestData))]
    public async Task Company_Flow_Should_Work_Correctly(Object newCompanyPayload)
    {

        // Etapa 1 - Criar empresas
        HttpResponseMessage? postResponse = await _client
            .PostAsJsonAsync("/api/empresa", newCompanyPayload);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var postResult = await postResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(postResult?.Data);
        int companyId = postResult.Data.Id;

        dynamic payload = newCompanyPayload;
        Assert.Equal((string)payload.name, postResult.Data.Name);
        Assert.True(companyId > 0);

        // Etapa 2 - Buscar empresa por ID
        var getResponse = await _client.GetAsync($"/api/empresa/{companyId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getResult = await getResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(getResult?.Data);
        Assert.Equal(companyId, getResult?.Data.Id);
        Assert.Equal((string)payload.name, getResult?.Data.Name);

        // Etapa 3 - Listar todas as empresas
        var listResponse = await _client.GetAsync("/api/empresa");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }
}
