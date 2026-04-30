# 3. Estrutura do Projeto em Detalhes

## 📁 Organização Completa de Diretórios

```
BancoDeTalentos/
│
├── .git/                              # Controle de versão Git
├── .vscode/                           # Configurações do VS Code
├── .gitignore                         # Arquivos ignorados pelo Git
├── .editorconfig                      # Configurações de formatação
│
├── BancoDeTalentos.API/               # ➡️ CAMADA DE APRESENTAÇÃO
│   ├── Controllers/
│   │   ├── CompanyController.cs       # Endpoints para empresas
│   │   ├── CandidateController.cs     # Endpoints para candidatos
│   │   └── WeatherForecastController.cs # Exemplo (remover)
│   ├── Properties/
│   │   └── launchSettings.json        # Configurações de execução
│   ├── bin/                           # Compilados (auto-gerado)
│   ├── obj/                           # Objetos intermediários (auto-gerado)
│   ├── appsettings.json               # Configurações gerais
│   ├── appsettings.Development.json   # Configurações de desenvolvimento
│   ├── Program.cs                     # Ponto de entrada da aplicação
│   ├── WeatherForecast.cs             # Modelo de exemplo (remover)
│   ├── BancoDeTalentos.API.http       # Requisições HTTP de teste
│   └── BancoDeTalentos.API.csproj     # Arquivo de projeto
│
├── BancoDeTalentos.Application/       # ➡️ CAMADA DE APLICAÇÃO
│   ├── Interfaces/
│   │   └── ICompanyService.cs         # Contrato de serviço
│   ├── Model/
│   │   ├── CreateCompanyModel.cs      # DTO de entrada
│   │   ├── CompanyViewModel.cs        # DTO de saída
│   │   └── ResultViewModel.cs         # Padrão de resposta
│   ├── Services/
│   │   └── CompanyService.cs          # Lógica de negócio
│   ├── ApplicationModule.cs           # Injeção de dependências
│   ├── bin/                           # Compilados
│   ├── obj/                           # Objetos intermediários
│   └── BancoDeTalentos.Application.csproj
│
├── BancoDeTalentos.Core/              # ➡️ CAMADA DE DOMÍNIO/CORE
│   ├── Entities/
│   │   ├── BaseEntity.cs              # Classe base para todas as entidades
│   │   ├── Company.cs                 # Entidade Empresa
│   │   ├── Candidate.cs               # Entidade Candidato
│   │   ├── Job.cs                     # Entidade Vaga de Emprego
│   │   ├── Resume.cs                  # Entidade Curriculum
│   │   └── Backoffice.cs              # Entidade Administrador
│   ├── Interfaces/
│   │   └── ICompanyRepository.cs      # Contrato de repository
│   ├── ValueObjects/
│   │   └── Address.cs                 # Objeto de valor Endereço
│   ├── bin/                           # Compilados
│   ├── obj/                           # Objetos intermediários
│   └── BancoDeTalentos.Core.csproj
│
├── BancoDeTalentos.Infrastructure/    # ➡️ CAMADA DE INFRAESTRUTURA
│   ├── Persistence/
│   │   ├── BancoDeTalentosDbContext.cs    # Context do Entity Framework
│   │   └── Repositories/
│   │       └── CompanyRepository.cs       # Implementação de ICompanyRepository
│   ├── bin/                           # Compilados
│   ├── obj/                           # Objetos intermediários
│   ├── InfrastructureModule.cs        # Configuração de injeção
│   └── BancoDeTalentos.Infrastructure.csproj
│
├── BancoDeTalentos.Tests/             # ➡️ CAMADA DE TESTES
│   ├── Integrations/
│   │   └── CompanyControllerTests.cs  # Testes de integração
│   ├── bin/                           # Compilados
│   ├── obj/                           # Objetos intermediários
│   └── BancoDeTalentos.Tests.csproj
│
├── docs/                              # 📚 DOCUMENTAÇÃO
│   ├── README.md                      # Índice principal
│   ├── 01-visao-geral.md
│   ├── 02-arquitetura-limpa.md
│   ├── 03-estrutura-projeto.md
│   └── ... (outros documentos)
│
└── BancoDeTalentos.slnx               # Arquivo de solução
```

---

## 🏗️ Descrição de Cada Camada

### 1️⃣ Camada de Apresentação - `BancoDeTalentos.API/`

**Responsabilidade:** Expor endpoints HTTP que o cliente pode chamar.

#### Controllers

Os controllers são classes que lidam com requisições HTTP. Cada controller é responsável por um recurso específico.

**Exemplo - CompanyController:**

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
[Route("api/empresa")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    // Injeção de dependência via construtor
    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    // POST /api/empresa
    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        // Delega para o serviço
        ResultViewModel<CompanyViewModel> companyResult =
            _companyService.CreateCompany(model);

        return CreatedAtAction(
            nameof(GetById),
            new { id = companyResult.Data?.Id },
            companyResult
        );
    }

    // GET /api/empresa/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok();
    }
}
```

**Responsabilidades do Controller:**

- ✅ Receber requisições HTTP
- ✅ Validar entrada (atributos `[Required]`, `[StringLength]`)
- ✅ Chamar o serviço apropriado
- ✅ Formatar e retornar resposta HTTP

**O que NÃO fazer no Controller:**

- ❌ Lógica de negócio complexa
- ❌ Acesso direto ao banco de dados
- ❌ Transformação de dados complexa

#### Program.cs - Ponto de Entrada

```csharp
// Arquivo: BancoDeTalentos.API/Program.cs
using BancoDeTalentos.Application;
using BancoDeTalentos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Injeção de dependências via módulos
builder.Services
    .AddApplication()                              // Registra services
    .AddInfrastructure(builder.Configuration);    // Registra repositories

// Adiciona controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

### 2️⃣ Camada de Aplicação - `BancoDeTalentos.Application/`

**Responsabilidade:** Orquestrar a lógica de negócio e coordenar entre controller e repositório.

#### Services

Os serviços implementam a lógica de negócio. Cada serviço é responsável por um conjunto de operações relacionadas a uma entidade.

**Exemplo - CompanyService:**

```csharp
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    // Injeção de dependência
    public CompanyService(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    // Implementação da lógica de negócio
    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        // 1. Criar entidade do domínio
        Company company = new Company(
            model.Name,
            model.Document,
            model.Telephone,
            model.Email,
            model.Password
        );

        // 2. Persistir através do repositório
        _companyRepository.CreateCompany(company);

        // 3. Converter para ViewModel (DTO)
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

        // 4. Retornar resultado padronizado
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }

    public ResultViewModel<CompanyViewModel> GetCompany(int id)
    {
        throw new NotImplementedException();
    }
}
```

#### Interfaces de Serviço

```csharp
// Arquivo: BancoDeTalentos.Application/Interfaces/ICompanyService.cs
public interface ICompanyService
{
    ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model);
    ResultViewModel<CompanyViewModel> GetCompany(int id);
}
```

**Benefícios:**

- Interface permite injeção de dependência
- Facilita testes com mock
- Define contrato claro

#### DTOs - Data Transfer Objects

Os DTOs são classes simples usadas para transferir dados entre camadas.

**CreateCompanyModel (Input DTO):**

```csharp
// Arquivo: BancoDeTalentos.Application/Model/CreateCompanyModel.cs
public class CreateCompanyModel
{
    public string Name { get; set; }
    public string Document { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

**Uso:**

```csharp
// No controller, o modelo é automaticamente desserializado
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    // ASP.NET Core automaticamente converte JSON → CreateCompanyModel
    // model.Name, model.Document, etc. estão disponíveis
}
```

**CompanyViewModel (Output DTO):**

```csharp
// Arquivo: BancoDeTalentos.Application/Model/CompanyViewModel.cs
public class CompanyViewModel
{
    public int Id { get; set; }
    public string Name { get; private set; }
    public string Telephone { get; private set; }
    public string Email { get; private set; }
    public string About { get; private set; }

    // Factory method para conversão
    public static CompanyViewModel? FromEntity(Company? entity)
        => new(
            entity!.Id,
            entity.Name,
            entity.Telephone,
            entity.Email,
            entity.About
        );
}
```

**Uso:**

```csharp
// No service
CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

// Retornado como JSON
// { "id": 1, "name": "Acme Corp", "telephone": "...", "email": "...", "about": "..." }
```

**ResultViewModel - Padrão de Resposta:**

```csharp
// Arquivo: BancoDeTalentos.Application/Model/ResultViewModel.cs
public class ResultViewModel
{
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
}

public class ResultViewModel<T> : ResultViewModel
{
    public T? Data { get; set; }

    public static ResultViewModel<T> Success(T data) => new(data);
    public static ResultViewModel<T> Error(string message, T? data) =>
        new(data, message, false);
}
```

**Uso:**

```csharp
// Resposta bem-sucedida
return ResultViewModel<CompanyViewModel>.Success(viewModel);

// JSON retornado:
// {
//   "data": { "id": 1, "name": "Acme Corp", ... },
//   "message": "",
//   "isSuccess": true
// }

// Resposta com erro
return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada", null);

// JSON retornado:
// {
//   "data": null,
//   "message": "Empresa não encontrada",
//   "isSuccess": false
// }
```

#### ApplicationModule - Injeção de Dependências

```csharp
// Arquivo: BancoDeTalentos.Application/ApplicationModule.cs
using Microsoft.Extensions.DependencyInjection;

public static class ApplicationModule
{
    // Método de extensão para configurar tudo da Application
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }

    // Registra todos os serviços
    private static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        // Registra ICompanyService com implementação CompanyService
        // Ciclo de vida Scoped = uma instância por requisição HTTP
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
```

**Uso no Program.cs:**

```csharp
// Arquivo: BancoDeTalentos.API/Program.cs
builder.Services.AddApplication();  // Registra todos os serviços da Application
```

---

### 3️⃣ Camada de Core/Domínio - `BancoDeTalentos.Core/`

**Responsabilidade:** Contém as entidades e regras de negócio. A camada mais pura e independente.

#### Entidades

**BaseEntity - Classe Base:**

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/BaseEntity.cs
public class BaseEntity
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
```

**Benefícios:**

- ✅ Evita duplicação de código
- ✅ Todas as entidades têm `Id` e `CreatedAt`
- ✅ Propriedades privadas garantem integridade

**Company - Entidade de Empresa:**

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
public class Company : BaseEntity
{
    // Construtor sem parâmetros (necessário para EF Core)
    public Company() { }

    // Construtor com parâmetros (para criação)
    public Company(
        string name,
        string document,
        string telephone,
        string email,
        string password
    ) : base()
    {
        Name = name;
        Document = document;
        Telephone = telephone;
        Email = email;
        Password = password;
        About = "";
        Jobs = new List<Job>();
    }

    // Propriedades privadas garantem controle
    public string Name { get; private set; }
    public string Document { get; private set; }
    public string Telephone { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string About { get; private set; }

    // Relacionamento one-to-many
    public List<Job> Jobs { get; private set; } = new List<Job>();
}
```

**Características:**

- ✅ Propriedades `private set` - Só podem ser modificadas dentro da entidade
- ✅ Encapsulamento - Protege a integridade dos dados
- ✅ Construtor com parâmetros - Valida dados na criação
- ✅ Construtor sem parâmetros - Necessário para Entity Framework

#### Outras Entidades

**Candidate (Candidato):**

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Candidate.cs
public class Candidate : BaseEntity
{
    public Candidate() { }

    public Candidate(
        string fullName,
        DateTime birthdate,
        string address,
        string description,
        string phoneNumber,
        string email,
        string document
    ) : base()
    {
        FullName = fullName;
        Birthdate = birthdate;
        Address = address;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        Document = document;
        Resume = null;
    }

    public string FullName { get; private set; }
    public DateTime Birthdate { get; private set; }
    public string Address { get; private set; }
    public string Description { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public string Document { get; private set; }
    public Resume? Resume { get; private set; }  // Relacionamento
}
```

**Resume (Curriculum):**

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Resume.cs
public class Resume : BaseEntity
{
    public Resume() { }

    public Resume(
        List<string> scholarity,
        List<string> courses,
        List<string> experiences
    ) : base()
    {
        Scholarity = scholarity;
        Courses = courses;
        Experiences = experiences;
    }

    public List<string> Scholarity { get; private set; }
    public List<string> Courses { get; private set; }
    public List<string> Experiences { get; private set; }
}
```

#### Interfaces de Negócio

```csharp
// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
public interface ICompanyRepository
{
    int CreateCompany(Company company);
}
```

**Características:**

- ✅ Define contrato, não implementação
- ✅ Fica no Core (camada independente)
- ✅ Implementação fica em Infrastructure

---

### 4️⃣ Camada de Infraestrutura - `BancoDeTalentos.Infrastructure/`

**Responsabilidade:** Implementar a persistência e detalhes técnicos.

#### DbContext - Entity Framework

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs
public class BancoDeTalentosDbContext : DbContext
{
    public BancoDeTalentosDbContext(DbContextOptions<BancoDeTalentosDbContext> options)
        : base(options) { }

    // DbSets representam tabelas no banco
    public DbSet<Company> Company { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<Backoffice> Backoffice { get; set; }

    // Configuração de mapeamento
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(
            e => e.HasKey(c => c.Id)
        );

        base.OnModelCreating(modelBuilder);
    }
}
```

#### Repository - Implementação

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    // Injeta o DbContext
    public CompanyRepository(BancoDeTalentosDbContext context)
        => _context = context;

    // Implementa a interface
    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }
}
```

#### InfrastructureModule - Configuração

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/InfrastructureModule.cs
public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddRepositories()
            .AddData(configuration);

        return services;
    }

    // Configuração do banco de dados
    private static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Em desenvolvimento, usa In-Memory
        services.AddDbContext<BancoDeTalentosDbContext>(
            o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
        );

        // Em produção, descomente para usar SQL Server:
        // string dbConnectionString =
        //     configuration.GetConnectionString("SqlConnectionString")!;
        // services.AddDbContext<BancoDeTalentosDbContext>(
        //     o => o.UseSqlServer(dbConnectionString)
        // );

        return services;
    }

    // Registra todos os repositórios
    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        return services;
    }
}
```

---

### 5️⃣ Camada de Testes - `BancoDeTalentos.Tests/`

**Responsabilidade:** Validar que todo o sistema funciona corretamente.

```csharp
// Arquivo: BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs
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
        // Dados de teste
        var newCompanyObject = new
        {
            name = _faker.Company.CompanyName(),
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash()
        };

        // Faz requisição HTTP
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/empresa",
            newCompanyObject
        );

        // Valida resposta
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ResultViewModel<CompanyViewModel?>? result =
            await response.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(newCompanyObject.name, result.Data!.Name);
        Assert.True(result.Data!.Id > 0);
    }
}
```

---

## 🔗 Relacionamento Entre as Camadas

```
┌────────────────────────────────────────┐
│ HTTP Request (POST /api/empresa)       │
└────────────┬─────────────────────────┘
             │
┌────────────▼──────────────────────────────┐
│ BancoDeTalentos.API                       │
│ ├─ CompanyController.Create()             │
│ │  └─ Recebe CreateCompanyModel           │
└────────────┬──────────────────────────────┘
             │ Chama _companyService
             │
┌────────────▼──────────────────────────────────┐
│ BancoDeTalentos.Application                   │
│ ├─ CompanyService.CreateCompany()             │
│ │  ├─ Cria entidade: new Company()            │
│ │  └─ Chama _companyRepository                │
└────────────┬──────────────────────────────────┘
             │
┌────────────▼──────────────────────────────────┐
│ BancoDeTalentos.Infrastructure                │
│ ├─ CompanyRepository.CreateCompany()          │
│ │  ├─ _context.Company.Add(company)           │
│ │  └─ _context.SaveChanges()                  │
└────────────┬──────────────────────────────────┘
             │
┌────────────▼──────────────────────────────────┐
│ In-Memory Database                            │
│ └─ Company table armazena os dados            │
└──────────────────────────────────────────────┘
             │
             ▼ Retorna ID criado

┌──────────────────────────────────────────────┐
│ Retorna CompanyViewModel como JSON           │
└──────────────────────────────────────────────┘
             │
             ▼
┌──────────────────────────────────────────────┐
│ HTTP Response (201 Created)                  │
│ Body: { data: {...}, isSuccess: true }      │
└──────────────────────────────────────────────┘
```

---

## 📝 Convenções de Nomenclatura

| Tipo                 | Convenção             | Exemplo                                |
| -------------------- | --------------------- | -------------------------------------- |
| **Classes**          | PascalCase            | `CompanyService`, `UserViewModel`      |
| **Interfaces**       | I + PascalCase        | `ICompanyService`, `IRepository`       |
| **Métodos**          | PascalCase            | `CreateCompany()`, `GetById()`         |
| **Variáveis locais** | camelCase             | `company`, `companyId`                 |
| **Propriedades**     | PascalCase            | `Name`, `CreatedAt`                    |
| **Private fields**   | \_camelCase           | `_companyRepository`, `_context`       |
| **Constantes**       | UPPER_SNAKE_CASE      | `MAX_ATTEMPTS`, `DEFAULT_TIMEOUT`      |
| **Namespaces**       | PascalCase com pontos | `BancoDeTalentos.Application.Services` |

---

## 🎯 Regra de Ouro

> **Cada arquivo tem uma responsabilidade clara e conhecida.**

- ✅ **Controller** - Receber HTTP, delegar ao Service
- ✅ **Service** - Lógica de negócio, delegar ao Repository
- ✅ **Repository** - Persistência, delegar ao DbContext
- ✅ **Entity** - Representar dados do domínio

Seguindo isso, o código fica simples, testável e mantível.

---

**Referências de Arquivo:**

- `BancoDeTalentos.slnx` - Arquivo da solução
- `BancoDeTalentos.API/BancoDeTalentos.API.csproj` - Dependências da API
- Todos os caminhos indicados acima
