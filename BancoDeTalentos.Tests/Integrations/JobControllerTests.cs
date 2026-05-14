using System.Net;
using System.Net.Http.Json;
using System.Runtime.ConstrainedExecution;
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
        object newCompanyObject = new CreateCompanyModel(
            _faker.Company.CompanyName(),
            _faker.Company.Cnpj(),
            _faker.Phone.PhoneNumber(),
            _faker.Internet.Email(),
            _faker.Random.Hash(),
            _faker.Lorem.Paragraph(2)
        );

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
        Assert.NotNull(_companyId);

        object createJobObject = new
        {
            title = _faker.Name.JobTitle(),
            description = _faker.Lorem.Paragraph(3),
            benefits = _faker.Lorem.Paragraph(1),
            requirements = _faker.Lorem.Paragraph(1),
            optionalRequirements = _faker.Lorem.Paragraph(1),
            address = _faker.Address.FullAddress(),
            modality = _faker.Random.Word(),
            salary = _faker.Random.Decimal(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = (int)_companyId
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
            salary = _faker.Random.Decimal(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = (int)_companyId
        };

        int jobId = await CreateJob(createJobObject);
        await GetAllJobs();
        await GetJobById(jobId, createJobObject);
        await UpdateJob(jobId, updateJobObject, createJobObject);
        await DeleteJob(jobId);
    }


    private async Task GetAllJobs()
    {
        HttpResponseMessage? res = await _client.GetAsync(_apiPath);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
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

        Assert.Equal(HttpStatusCode.Created, getJobResponse.StatusCode);

        ResultViewModel<JobViewModel?>? postResult = await getJobResponse
            .Content
            .ReadFromJsonAsync<ResultViewModel<JobViewModel?>>();

        Assert.NotNull(postResult!.Data);

        return postResult.Data.Id;
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

        dynamic updatedData = newJobObject;
        Assert.Equal(updatedData.title, getResult!.Data!.Title);
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

        dynamic model = createJobObject;
        dynamic updateModel = updateJobObject;
        Assert.NotEqual(res?.Data?.Title, model.title);
        Assert.Equal(res?.Data?.Title, updateModel.title);
    }

    /// <summary>
    /// Teste de deleção de vaga
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    private async Task DeleteJob(int jobId)
    {
        HttpResponseMessage? res = await _client.DeleteAsync($"{_apiPath}/{jobId}");

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

        HttpResponseMessage? deletedRes = await _client.GetAsync($"{_apiPath}/{jobId}");

        Console.WriteLine($"{deletedRes}");

        Assert.Equal(HttpStatusCode.NotFound, deletedRes.StatusCode);
    }

    [Fact]
    public async Task GetJobsByCompany_Should_Return_Jobs_From_Specific_Company()
    {
        // Arrange
        Assert.NotNull(_companyId);
        int differentCompanyId = await CreateDifferentCompany();

        object job1 = new
        {
            title = _faker.Name.JobTitle(),
            description = _faker.Lorem.Paragraph(3),
            benefits = _faker.Lorem.Paragraph(1),
            requirements = _faker.Lorem.Paragraph(1),
            optionalRequirements = _faker.Lorem.Paragraph(1),
            address = _faker.Address.FullAddress(),
            modality = _faker.Random.Word(),
            salary = _faker.Random.Decimal(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = (int)_companyId
        };

        object job2 = new
        {
            title = _faker.Name.JobTitle(),
            description = _faker.Lorem.Paragraph(3),
            benefits = _faker.Lorem.Paragraph(1),
            requirements = _faker.Lorem.Paragraph(1),
            optionalRequirements = _faker.Lorem.Paragraph(1),
            address = _faker.Address.FullAddress(),
            modality = _faker.Random.Word(),
            salary = _faker.Random.Decimal(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = (int)_companyId
        };

        object jobOtherCompany = new
        {
            title = _faker.Name.JobTitle(),
            description = _faker.Lorem.Paragraph(3),
            benefits = _faker.Lorem.Paragraph(1),
            requirements = _faker.Lorem.Paragraph(1),
            optionalRequirements = _faker.Lorem.Paragraph(1),
            address = _faker.Address.FullAddress(),
            modality = _faker.Random.Word(),
            salary = _faker.Random.Decimal(1000, 3000),
            showSalary = _faker.Random.Bool(),
            dueDate = _faker.Date.Future(refDate: DateTime.Now),
            openedVacancies = _faker.Random.Int(1, 10),
            companyId = differentCompanyId
        };

        await CreateJob(job1);
        await CreateJob(job2);
        await CreateJob(jobOtherCompany);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/empresa/{_companyId}/vagas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResultViewModel<List<JobViewModel>>>();
        Assert.NotNull(result!.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.All(result.Data, job =>
            Assert.Equal((int)_companyId, job.CompanyId));
    }

    private async Task<int> CreateDifferentCompany()
    {
        object newCompanyObject = new CreateCompanyModel(
            _faker.Company.CompanyName(),
            _faker.Company.Cnpj(),
            _faker.Phone.PhoneNumber(),
            _faker.Internet.Email(),
            _faker.Random.Hash(),
            _faker.Lorem.Paragraph(2)
        );

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/empresa", newCompanyObject);
        var result = await response.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();
        return result!.Data!.Id;
    }
}
