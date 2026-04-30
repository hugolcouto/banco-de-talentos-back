# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 7. Padrão Repository

## 💡 O que é o Padrão Repository?

O **Padrão Repository** é um padrão de design que **abstrai o acesso aos dados**. Ele funciona como um intermediário entre a lógica de negócio e a camada de persistência (banco de dados).

### Analogia Real

```
Sem Repository:
┌───────────────┐
│   Service     │ ← Conhecido de como salvar no BD
│               │ ← Conhece SQL, Entity Framework
└───────────────┘

Com Repository:
┌───────────────┐
│   Service     │ ← Não sabe detalhes
│               │ ← Apenas pede para Repository
└────────┬──────┘
         │ Pede: "Salva essa empresa"
         ▼
┌───────────────────┐
│   Repository      │ ← Conhece como salvar
│   (Abstração)     │ ← Conhece BD
└──────┬────────────┘
       │ Como? Usa SQL Server, MongoDB, etc
       ▼
   [Database]
```

---

## 🏗️ Interface do Repository

```csharp
// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
namespace BancoDeTalentos.Core.Interfaces;

public interface ICompanyRepository
{
    int CreateCompany(Company company);
}
```

**Características:**

- ✅ Fica no **Core** (camada independente)
- ✅ Define **contrato** (não implementação)
- ✅ Específica para Company (Interface Segregation)
- ✅ Métodos representam **operações de negócio**, não CRUD genérico

### Por Que Fica no Core?

```
Se fica em Infrastructure:
❌ Service dependeria de Infrastructure
❌ Violaria a Arquitetura Limpa

Se fica em Core:
✅ Interface fica pura (sem framework)
✅ Infrastructure implementa
✅ Service depende apenas de abstração
```

---

## 🔧 Implementação do Repository

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
using BancoDeTalentos.Core.Entities;
using BancoDeTalentos.Core.Interfaces;

namespace BancoDeTalentos.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    // Recebe DbContext via injeção de dependência
    public CompanyRepository(BancoDeTalentosDbContext context)
        => _context = context;

    // Implementa a interface
    public int CreateCompany(Company company)
    {
        // 1. Adiciona à coleção em memória
        _context.Company.Add(company);

        // 2. Persiste no banco de dados
        _context.SaveChanges();

        // 3. Retorna o ID gerado
        return company.Id;
    }
}
```

**Passo a Passo:**

```csharp
public int CreateCompany(Company company)
{
    // Passo 1: Add()
    // Adiciona company ao DbSet (coleção em memória do Entity Framework)
    _context.Company.Add(company);

    // Entity Framework agora rastreia essa entidade
    // Sabe que é nova (não estava no BD antes)

    // Passo 2: SaveChanges()
    // Executa INSERT no banco de dados
    // Entity Framework:
    // - Gera SQL apropriado
    // - Executa no BD
    // - Atualiza o Id da entidade (gerado pelo BD)
    _context.SaveChanges();

    // Passo 3: Retorna Id
    // company.Id agora tem o ID gerado pelo BD
    return company.Id;
}
```

---

## 📊 Benefícios do Padrão Repository

### 1. Abstração de Dados

```csharp
// ❌ SEM Repository
public class BadService
{
    private BancoDeTalentosDbContext _context;

    public void CreateCompany(CreateCompanyModel model)
    {
        Company company = new Company(...);
        _context.Company.Add(company);  // ← Service conhece Entity Framework!
        _context.SaveChanges();
    }
}

// ✅ COM Repository
public class GoodService
{
    private ICompanyRepository _repository;

    public void CreateCompany(CreateCompanyModel model)
    {
        Company company = new Company(...);
        _repository.CreateCompany(company);  // ← Service não conhece EF
    }
}
```

**Benefício:** Service desacoplado de framework

### 2. Trocar Implementação Sem Impacto

```csharp
// Hoje usamos SQL Server
public class CompanyRepository : ICompanyRepository
{
    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }
}

// Amanhã podemos trocar para MongoDB sem alterar Service
public class CompanyMongoRepository : ICompanyRepository
{
    public int CreateCompany(Company company)
    {
        _mongoCollection.InsertOne(company);
        return company.Id;
    }
}

// No Program.cs, apenas trocar:
// services.AddScoped<ICompanyRepository, CompanyRepository>();
// Para:
// services.AddScoped<ICompanyRepository, CompanyMongoRepository>();
```

**Benefício:** Flexibilidade total

### 3. Facilita Testes

```csharp
// ❌ SEM Repository (impossível testar sem BD)
[Fact]
public void CreateCompany_WithoutRepository()
{
    var service = new BadService(new BancoDeTalentosDbContext());  // ← Precisa BD real!
    var result = service.CreateCompany(new CreateCompanyModel());
    // Teste lento e frágil
}

// ✅ COM Repository (pode usar mock)
[Fact]
public void CreateCompany_WithRepository()
{
    // Mock do repository
    var mockRepository = new Mock<ICompanyRepository>();
    var service = new CompanyService(mockRepository.Object);

    // Teste rápido e isolado
    var result = service.CreateCompany(new CreateCompanyModel());

    // Valida que repository foi chamado
    mockRepository.Verify(r => r.CreateCompany(It.IsAny<Company>()), Times.Once);
}
```

**Benefício:** Testes rápidos e confiáveis

---

## 🔄 Fluxo Completo com Repository

```
POST /api/empresa
    ↓
┌─────────────────────────────────┐
│   CompanyController             │
│   - Recebe CreateCompanyModel   │
│   - Chama _companyService       │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   CompanyService                │
│   - Cria entidade Company       │
│   - Chama _repository           │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   CompanyRepository             │
│   - Adiciona ao DbSet           │
│   - Executa SaveChanges()       │
│   - Retorna Id                  │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   Entity Framework              │
│   - Gera INSERT SQL             │
│   - Executa no BD               │
│   - Atualiza Id                 │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   In-Memory Database            │
│   - Armazena registro           │
└──────────────┬──────────────────┘
               │
               ▼
   Retorna Id → Service → Controller → Response (JSON)
```

---

## 📝 Exemplo Prático: Usar Repository

### Criando Empresa

```csharp
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        // 1. Cria entidade
        Company company = new Company(
            model.Name,
            model.Document,
            model.Telephone,
            model.Email,
            model.Password
        );

        // 2. Usa repository (não sabe como salva, só sabe que salva)
        int createdId = _companyRepository.CreateCompany(company);

        // 3. Depois pode buscar ou usar company.Id
        // Entity Framework atualizou company.Id automaticamente

        // 4. Converte para ViewModel
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

        // 5. Retorna resultado padronizado
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }
}
```

### Usando no Controller

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
[Route("api/empresa")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        // Service (que usa Repository) cria a empresa
        ResultViewModel<CompanyViewModel> companyResult =
            _companyService.CreateCompany(model);

        // Retorna 201 Created com localização
        return CreatedAtAction(
            nameof(GetById),
            new { id = companyResult.Data?.Id },
            companyResult
        );
    }
}
```

---

## 🛠️ Estendendo Repository

Se precisar de mais operações:

```csharp
// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
public interface ICompanyRepository
{
    // Criar
    int CreateCompany(Company company);

    // Ler
    Company? GetCompanyById(int id);
    List<Company> GetAllCompanies();

    // Atualizar
    void UpdateCompany(Company company);

    // Deletar
    void DeleteCompany(int id);
}
```

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public CompanyRepository(BancoDeTalentosDbContext context)
        => _context = context;

    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }

    public Company? GetCompanyById(int id)
    {
        return _context.Company.Find(id);
    }

    public List<Company> GetAllCompanies()
    {
        return _context.Company.ToList();
    }

    public void UpdateCompany(Company company)
    {
        _context.Company.Update(company);
        _context.SaveChanges();
    }

    public void DeleteCompany(int id)
    {
        var company = _context.Company.Find(id);
        if (company != null)
        {
            _context.Company.Remove(company);
            _context.SaveChanges();
        }
    }
}
```

---

## 🎯 Boas Práticas

1. ✅ **Uma Interface por Entidade** - `ICompanyRepository`, não `IRepository`
2. ✅ **Métodos Significativos** - `CreateCompany()`, não `Add()`
3. ✅ **Sem Queries Complexas** - Repository é simples, Service orquestra
4. ✅ **Sem Async (por enquanto)** - Simplicidade primeiro
5. ✅ **Registrar em Module** - Centralizado em `InfrastructureModule.cs`
6. ✅ **Testar com Mock** - Repository é testável

---

## ❌ Anti-Padrões

```csharp
// ❌ NÃO FAZER: Repository genérico
public interface IRepository<T>
{
    void Add(T entity);
    void Remove(T entity);
    // Muito genérico, perde semântica
}

// ❌ NÃO FAZER: Queries no Service
public class BadService
{
    public void CreateCompany(Company company)
    {
        // Conhece detalhes de persistência
        _context.Company.Add(company);
        _context.SaveChanges();
    }
}

// ❌ NÃO FAZER: Lógica complexa no Repository
public class BadRepository
{
    public List<Company> GetCompaniesAndCandidatesAndJobs()
    {
        // Muita responsabilidade
        // Use Service para orquestrar
    }
}
```

---

## 📊 Repository vs Service

| Aspecto              | Repository     | Service           |
| -------------------- | -------------- | ----------------- |
| **Responsabilidade** | CRUD           | Lógica de negócio |
| **Localização**      | Infrastructure | Application       |
| **Conhece**          | BD, EF         | Entidades, regras |
| **Chama**            | DbContext      | Repository        |
| **Chamado por**      | Service        | Controller        |
| **Testa com**        | Mock           | Mock Repository   |

---

**Referências de Arquivo:**

- `BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs`
- `BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs`
- `BancoDeTalentos.Application/Services/CompanyService.cs`
- `BancoDeTalentos.Infrastructure/InfrastructureModule.cs`
