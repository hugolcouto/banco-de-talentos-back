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
    private readonly string _apiPath = "/api/empresa";
    private Faker _faker = new Faker("pt_BR");

    public CompanyControllerTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Company_Flow_Should_Work()
    {
        object createModel = new
        {
            name = _faker.Company.CompanyName(),
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash(),
            about = _faker.Lorem.Paragraph(),
        };

        object updateModel = new
        {
            name = _faker.Company.CompanyName(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            about = _faker.Lorem.Paragraph()
        };

        int companyId = await CreateCompany(createModel);
        await CreateCompany(createModel);
        await GetAllCompanies();
        await GetCompanyById(companyId, createModel);
        await UpdateCompany(companyId, updateModel, createModel);
        await DeleteCompany(companyId);
    }

    // Create
    private async Task<int> CreateCompany(object createModel)
    {
        HttpResponseMessage? res = await _client.PostAsJsonAsync(
            _apiPath,
            createModel
        );

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        ResultViewModel<CompanyViewModel?>? content = await res
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(content!.Data);

        return content.Data.Id!;
    }

    // Read
    private async Task GetAllCompanies()
    {
        HttpResponseMessage? res = await _client.GetAsync(_apiPath);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        Assert.NotNull(res);
    }

    private async Task GetCompanyById(int companyId, object createModel)
    {
        HttpResponseMessage? res = await _client.GetAsync($"{_apiPath}/{companyId}");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        ResultViewModel<CompanyViewModel?>? content = await res
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        dynamic payload = createModel;
        Assert.Equal((string)payload.name, content!.Data!.Name);
    }

    // Update
    private async Task UpdateCompany(int companyId, object updateModel, object createModel)
    {
        HttpResponseMessage? res = await _client.PatchAsJsonAsync(
            $"{_apiPath}/{companyId}", updateModel
        );

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        HttpResponseMessage? updatedRes = await _client.GetAsync(
            $"{_apiPath}/{companyId}"
        );

        ResultViewModel<CompanyViewModel?>? content = await updatedRes
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(content!.Data!);

        dynamic createModelPayload = createModel;
        Assert.NotEqual(content.Data.Name, (string)createModelPayload.name);
    }

    // Delete
    private async Task DeleteCompany(int companyId)
    {
        HttpResponseMessage? res = await _client.DeleteAsync($"{_apiPath}/{companyId}");

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

        HttpResponseMessage? deletedRes = await _client.GetAsync($"{_apiPath}/{companyId}");

        Assert.Equal(HttpStatusCode.NotFound, deletedRes.StatusCode);
    }
}
