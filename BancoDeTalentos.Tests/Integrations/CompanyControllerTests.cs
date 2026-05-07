using System.Net;
using System.Net.Http.Json;
using BancoDeTalentos.Application.Model;
using BancoDeTalentos.Tests.Factories;
using Bogus;
using Bogus.Extensions.Brazil;
using Microsoft.AspNetCore.Http;

namespace BancoDeTalentos.Tests.Integrations;

public class CompanyControllerTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;
    private int? _companyId;

    public CompanyControllerTests(TestingWebApplicationFactory factory) : base()
    {
        _client = factory.CreateClient();
        _companyId = null;
    }

    public static IEnumerable<object[]> GetCompanyTestData()
    {
        Faker faker = new Faker("pt_BR");

        for (int i = 0; i < 2; i++)
        {
            yield return new object[]
            {
                // Dados de criação
                new
                {
                    name = faker.Company.CompanyName(),
                    document = faker.Company.Cnpj(),
                    telephone = faker.Phone.PhoneNumber(),
                    email = faker.Internet.Email(),
                    password = faker.Random.Hash(),
                    about = faker.Lorem.Paragraph(),
                },

                // Dados de atualização
                new
                {
                    name = faker.Company.CompanyName(),
                    telephone = faker.Phone.PhoneNumber(),
                    email = faker.Internet.Email(),
                    about = faker.Lorem.Paragraph()
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(GetCompanyTestData))]
    public async Task Company_Flow_Should_Work_Correctly(Object newCompanyPayload, Object updatePayload)
    {
        // Etapa 1 - Criar empresas
        HttpResponseMessage? postResponse = await _client
            .PostAsJsonAsync("/api/empresa", newCompanyPayload);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        ResultViewModel<CompanyViewModel?>? postResult = await postResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(postResult?.Data);
        int companyId = postResult.Data.Id;
        _companyId = postResult.Data.Id;

        dynamic payload = newCompanyPayload;
        Assert.Equal((string)payload.name, postResult.Data.Name);
        Assert.True(companyId > 0);

        // Etapa 2 - Buscar empresa por ID
        HttpResponseMessage? getResponse = await _client.GetAsync($"/api/empresa/{companyId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        ResultViewModel<CompanyViewModel?>? getResult = await getResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(getResult?.Data);
        Assert.Equal(companyId, getResult?.Data.Id);
        Assert.Equal((string)payload.name, getResult?.Data.Name);

        // Etapa 3 - Listar todas as empresas
        HttpResponseMessage? listResponse = await _client.GetAsync("/api/empresa");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Etapa 4 - Atualizar empresa
        HttpResponseMessage updateResponse = await _client
            .PatchAsJsonAsync($"/api/empresa/{companyId}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        HttpResponseMessage? getUpdatedResponse = await _client.GetAsync($"/api/empresa/{companyId}");

        Assert.Equal(HttpStatusCode.OK, getUpdatedResponse.StatusCode);

        ResultViewModel<CompanyViewModel?>? getUpdateResult = await getUpdatedResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        payload = updatePayload;
        Assert.Equal((string)payload.name, getUpdateResult?.Data?.Name);

        // Etapa 5 - Deletar empresa
        HttpResponseMessage? deleteResponse = await _client.DeleteAsync($"/api/empresa/{companyId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage tryGetDeleted = await _client.GetAsync($"/api/empresa/{companyId}");

        Assert.Equal(HttpStatusCode.NotFound, tryGetDeleted.StatusCode);
    }
}
