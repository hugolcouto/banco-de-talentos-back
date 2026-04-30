# 13. Guia Passo-a-Passo de Desenvolvimento

## 🎯 Como Adicionar um Novo Recurso

Este guia mostra como adicionar um novo recurso do zero, seguindo os padrões do projeto. Usaremos como exemplo adicionar funcionalidade completa para **Job (Vaga de Emprego)**.

---

## 📋 Checklist: Passo-a-Passo

- [ ] Passo 1: Revisar Entidade no Core
- [ ] Passo 2: Criar DTOs na Application
- [ ] Passo 3: Criar Interface de Service na Application
- [ ] Passo 4: Implementar Service na Application
- [ ] Passo 5: Criar Interface de Repository no Core
- [ ] Passo 6: Implementar Repository na Infrastructure
- [ ] Passo 7: Registrar Dependências
- [ ] Passo 8: Criar Controller na API
- [ ] Passo 9: Implementar Testes
- [ ] Passo 10: Testar Manualmente

---

## 🔧 Passo 1: Revisar Entidade no Core

A entidade provavelmente já existe. Verifique:

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Job.cs
public class Job : BaseEntity
{
    public Job() { }

    public Job(
        string title,
        string description,
        string benefits,
        string requirements,
        string optionalRequirements,
        string address,
        string modality,
        decimal salary,
        int myProperty,
        DateTime dueDate,
        int openedVacancies,
        int hirerId
    ) : base()
    {
        Title = title;
        Description = description;
        Benefits = benefits;
        Requirements = requirements;
        OptionalRequirements = optionalRequirements;
        Address = address;
        Modality = modality;
        Salary = salary;
        ShowSalary = true;
        MyProperty = myProperty;
        DueDate = dueDate;
        OpenedVacancies = openedVacancies;
        HirerId = hirerId;
    }

    // Propriedades com private set
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Benefits { get; private set; }
    public string Requirements { get; private set; }
    public string OptionalRequirements { get; private set; }
    public string Address { get; private set; }
    public string Modality { get; private set; }
    public decimal Salary { get; private set; }
    public bool ShowSalary { get; private set; }
    public int MyProperty { get; private set; }
    public DateTime DueDate { get; private set; }
    public int OpenedVacancies { get; private set; }
    public int HirerId { get; private set; }
}
```

✅ Entidade Job já existe e está em BaseEntity

---

## 📦 Passo 2: Criar DTOs na Application

### Input DTO - CreateJobModel

```csharp
// Arquivo: BancoDeTalentos.Application/Model/CreateJobModel.cs
namespace BancoDeTalentos.Application.Model;

public class CreateJobModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Benefits { get; set; }
    public string Requirements { get; set; }
    public string OptionalRequirements { get; set; }
    public string Address { get; set; }
    public string Modality { get; set; }  // "Remoto", "Híbrido", "Presencial"
    public decimal Salary { get; set; }
    public bool ShowSalary { get; set; }
    public DateTime DueDate { get; set; }
    public int OpenedVacancies { get; set; }
    public int CompanyId { get; set; }  // Referência à empresa (não HirerId)
}
```

### Output DTO - JobViewModel

```csharp
// Arquivo: BancoDeTalentos.Application/Model/JobViewModel.cs
using BancoDeTalentos.Core.Entities;

namespace BancoDeTalentos.Application.Model;

public class JobViewModel
{
    public JobViewModel(
        int id,
        string title,
        string description,
        string modality,
        decimal salary,
        bool showSalary,
        DateTime dueDate,
        int openedVacancies)
    {
        Id = id;
        Title = title;
        Description = description;
        Modality = modality;
        Salary = salary;
        ShowSalary = showSalary;
        DueDate = dueDate;
        OpenedVacancies = openedVacancies;
    }

    public int Id { get; set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Modality { get; private set; }
    public decimal Salary { get; private set; }
    public bool ShowSalary { get; private set; }
    public DateTime DueDate { get; private set; }
    public int OpenedVacancies { get; private set; }

    // ❌ NÃO incluído: Requirements, OptionalRequirements (pode ser muito texto)
    // ❌ NÃO incluído: Benefits (detalhe)
    // ❌ NÃO incluído: HirerId (interno)

    public static JobViewModel? FromEntity(Job? entity)
        => new(
            entity!.Id,
            entity.Title,
            entity.Description,
            entity.Modality,
            entity.Salary,
            entity.ShowSalary,
            entity.DueDate,
            entity.OpenedVacancies
        );
}
```

---

## 🏗️ Passo 3: Criar Interface de Service

```csharp
// Arquivo: BancoDeTalentos.Application/Interfaces/IJobService.cs
namespace BancoDeTalentos.Application.Interfaces;

public interface IJobService
{
    ResultViewModel<JobViewModel> CreateJob(CreateJobModel model);
    ResultViewModel<JobViewModel> GetJobById(int id);
    ResultViewModel<List<JobViewModel>> GetJobsByCompany(int companyId);
    ResultViewModel<List<JobViewModel>> GetAllJobs();
    ResultViewModel<JobViewModel> UpdateJob(int id, UpdateJobModel model);
    ResultViewModel DeleteJob(int id);
}
```

---

## 💼 Passo 4: Implementar Service

```csharp
// Arquivo: BancoDeTalentos.Application/Services/JobService.cs
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;

namespace BancoDeTalentos.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;

    public JobService(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository)
    {
        _jobRepository = jobRepository;
        _companyRepository = companyRepository;
    }

    public ResultViewModel<JobViewModel> CreateJob(CreateJobModel model)
    {
        // 1. Validar entrada
        if (string.IsNullOrWhiteSpace(model.Title))
            return ResultViewModel<JobViewModel>.Error("Título é obrigatório", null);

        if (model.Salary < 0)
            return ResultViewModel<JobViewModel>.Error("Salário não pode ser negativo", null);

        if (model.DueDate < DateTime.Now)
            return ResultViewModel<JobViewModel>.Error("Data limite não pode ser no passado", null);

        // 2. Validar que empresa existe
        Company? company = _companyRepository.GetCompanyById(model.CompanyId);
        if (company == null)
            return ResultViewModel<JobViewModel>.Error("Empresa não encontrada", null);

        // 3. Criar entidade
        Job job = new Job(
            model.Title,
            model.Description,
            model.Benefits,
            model.Requirements,
            model.OptionalRequirements,
            model.Address,
            model.Modality,
            model.Salary,
            0,  // myProperty
            model.DueDate,
            model.OpenedVacancies,
            model.CompanyId  // hirerId = companyId
        );

        // 4. Persistir
        _jobRepository.CreateJob(job);

        // 5. Converter e retornar
        JobViewModel? viewModel = JobViewModel.FromEntity(job);
        return ResultViewModel<JobViewModel>.Success(viewModel!);
    }

    public ResultViewModel<JobViewModel> GetJobById(int id)
    {
        if (id <= 0)
            return ResultViewModel<JobViewModel>.Error("ID inválido", null);

        Job? job = _jobRepository.GetJobById(id);
        if (job == null)
            return ResultViewModel<JobViewModel>.Error("Vaga não encontrada", null);

        JobViewModel? viewModel = JobViewModel.FromEntity(job);
        return ResultViewModel<JobViewModel>.Success(viewModel!);
    }

    public ResultViewModel<List<JobViewModel>> GetAllJobs()
    {
        List<Job> jobs = _jobRepository.GetAllJobs();
        List<JobViewModel> viewModels = jobs
            .Select(j => JobViewModel.FromEntity(j))
            .ToList();

        return ResultViewModel<List<JobViewModel>>.Success(viewModels);
    }

    // ... outros métodos similares ...
}
```

---

## 🗄️ Passo 5: Criar Interface de Repository

```csharp
// Arquivo: BancoDeTalentos.Core/Interfaces/IJobRepository.cs
namespace BancoDeTalentos.Core.Interfaces;

public interface IJobRepository
{
    int CreateJob(Job job);
    Job? GetJobById(int id);
    List<Job> GetAllJobs();
    List<Job> GetJobsByCompanyId(int companyId);
    void UpdateJob(Job job);
    void DeleteJob(int id);
}
```

---

## 🔧 Passo 6: Implementar Repository

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/JobRepository.cs
using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;

namespace BancoDeTalentos.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public JobRepository(BancoDeTalentosDbContext context)
        => _context = context;

    public int CreateJob(Job job)
    {
        _context.Jobs.Add(job);
        _context.SaveChanges();
        return job.Id;
    }

    public Job? GetJobById(int id)
    {
        return _context.Jobs.FirstOrDefault(j => j.Id == id);
    }

    public List<Job> GetAllJobs()
    {
        return _context.Jobs.ToList();
    }

    public List<Job> GetJobsByCompanyId(int companyId)
    {
        return _context.Jobs
            .Where(j => j.HirerId == companyId)
            .ToList();
    }

    public void UpdateJob(Job job)
    {
        _context.Jobs.Update(job);
        _context.SaveChanges();
    }

    public void DeleteJob(int id)
    {
        var job = _context.Jobs.Find(id);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            _context.SaveChanges();
        }
    }
}
```

---

## 📝 Passo 7: Registrar Dependências

### Application Module

```csharp
// Arquivo: BancoDeTalentos.Application/ApplicationModule.cs
private static IServiceCollection AddServices(
    this IServiceCollection services)
{
    services.AddScoped<ICompanyService, CompanyService>();
    services.AddScoped<IJobService, JobService>();  // ← Adicionar

    return services;
}
```

### Infrastructure Module

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/InfrastructureModule.cs
private static IServiceCollection AddRepositories(
    this IServiceCollection services)
{
    services.AddScoped<ICompanyRepository, CompanyRepository>();
    services.AddScoped<IJobRepository, JobRepository>();  // ← Adicionar

    return services;
}
```

---

## 🌐 Passo 8: Criar Controller

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/JobController.cs
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

[Route("api/vaga")]  // Rota: /api/vaga
[ApiController]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    // POST /api/vaga
    [HttpPost]
    public IActionResult Create(CreateJobModel model)
    {
        ResultViewModel<JobViewModel> result = _jobService.CreateJob(model);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data?.Id },
            result
        );
    }

    // GET /api/vaga
    [HttpGet]
    public IActionResult Get()
    {
        ResultViewModel<List<JobViewModel>> result = _jobService.GetAllJobs();
        return Ok(result);
    }

    // GET /api/vaga/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        ResultViewModel<JobViewModel> result = _jobService.GetJobById(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // GET /api/vaga/empresa/{companyId}
    [HttpGet("empresa/{companyId}")]
    public IActionResult GetByCompany(int companyId)
    {
        ResultViewModel<List<JobViewModel>> result =
            _jobService.GetJobsByCompany(companyId);

        return Ok(result);
    }

    // PATCH /api/vaga/{id}
    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateJobModel model)
    {
        ResultViewModel<JobViewModel> result = _jobService.UpdateJob(id, model);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // DELETE /api/vaga/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        ResultViewModel result = _jobService.DeleteJob(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return NoContent();
    }
}
```

---

## 🧪 Passo 9: Implementar Testes

```csharp
// Arquivo: BancoDeTalentos.Tests/Integrations/JobControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using BancoDeTalentos.Application.Model;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BancoDeTalentos.Tests.Integrations;

public class JobControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Faker _faker;

    public JobControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task Create_Job_WithValidData_ShouldReturnCreated()
    {
        // ARRANGE: Criar empresa primeiro
        var companyModel = new CreateCompanyModel
        {
            Name = _faker.Company.CompanyName(),
            Document = _faker.Company.Cnpj(),
            Telephone = _faker.Phone.PhoneNumber(),
            Email = _faker.Internet.Email(),
            Password = _faker.Random.Hash()
        };

        var companyResponse = await _client.PostAsJsonAsync(
            "/api/empresa",
            companyModel
        );

        var companyResult = await companyResponse.Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        int companyId = companyResult?.Data?.Id ?? 0;

        // Criar job
        var jobModel = new CreateJobModel
        {
            Title = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentences(3).ToString(),
            Benefits = _faker.Lorem.Sentence(),
            Requirements = _faker.Lorem.Sentence(),
            OptionalRequirements = _faker.Lorem.Sentence(),
            Address = _faker.Address.FullAddress(),
            Modality = "Remoto",
            Salary = _faker.Random.Decimal(1000, 10000),
            ShowSalary = true,
            DueDate = DateTime.Now.AddDays(30),
            OpenedVacancies = _faker.Random.Int(1, 10),
            CompanyId = companyId
        };

        // ACT
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/vaga",
            jobModel
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ResultViewModel<JobViewModel?>>();

        Assert.NotNull(result?.Data);
        Assert.Equal(jobModel.Title, result.Data!.Title);
        Assert.True(result.Data!.Id > 0);
    }

    [Fact]
    public async Task GetById_Job_ShouldReturnOk()
    {
        // Criar job (omitido por brevidade)
        // ...

        // Buscar
        HttpResponseMessage response = await _client.GetAsync("/api/vaga/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## 🔍 Passo 10: Testar Manualmente

### Usando Postman/Insomnia

```http
POST /api/vaga
Content-Type: application/json

{
    "title": "Desenvolvedor C#",
    "description": "Procuramos devs experientes",
    "benefits": "Vale refeição, home office",
    "requirements": "3+ anos com .NET",
    "optionalRequirements": "MongoDB, Docker",
    "address": "São Paulo, SP",
    "modality": "Híbrido",
    "salary": 8000,
    "showSalary": true,
    "dueDate": "2026-06-30",
    "openedVacancies": 2,
    "companyId": 1
}
```

```http
GET /api/vaga/1
```

```http
GET /api/vaga
```

---

## ✅ Checklist Completo

- ✅ Entidade criada (Job)
- ✅ DTOs criados (CreateJobModel, JobViewModel)
- ✅ Interfaces criadas (IJobService, IJobRepository)
- ✅ Service implementado com validações
- ✅ Repository implementado
- ✅ Dependências registradas
- ✅ Controller criado com endpoints CRUD
- ✅ Testes de integração
- ✅ Testado manualmente

Parabéns! Novo recurso completo e funcionando! 🎉

---

**Referências de Arquivo:**

- `BancoDeTalentos.Core/Entities/Job.cs`
- `BancoDeTalentos.Application/Model/CreateJobModel.cs`
- `BancoDeTalentos.Application/Model/JobViewModel.cs`
- `BancoDeTalentos.Application/Interfaces/IJobService.cs`
- `BancoDeTalentos.Application/Services/JobService.cs`
- `BancoDeTalentos.Core/Interfaces/IJobRepository.cs`
- `BancoDeTalentos.Infrastructure/Persistence/Repositories/JobRepository.cs`
- `BancoDeTalentos.API/Controllers/JobController.cs`
- `BancoDeTalentos.Tests/Integrations/JobControllerTests.cs`
