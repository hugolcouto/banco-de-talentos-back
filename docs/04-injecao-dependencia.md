# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 4. Injeção de Dependência (DI)

## 💡 O que é Injeção de Dependência?

**Injeção de Dependência** (Dependency Injection) é um padrão de design que **fornece as dependências de uma classe ao invés de ela mesma criá-las**. Isso promove baixo acoplamento e facilita testes.

### Problema Sem DI

```csharp
// ❌ SEM Injeção de Dependência (RUIM)
public class CompanyService
{
    private CompanyRepository _repository;

    public CompanyService()
    {
        // Cria a dependência internamente
        _repository = new CompanyRepository();
    }

    public void CreateCompany(Company company)
    {
        _repository.CreateCompany(company);
    }
}

// Problemas:
// 1. Altamente acoplado - impossível testar sem banco real
// 2. Difícil de mudar implementação
// 3. Cada instância cria uma nova dependência
// 4. Impossível fazer mock em testes
```

### Solução Com DI

```csharp
// ✅ COM Injeção de Dependência (BOM)
public class CompanyService
{
    private readonly ICompanyRepository _repository;

    // Recebe a dependência via construtor
    public CompanyService(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public void CreateCompany(Company company)
    {
        _repository.CreateCompany(company);
    }
}

// Benefícios:
// 1. Baixo acoplamento - depende de interface
// 2. Fácil de testar - pode receber mock
// 3. Flexível - trocar implementação sem mudanças
// 4. Uma só responsabilidade
```

---

## 🔧 Como Funciona No Projeto

### Passo 1: Definir Interface (Core)

```csharp
// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
namespace BancoDeTalentos.Core.Interfaces;

public interface ICompanyRepository
{
    int CreateCompany(Company company);
}
```

**Características:**

- Interface fica no Core (camada independente)
- Define o contrato, não implementação
- Qualquer classe pode implementar

### Passo 2: Implementar Interface (Infrastructure)

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
namespace BancoDeTalentos.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    // Recebe o DbContext via DI
    public CompanyRepository(BancoDeTalentosDbContext context)
        => _context = context;

    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }
}
```

### Passo 3: Usar em Service (Application)

```csharp
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
namespace BancoDeTalentos.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    // Recebe a interface via construtor
    public CompanyService(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        Company company = new Company(
            model.Name,
            model.Document,
            model.Telephone,
            model.Email,
            model.Password
        );

        _companyRepository.CreateCompany(company);
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }
}
```

### Passo 4: Usar em Controller (API)

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
namespace BancoDeTalentos.API.Controllers;

[Route("api/empresa")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    // Recebe a interface via construtor
    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        ResultViewModel<CompanyViewModel> result =
            _companyService.CreateCompany(model);

        return CreatedAtAction(nameof(GetById),
            new { id = result.Data?.Id }, result);
    }
}
```

### Passo 5: Registrar no Container de DI

#### Application Module

```csharp
// Arquivo: BancoDeTalentos.Application/ApplicationModule.cs
using Microsoft.Extensions.DependencyInjection;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Services;

namespace BancoDeTalentos.Application;

public static class ApplicationModule
{
    // Método de extensão para registrar tudo da Application
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        // Registra: quando alguém pedir ICompanyService,
        // fornece uma instância de CompanyService
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
```

#### Infrastructure Module

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/InfrastructureModule.cs
using BancoDeTalentos.Core.Interfaces;
using BancoDeTalentos.Infrastructure.Persistence;
using BancoDeTalentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BancoDeTalentos.Infrastructure;

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

    // Registra todos os repositórios
    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        // Registra: quando alguém pedir ICompanyRepository,
        // fornece uma instância de CompanyRepository
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }

    // Configura o banco de dados
    private static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registra o DbContext
        services.AddDbContext<BancoDeTalentosDbContext>(
            o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
        );

        return services;
    }
}
```

#### Program.cs - Orquestrador Central

```csharp
// Arquivo: BancoDeTalentos.API/Program.cs
using BancoDeTalentos.Application;
using BancoDeTalentos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 🔑 CENTRAL: Registra tudo via módulos
builder.Services
    .AddApplication()                          // Registra services
    .AddInfrastructure(builder.Configuration); // Registra repositories

// Adiciona controllers (ASP.NET Core)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

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

## 📊 Fluxo de Registro e Resolução

### Registro (Startup)

```
Program.cs chama:
    ↓
builder.Services.AddApplication()
    ↓
ApplicationModule.AddApplication()
    ↓
ApplicationModule.AddServices()
    ↓
services.AddScoped<ICompanyService, CompanyService>()
    ↓
Container DI: "Quando pedirmos ICompanyService, dê CompanyService"

builder.Services.AddInfrastructure(configuration)
    ↓
InfrastructureModule.AddInfrastructure()
    ↓
InfrastructureModule.AddRepositories()
    ↓
services.AddScoped<ICompanyRepository, CompanyRepository>()
    ↓
Container DI: "Quando pedirmos ICompanyRepository, dê CompanyRepository"
```

### Resolução (Requisição HTTP)

```
POST /api/empresa
    ↓
ASP.NET Core precisa criar CompanyController
    ↓
Vê que CompanyController precisa de ICompanyService
    ↓
Container fornece CompanyService
    ↓
CompanyService precisa de ICompanyRepository
    ↓
Container fornece CompanyRepository
    ↓
CompanyRepository precisa de BancoDeTalentosDbContext
    ↓
Container fornece DbContext
    ↓
Tudo instanciado e conectado ✅
    ↓
Request é processado
```

---

## 🔄 Ciclos de Vida

O ASP.NET Core oferece 3 ciclos de vida para dependências:

### 1️⃣ **Transient** - Nova instância sempre

```csharp
services.AddTransient<IMyService, MyService>();
```

- **Quando usar:** Stateless services, geradores de dados temporários
- **Comportamento:** Cada vez que a dependência é requisitada, uma nova instância é criada
- **Performance:** Mais leve, mas cria mais objetos

**Exemplo:**

```csharp
// No Controller 1
IMyService service1 = serviceProvider.GetService<IMyService>();

// No Controller 2
IMyService service2 = serviceProvider.GetService<IMyService>();

// service1 !== service2 (instâncias diferentes)
```

### 2️⃣ **Scoped** - Uma por requisição HTTP (PADRÃO DO PROJETO)

```csharp
services.AddScoped<ICompanyService, CompanyService>();
```

- **Quando usar:** Services com estado, repositories, DB contexts
- **Comportamento:** Uma instância por requisição HTTP
- **Performance:** Equilibrado, evita estado compartilhado entre requisições

**Exemplo:**

```csharp
// Dentro da mesma requisição HTTP
ICompanyRepository repo1 = serviceProvider.GetService<ICompanyRepository>();
ICompanyRepository repo2 = serviceProvider.GetService<ICompanyRepository>();

// repo1 === repo2 (mesma instância dentro da requisição)

// Em outra requisição HTTP
ICompanyRepository repo3 = serviceProvider.GetService<ICompanyRepository>();

// repo1 !== repo3 (instâncias diferentes entre requisições)
```

### 3️⃣ **Singleton** - Mesma instância sempre

```csharp
services.AddSingleton<IMyService, MyService>();
```

- **Quando usar:** Cache, configurações globais, logger
- **Comportamento:** A mesma instância por toda a vida da aplicação
- **Performance:** Mais rápido, mas compartilha estado entre requisições

**Exemplo:**

```csharp
// Em qualquer lugar, qualquer hora
IMyService service1 = serviceProvider.GetService<IMyService>();
IMyService service2 = serviceProvider.GetService<IMyService>();

// service1 === service2 (mesma instância sempre)
```

### Comparação

| Ciclo         | Instâncias   | Melhor Para            | Exemplo                             |
| ------------- | ------------ | ---------------------- | ----------------------------------- |
| **Transient** | Nova sempre  | Stateless, geradores   | Logger (pode criar vários)          |
| **Scoped**    | Uma por HTTP | Services, Repositories | CompanyService, CompanyRepository   |
| **Singleton** | Uma sempre   | Cache, Config          | IConfiguration, Logger centralizado |

### No Projeto

```csharp
// Arquivo: BancoDeTalentos.Application/ApplicationModule.cs
services.AddScoped<ICompanyService, CompanyService>();
// ✅ Scoped: una nova instância por requisição HTTP

// Arquivo: BancoDeTalentos.Infrastructure/InfrastructureModule.cs
services.AddScoped<ICompanyRepository, CompanyRepository>();
// ✅ Scoped: uma nova instância por requisição HTTP

services.AddDbContext<BancoDeTalentosDbContext>(
    o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
);
// ✅ Scoped (padrão do DbContext): uma nova instância por requisição
```

---

## 🧪 Testando Com Dependency Injection

A grande vantagem da DI é facilitar testes com mocks:

```csharp
// Arquivo: BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs
public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Faker _faker;

    public CompanyControllerTests(WebApplicationFactory<Program> factory)
    {
        // Factory cria uma instância completa da aplicação
        // Mas com InMemory Database ao invés de SQL Server
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

        // Faz uma requisição HTTP real através da aplicação
        // Mas com dependências mock (InMemory DB)
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/empresa",
            newCompanyObject
        );

        // Valida
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync
            <ResultViewModel<CompanyViewModel?>>();
        Assert.NotNull(result?.Data);
    }
}
```

**Como funciona:**

1. `WebApplicationFactory<Program>` cria toda a aplicação
2. Tudo é registrado normalmente via módulos
3. DbContext usa InMemory ao invés de SQL Server (veja `InfrastructureModule.cs`)
4. Testes rodamcom aplicação real, mas BD em memória
5. Cada teste é isolado (novo DbContext)

---

## 🎯 Resumo

| Conceito          | Descrição               | Benefício                    |
| ----------------- | ----------------------- | ---------------------------- |
| **Interface**     | Contrato                | Define o que usar            |
| **Implementação** | Código real             | Cumpre o contrato            |
| **Injeção**       | Fornecer via construtor | Baixo acoplamento            |
| **Módulo**        | Grupo de registros      | Organização                  |
| **Container**     | Gerenciador de DI       | Cria instâncias              |
| **Scoped**        | Uma por requisição      | Isolamento entre requisições |

---

## 📝 Checklist - Aplicando DI Corretamente

- ✅ Interface fica em `Core/Interfaces/`
- ✅ Implementação fica em `Infrastructure/`
- ✅ Service recebe interface via construtor
- ✅ Nenhum `new` direto de implementação concreta
- ✅ Registrar em módulo (`ApplicationModule`, `InfrastructureModule`)
- ✅ Chamar módulo em `Program.cs`
- ✅ Usar `Scoped` para Services e Repositories
- ✅ Testar com `WebApplicationFactory`

---

**Referências de Arquivo:**

- `BancoDeTalentos.API/Program.cs` - Orquestração central
- `BancoDeTalentos.Application/ApplicationModule.cs` - Registro de services
- `BancoDeTalentos.Infrastructure/InfrastructureModule.cs` - Registro de repositories
- `BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs` - Teste com DI
