using System.Net;
using System.Net.Http.Json;
using BancoDeTalentos.API.Extensions;
using BancoDeTalentos.Application.Model;
using BancoDeTalentos.Tests.Factories;
using Bogus;
using Bogus.Extensions.Brazil;

namespace BancoDeTalentos.Tests.Integrations;

public class JobControllerTests : IClassFixture<TestingWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private const string _apiPath = "/api/vagas";
    private Faker _faker = new Faker("pt_BR");
    private int? _companyId;

    public JobControllerTests(TestingWebApplicationFactory factory) : base()
    {
        _client = factory.CreateClient();
    }


    public async Task InitializeAsync()
    {
        object newCompanyObject = new
        {
            name = _faker.Company.CompanyName(),
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash(),
            about = _faker.Lorem.Paragraph(2),
        };

        HttpResponseMessage? getResponse = await _client
            .PostAsJsonAsync("/api/empresa", newCompanyObject);

        Assert.Equal(HttpStatusCode.Created, getResponse.StatusCode);

        ResultViewModel<CompanyViewModel?>? getResult = await getResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        _companyId = getResult!.Data!.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Job_Flow_Should_Work()
    {
        object createJobObject = new
        {
            title = _faker.Name.JobTitle(),
            description = _faker.Lorem.Paragraph(3),
            benefits = _faker.Lorem.Paragraph(1),
            requirements = _faker.Lorem.Paragraph(1),
            optionalRequirements = _faker.Lorem.Paragraph(1),
            address = _faker.Address.FullAddress(),
            modality = _faker.Random.Word(),
            salary = _faker.Random.Double(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = _companyId,
        };

        object updateJobObject = new
        {
            title = _faker.Name.JobTitle(),
            description = _faker.Lorem.Paragraph(3),
            benefits = _faker.Lorem.Paragraph(1),
            requirements = _faker.Lorem.Paragraph(1),
            optionalRequirements = _faker.Lorem.Paragraph(1),
            address = _faker.Address.FullAddress(),
            modality = _faker.Random.Word(),
            salary = _faker.Random.Double(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = _companyId,
        };

        Assert.NotNull(_companyId);
        int jobId = await CreateJob(createJobObject);
        await GetJobById(jobId, createJobObject);
        await UpdateJob(jobId, updateJobObject, createJobObject);
        await DeleteJob(jobId);
    }

    /// <summary>
    /// Teste de criação de vaga
    /// </summary>
    /// <param name="newJobObject"></param>
    /// <returns></returns>
    private async Task<int> CreateJob(object newJobObject)
    {
        // Step 2 - Create a new job
        HttpResponseMessage? getJobResponse = await _client.PostAsJsonAsync(_apiPath, newJobObject);

        Assert.Equal(HttpStatusCode.OK, getJobResponse.StatusCode);

        ResultViewModel<JobViewModel?>? postResult = await getJobResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<JobViewModel?>>();

        Assert.NotNull(postResult!.Data);

        return postResult!.Data!.Id;
    }

    /// <summary>
    /// Teste de busca de vaga por id
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="newJobObject"></param>
    /// <returns></returns>
    private async Task GetJobById(int jobId, object newJobObject)
    {
        HttpResponseMessage? getJobRepsonse = await _client.GetAsync($"{_apiPath}/{jobId}");

        Assert.Equal(HttpStatusCode.OK, getJobRepsonse.StatusCode);

        ResultViewModel<JobViewModel?>? getResult = await getJobRepsonse
            .Content
            .ReadFromJsonAsync<ResultViewModel<JobViewModel?>>();

        Assert.Equal(newJobObject, getResult!.Data);
    }

    /// <summary>
    /// Teste de criação de vaga
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="updateJobObject"></param>
    /// <param name="createJobObject"></param>
    /// <returns></returns>
    private async Task UpdateJob(int jobId, object updateJobObject, object createJobObject)
    {
        HttpResponseMessage? updateJob = await _client.PatchAsJsonAsync(
            $"{_apiPath}/{jobId}", updateJobObject
        );

        Assert.Equal(HttpStatusCode.OK, updateJob.StatusCode);

        HttpResponseMessage? getJob = await _client.GetAsync($"{_apiPath}/{jobId}");

        Assert.Equal(HttpStatusCode.OK, getJob.StatusCode);

        ResultViewModel<JobViewModel?>? res = await getJob
            .Content
            .ReadFromJsonAsync<ResultViewModel<JobViewModel?>>();

        Assert.NotEqual(createJobObject, updateJobObject);
    }

    /// <summary>
    /// Teste de deleção de vaga
    /// </summary>
    /// <returns></returns>
    private async Task DeleteJob(int jobId)
    {
        HttpResponseMessage? deleteJob = await _client.DeleteAsync($"{_apiPath}/{jobId}");

        Assert.Equal(HttpStatusCode.OK, deleteJob.StatusCode);

        HttpResponseMessage? getDeletedJob = await _client.GetAsync($"{_apiPath}/{jobId}");

        Assert.Equal(HttpStatusCode.NotFound, getDeletedJob.StatusCode);
    }
}
