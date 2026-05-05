# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 8. Camada de Serviços (Application) - Lógica de Negócio

## 💡 O que é a Camada de Serviços?

A **Camada de Serviços** (Service Layer) é responsável por **orquestrar a lógica de negócio**. É o intermediário entre o Controller e o Repository, garantindo que as regras de negócio sejam aplicadas.

### Responsabilidades do Service

✅ Implementar regras de negócio  
✅ Validar dados  
✅ Coordenar múltiplos repositórios  
✅ Transformar dados entre entidades e DTOs  
✅ Conter lógica reutilizável

### O Que NÃO Fazer no Service

❌ Aceitar requisições HTTP direto  
❌ Retornar status HTTP  
❌ Acessar BD diretamente (usar Repository)  
❌ Validações de formato HTTP

---

## 🏗️ Interface de Serviço

```csharp
// Arquivo: BancoDeTalentos.Application/Interfaces/ICompanyService.cs
namespace BancoDeTalentos.Application.Interfaces;

public interface ICompanyService
{
    // Contrato do serviço
    ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model);
    ResultViewModel<CompanyViewModel> GetCompany(int id);
}
```

**Características:**

- ✅ Define contrato (não implementação)
- ✅ Métodos específicos de negócio
- ✅ Retorna ViewModels (DTOs)
- ✅ Permite injeção de dependência

### Por que Interface?

```csharp
// ❌ SEM Interface
public class CompanyService { }
_companyService = new CompanyService();  // Acoplado

// ✅ COM Interface
public interface ICompanyService { }
public class CompanyService : ICompanyService { }
_companyService = // ← Pode ser qualquer implementação

// Benefícios:
// 1. Pode usar mock em testes
// 2. Pode ter múltiplas implementações
// 3. Fácil de trocar sem quebrar código
```

---

## 🔧 Implementação do Service

```csharp
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Exceptions;
using BancoDeTalentos.Application.Model;
using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;

namespace BancoDeTalentos.Application.Services;

public class CompanyService : ICompanyService
{
    // Injeção de dependência do Repository
    private readonly ICompanyRepository _companyRepository;

    // Construtor - recebe dependência
    public CompanyService(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Implementação: Criar Empresa
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        // Passo 1: Validar entrada
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return ResultViewModel<CompanyViewModel>.Error(
                "Nome da empresa é obrigatório",
                null,
                ErrorCode.NOT_FOUND
            );
        }

        if (string.IsNullOrWhiteSpace(model.Document))
        {
            return ResultViewModel<CompanyViewModel>.Error(
                "Documento é obrigatório",
                null,
                ErrorCode.NOT_FOUND
            );
        }

        // Passo 2: Criar entidade de domínio
        // (A entidade é responsável por suas próprias regras)
        Company company = new Company(
            model.Name,
            model.Document,
            model.Telephone,
            model.Email,
            model.Password
        );

        // Passo 3: Persistir usando repositório
        // (Repository sabe como salvar no BD)
        int createdId = _companyRepository.CreateCompany(company);

        // Passo 4: Converter para ViewModel
        // (DTOs para transferir dados entre camadas)
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

        // Passo 5: Retornar resultado padronizado
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Implementação: Obter Empresa por ID
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
    {
        // Passo 1: Validar entrada
        if (id <= 0)
        {
            return ResultViewModel<CompanyViewModel>.Error(
                "ID inválido",
                null,
                ErrorCode.NOT_FOUND
            );
        }

        // Passo 2: Buscar através do repositório
        Company? company = _companyRepository.GetCompanyById(id);

        // Passo 3: Validar resultado
        if (company == null)
        {
            return ResultViewModel<CompanyViewModel>.Error(
                "Empresa não encontrada",
                null,
                ErrorCode.NOT_FOUND
            );
        }

        // Passo 4: Converter para ViewModel
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

        // Passo 5: Retornar resultado
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }
}
```

---

## 📊 Fluxo Passo-a-Passo: CreateCompany

```
Controller recebe CreateCompanyModel
{ name: "Acme", document: "123", telephone: "...", ... }
        │
        ▼
Service.CreateCompany(model)
        │
        ├─► 1️⃣  Validação
        │   if (string.IsNullOrWhiteSpace(model.Name))
        │   └─► Erro: "Nome obrigatório"
        │
        ├─► 2️⃣  Criar Entidade
        │   Company company = new Company(
        │       model.Name,
        │       model.Document,
        │       ...
        │   )
        │
        ├─► 3️⃣  Persistir
        │   _companyRepository.CreateCompany(company)
        │   └─► Repository faz INSERT
        │       └─► Entity Framework atualiza company.Id
        │
        ├─► 4️⃣  Converter para ViewModel
        │   CompanyViewModel.FromEntity(company)
        │   └─► Remove campos sensíveis
        │       └─► Retorna dados públicos
        │
        └─► 5️⃣  Retornar Resultado
            ResultViewModel<CompanyViewModel>.Success(viewModel)
                    │
                    ▼
            JSON Response
            {
              "data": {
                "id": 1,
                "name": "Acme",
                ...
              },
              "isSuccess": true
            }
```

---

## 🎯 Lógica de Negócio no Service

### Exemplo: Validações Complexas

```csharp
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    // Validações de negócio

    // 1. Validar nome
    if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Length < 3)
    {
        return ResultViewModel<CompanyViewModel>.Error(
            "Nome deve ter no mínimo 3 caracteres",
            null
        );
    }

    // 2. Validar CNPJ formato
    if (!IsValidCNPJ(model.Document))
    {
        return ResultViewModel<CompanyViewModel>.Error(
            "CNPJ inválido",
            null
        );
    }

    // 3. Validar email formato
    if (!IsValidEmail(model.Email))
    {
        return ResultViewModel<CompanyViewModel>.Error(
            "Email inválido",
            null
        );
    }

    // 4. Validar senha força
    if (!IsStrongPassword(model.Password))
    {
        return ResultViewModel<CompanyViewModel>.Error(
            "Senha deve ter no mínimo 8 caracteres",
            null
        );
    }

    // Se passou todas as validações, criar
    Company company = new Company(
        model.Name,
        model.Document,
        model.Telephone,
        model.Email,
        HashPassword(model.Password)  // Hash antes de salvar!
    );

    _companyRepository.CreateCompany(company);
    CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);
    return ResultViewModel<CompanyViewModel>.Success(viewModel!);
}

// Helpers de validação
private bool IsValidCNPJ(string cnpj)
{
    // Implementar lógica de validação CNPJ
    return !string.IsNullOrWhiteSpace(cnpj) && cnpj.Length == 14;
}

private bool IsValidEmail(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}

private bool IsStrongPassword(string password)
{
    return !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
}

private string HashPassword(string password)
{
    // Usar BCrypt ou similar em produção
    return System.Security.Cryptography.MD5.Create()
        .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
        .ToString();
}
```

---

## 🔄 Coordenar Múltiplos Repositórios

```csharp
// Futuro: quando houver mais entidades

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
        // Validar que empresa existe
        Company? company = _companyRepository.GetCompanyById(model.CompanyId);
        if (company == null)
        {
            return ResultViewModel<JobViewModel>.Error(
                "Empresa não encontrada",
                null
            );
        }

        // Validações de negócio
        if (model.Salary <= 0)
        {
            return ResultViewModel<JobViewModel>.Error(
                "Salário deve ser positivo",
                null
            );
        }

        // Criar job
        Job job = new Job(...);
        _jobRepository.CreateJob(job);

        // Retornar resultado
        JobViewModel viewModel = JobViewModel.FromEntity(job);
        return ResultViewModel<JobViewModel>.Success(viewModel);
    }
}
```

---

## 🧪 Testando Service

```csharp
// Arquivo: BancoDeTalentos.Tests/Services/CompanyServiceTests.cs
// (Exemplo de teste unitário com mock)

using Moq;
using Xunit;

public class CompanyServiceTests
{
    [Fact]
    public void CreateCompany_WithValidData_ShouldSuccess()
    {
        // Arrange (Preparar)
        var mockRepository = new Mock<ICompanyRepository>();
        var service = new CompanyService(mockRepository.Object);

        var model = new CreateCompanyModel
        {
            Name = "Acme Inc",
            Document = "12345678000100",
            Telephone = "11999999999",
            Email = "contact@acme.com",
            Password = "password123"
        };

        // Act (Agir)
        var result = service.CreateCompany(model);

        // Assert (Validar)
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Acme Inc", result.Data.Name);

        // Verificar que repository foi chamado
        mockRepository.Verify(
            r => r.CreateCompany(It.IsAny<Company>()),
            Times.Once
        );
    }

    [Fact]
    public void CreateCompany_WithoutName_ShouldError()
    {
        // Arrange
        var mockRepository = new Mock<ICompanyRepository>();
        var service = new CompanyService(mockRepository.Object);

        var model = new CreateCompanyModel
        {
            Name = "",  // ← Inválido
            Document = "12345678000100",
            Telephone = "11999999999",
            Email = "contact@acme.com",
            Password = "password123"
        };

        // Act
        var result = service.CreateCompany(model);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotEmpty(result.Message);

        // Repository não deve ser chamado
        mockRepository.Verify(
            r => r.CreateCompany(It.IsAny<Company>()),
            Times.Never
        );
    }
}
```

---

## 📝 ApplicationModule - Registrar Services

```csharp
// Arquivo: BancoDeTalentos.Application/ApplicationModule.cs
using Microsoft.Extensions.DependencyInjection;

namespace BancoDeTalentos.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        // Registra interface com implementação
        // Ciclo Scoped = uma instância por requisição HTTP
        services.AddScoped<ICompanyService, CompanyService>();

        // Futuro:
        // services.AddScoped<IJobService, JobService>();
        // services.AddScoped<ICandidateService, CandidateService>();

        return services;
    }
}
```

---

## 🎯 Boas Práticas

1. ✅ **Implementar Interface** - Facilita testes e DI
2. ✅ **Receber dependências no construtor** - Injeção clara
3. ✅ **Validações primeiro** - Retornar erro cedo
4. ✅ **Usar Entities e DTOs** - Não misturar camadas
5. ✅ **ResultViewModel sempre** - Resposta padronizada
6. ✅ **Métodos específicos** - `CreateCompany()`, não `Create()`
7. ✅ **Sem acesso direto ao BD** - Sempre via Repository
8. ✅ **Regras de negócio aqui** - Não no Entity ou Repository

---

## ❌ Anti-Padrões

```csharp
// ❌ NÃO FAZER: Service fazendo chamadas HTTP
public class BadService
{
    public void CreateCompany(Company company)
    {
        // Responsabilidade de Controller!
        var response = new HttpResponseMessage();
    }
}

// ❌ NÃO FAZER: Service acessando BD direto
public class BadService
{
    public void CreateCompany(Company company)
    {
        _context.Company.Add(company);  // Sem Repository!
        _context.SaveChanges();
    }
}

// ❌ NÃO FAZER: Service sem validações
public class BadService
{
    public void CreateCompany(CreateCompanyModel model)
    {
        Company company = new Company(
            model.Name,  // Pode ser null!
            model.Document,
            ...
        );
        _repository.CreateCompany(company);
    }
}
```

---

## 📊 Service vs Controller vs Repository

| Aspecto       | Controller            | Service         | Repository     |
| ------------- | --------------------- | --------------- | -------------- |
| **Recebe**    | HTTP                  | DTO             | Entity         |
| **Faz**       | Rotear                | Validar         | Persistir      |
| **Retorna**   | HTTP Response         | ResultViewModel | int/Entity     |
| **Conhece**   | HTTP                  | Regras negócio  | BD             |
| **Testa com** | WebApplicationFactory | Mock Repository | Mock DbContext |

---

**Referências de Arquivo:**

- `BancoDeTalentos.Application/Interfaces/ICompanyService.cs`
- `BancoDeTalentos.Application/Services/CompanyService.cs`
- `BancoDeTalentos.Application/ApplicationModule.cs`
- `BancoDeTalentos.API/Controllers/CompanyController.cs` (usar service)
