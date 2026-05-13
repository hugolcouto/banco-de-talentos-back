# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 14. Boas Práticas e Convenções do Projeto

## 📌 Padrões de Nomenclatura

### Classes e Interfaces

```csharp
// ✅ CERTO - PascalCase para classes
public class CompanyService
public interface ICompanyRepository
public abstract class BaseEntity

// ❌ ERRADO
public class companyService
public interface companyRepository
```

**Regra:** Interfaces sempre começam com `I` seguido de PascalCase

---

### Propriedades e Campos

```csharp
// ✅ CERTO - camelCase privados, PascalCase públicos
private readonly ICompanyRepository _repository;
public string CompanyName { get; set; }

// ❌ ERRADO
private readonly ICompanyRepository repository;
public string companyName { get; set; }
```

**Regra:**

- Campos privados: `_camelCase` com underscore
- Propriedades públicas: `PascalCase`

---

### Métodos

```csharp
// ✅ CERTO - PascalCase, verbo no início
public void CreateCompany(CreateCompanyModel model)
public Company? GetCompanyById(int id)
public List<Company> GetAllCompanies()
public void UpdateCompany(Company company)
public void DeleteCompany(int id)

// ❌ ERRADO
public void create_company()
public void companyCreate()
```

**Regra:** PascalCase, verbos primeiros (Create, Get, Update, Delete)

---

## 🏗️ Padrões de Arquitetura

### 1. Responsabilidade Única

Cada camada tem uma responsabilidade específica:

```csharp
// ✅ CERTO - Controller apenas orquestra
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    var result = _companyService.CreateCompany(model);
    return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
}

// ✅ CERTO - Service contém regra de negócio
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    if (string.IsNullOrWhiteSpace(model.Name))
        return ResultViewModel<CompanyViewModel>.Error("Nome obrigatório", null);

    var company = new Company(...);
    _repository.CreateCompany(company);

    return ResultViewModel<CompanyViewModel>.Success(CompanyViewModel.FromEntity(company));
}

// ✅ CERTO - Repository apenas persiste
public int CreateCompany(Company company)
{
    _context.Company.Add(company);
    _context.SaveChanges();
    return company.Id;
}

// ❌ ERRADO - Lógica de negócio no Controller
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    if (string.IsNullOrWhiteSpace(model.Name))
        return BadRequest("Nome obrigatório");

    var company = new Company(...);
    _context.Company.Add(company);
    _context.SaveChanges();

    return CreatedAtAction(...);
}
```

---

### 2. Injeção de Dependência

**Sempre usar interface, não implementação:**

```csharp
// ✅ CERTO
public class CompanyService
{
    private readonly ICompanyRepository _repository;

    public CompanyService(ICompanyRepository repository)
        => _repository = repository;
}

// ❌ ERRADO - Acoplamento direto
public class CompanyService
{
    private readonly CompanyRepository _repository;

    public CompanyService()
        => _repository = new CompanyRepository();
}
```

**Registro em módulos:**

```csharp
// ✅ CERTO - No ApplicationModule.cs
services.AddScoped<ICompanyService, CompanyService>();

// ✅ CERTO - No InfrastructureModule.cs
services.AddScoped<ICompanyRepository, CompanyRepository>();
```

---

### 3. Encapsulamento

**Sempre usar private setters nas entidades:**

```csharp
// ✅ CERTO - Imutável após criação
public class Company : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    public Company(string name, string email)
    {
        Name = name;
        Email = email;
    }
}

// ❌ ERRADO - Mutável, sem controle
public class Company : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

---

## 🔄 Padrões de Fluxo de Dados

### Validação em Camadas

```
Request
   ↓
[1] Model Binding (ASP.NET Core - tipo correto?)
   ↓
[2] Controller (status HTTP válido?)
   ↓
[3] Service (regras de negócio - valores válidos?)
   ↓
[4] Repository (persistência)
   ↓
Response
```

**Exemplo prático:**

```csharp
// [1] Model Binding
[HttpPost]
public IActionResult Create(CreateCompanyModel model)  // ASP.NET Core valida tipos
{
    // [2] Controller - validação básica
    if (model == null)
        return BadRequest("Dados inválidos");

    // [3] Service - regras de negócio
    var result = _companyService.CreateCompany(model);

    if (!result.IsSuccess)
        return BadRequest(result);

    return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
}

// [3] Service
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    // Validações de negócio
    if (string.IsNullOrWhiteSpace(model.Name))
        return ResultViewModel<CompanyViewModel>.Error("Nome é obrigatório", null);

    if (model.Name.Length < 3)
        return ResultViewModel<CompanyViewModel>.Error("Nome deve ter no mínimo 3 caracteres", null);

    if (!IsValidDocument(model.Document))
        return ResultViewModel<CompanyViewModel>.Error("Documento inválido", null);

    // [4] Repository - apenas persiste
    var company = new Company(model.Name, model.Document, ...);
    _repository.CreateCompany(company);

    return ResultViewModel<CompanyViewModel>.Success(CompanyViewModel.FromEntity(company));
}
```

---

## 🛡️ Segurança

### 1. Nunca Expor Dados Sensíveis

```csharp
// ✅ CERTO - ViewModel exclui dados sensíveis
public class CompanyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    // ❌ Nunca: public string Password { get; set; }
    // ❌ Nunca: public string Document { get; set; }  // Dado sensível
}

// ❌ ERRADO - Entidade exposta diretamente
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    Company company = _repository.GetCompanyById(id);
    return Ok(company);  // Expõe Password!
}
```

---

## 🗑️ Soft Delete Pattern

Todas as entidades herdam de `BaseEntity`, que possui a propriedade `IsDeleted` e o método `SetAsDeleted()`. A exclusão no projeto é **sempre lógica** -- nunca física. O registro permanece no banco de dados, mas é marcado como deletado e ignorado em consultas públicas.

### Regras Obrigatórias

**Regra 1: Service deve chamar `SetAsDeleted()` antes de invocar o Repository**

```csharp
// ✅ CERTO
public ResultViewModel DeleteJob(int id)
{
    Job? job = _jobRepository.GetJobById(id);
    if (job is null)
        return ResultViewModel.Error("Vaga não encontrada", HttpStatusCode.NotFound);

    job.SetAsDeleted();                            // ← OBRIGATÓRIO
    _jobRepository.DeleteJob(job);
    return ResultViewModel.Sucess();
}

// ❌ ERRADO - Esqueceu SetAsDeleted()
public ResultViewModel DeleteJob(int id)
{
    Job? job = _jobRepository.GetJobById(id);
    if (job is null)
        return ResultViewModel.Error("Vaga não encontrada", HttpStatusCode.NotFound);

    _jobRepository.DeleteJob(job);                 // ← BUG! IsDeleted continua false
    return ResultViewModel.Sucess();
}

// Consequência do ❌:
// 1. DeleteJob() executa sem problemas
// 2. GetJobById(id) após DELETE retorna o registro (IsDeleted = false)
// 3. Teste falha: espera NotFound, recebe OK
```

**Regra 2: Repository Delete deve usar `Update()` (nunca `Remove()`)**

```csharp
// ✅ CERTO - Persiste apenas a mudança de IsDeleted
public void DeleteJob(Job job)
{
    _context.Jobs.Update(job);     // Apenas atualiza IsDeleted = true
    _context.SaveChanges();
}

// ❌ ERRADO - Exclui fisicamente o registro
public void DeleteJob(Job job)
{
    _context.Jobs.Remove(job);     // Deleta a linha do banco
    _context.SaveChanges();
}

// Consequência do ❌:
// 1. Registro some completamente do banco
// 2. Não há histórico de exclusão para auditoria
// 3. Quebra o contrato do projeto (todas as entidades usam soft delete)
```

**Regra 3: Queries públicas devem SEMPRE filtrar `!IsDeleted`**

```csharp
// ✅ CERTO - Filtra ativos apenas
public Job? GetJobById(int id)
{
    return _context.Jobs
        .SingleOrDefault(j => j.Id == id && !j.IsDeleted);
}

public List<Job> GetJobs()
{
    return _context.Jobs
        .Where(j => !j.IsDeleted)
        .ToList();
}

// ❌ ERRADO - Inclui deletados nas consultas
public Job? GetJobById(int id)
{
    return _context.Jobs.Find(id);  // Pode retornar registro com IsDeleted = true
}
```

### Por que Soft Delete?

| Vantagem | Explicação |
|---|---|
| **Auditoria** | Permite saber quem foi deletado e quando (via CreatedAt) |
| **Recuperação** | Dados podem ser restaurados (basta `IsDeleted = false`) |
| **Histórico** | Mantém relacionamentos consistentes (não quebra chaves estrangeiras) |
| **Conformidade** | Algumas regulamentações exigem retenção de dados |

### Checklist de Implementação

Ao implementar Delete, verificar:

- [ ] Service chama `entity.SetAsDeleted()` antes do Repository
- [ ] Repository usa `_context.Update(entity)`, não `_context.Remove(entity)`
- [ ] Repository não recebe `id` como parâmetro -- recebe a entidade já marcada
- [ ] Todos os métodos `Get*` possuem filtro `!entity.IsDeleted`
- [ ] Teste de integração valida: DELETE retorna 204, GET posterior retorna 404

### Troubleshooting Rápido

** Sintoma:** `GET /api/vaga/{id}` retorna 200 OK após ter feito `DELETE /api/vaga/{id}`

**Causa mais provável:** `SetAsDeleted()` não foi chamado no Service antes de `_jobRepository.DeleteJob(job)`

**Verificar:**

1. No Service, linha antes de `_jobRepository.DeleteJob(...)`: existe `job.SetAsDeleted()`?
2. No Repository, método `DeleteJob` usa `Update()` ou `Remove()`?
3. No Repository, método `GetJobById` inclui `!j.IsDeleted` no filtro?

**Correção:**

```csharp
// Service
public ResultViewModel DeleteJob(int id)
{
    Job? job = _jobRepository.GetJobById(id);
    if (job is null) return ResultViewModel.Error("Não encontrado", HttpStatusCode.NotFound);

    job.SetAsDeleted();                  // ← Adicionar esta linha
    _jobRepository.DeleteJob(job);
    return ResultViewModel.Sucess();
}
```

---

### 2. Validação de Entrada

```csharp
// ✅ CERTO
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    // Validar cada campo
    if (string.IsNullOrWhiteSpace(model.Name))
        return ResultViewModel<CompanyViewModel>.Error("Nome obrigatório", null);

    if (string.IsNullOrWhiteSpace(model.Email))
        return ResultViewModel<CompanyViewModel>.Error("Email obrigatório", null);

    if (!IsValidEmail(model.Email))
        return ResultViewModel<CompanyViewModel>.Error("Email inválido", null);

    if (string.IsNullOrWhiteSpace(model.Document))
        return ResultViewModel<CompanyViewModel>.Error("Documento obrigatório", null);

    // Processamento seguro após validação
    ...
}

// ❌ ERRADO - Nenhuma validação
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    var company = new Company(model.Name, model.Email, model.Document);
    _repository.CreateCompany(company);

    return ResultViewModel<CompanyViewModel>.Success(...);
}
```

---

## ✅ Tratamento de Erros

### Usar ResultViewModel para Respostas Consistentes

```csharp
// ✅ CERTO - Respostas consistentes
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    if (id <= 0)
        return ResultViewModel<CompanyViewModel>.Error("ID inválido", null);

    var company = _repository.GetCompanyById(id);

    if (company == null)
        return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada", null);

    return ResultViewModel<CompanyViewModel>.Success(CompanyViewModel.FromEntity(company));
}

// Controller recebe resultado consistente
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var result = _companyService.GetCompanyById(id);

    if (!result.IsSuccess)
        return NotFound(result);

    return Ok(result);
}

// ❌ ERRADO - Exceções soltas
public IActionResult GetById(int id)
{
    try
    {
        Company company = _repository.GetCompanyById(id);
        return Ok(company);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}
```

---

## 🧪 Padrões de Teste

### Arrange-Act-Assert (AAA)

```csharp
[Fact]
public async Task Create_Company_WithValidData_ShouldReturnCreated()
{
    // ARRANGE - Preparar dados
    var model = new CreateCompanyModel
    {
        Name = "Tech Corp",
        Document = "12345678000190",
        Telephone = "(11) 98765-4321",
        Email = "contact@techcorp.com",
        Password = "SecurePassword123"
    };

    // ACT - Executar ação
    var response = await _client.PostAsJsonAsync("/api/empresa", model);

    // ASSERT - Verificar resultado
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

    Assert.NotNull(result?.Data);
    Assert.Equal(model.Name, result.Data!.Name);
    Assert.True(result.Data!.Id > 0);
}
```

### Cobertura Importante

```csharp
// ✅ TESTE: Dados válidos
[Fact]
public void CreateCompany_WithValidData_ShouldSucceed() { }

// ✅ TESTE: Dados inválidos (cada campo)
[Fact]
public void CreateCompany_WithoutName_ShouldFail() { }

[Fact]
public void CreateCompany_WithInvalidEmail_ShouldFail() { }

// ✅ TESTE: Regras de negócio
[Fact]
public void CreateCompany_WithDuplicateDocument_ShouldFail() { }

// ✅ TESTE: Fluxo completo
[Fact]
public async Task CreateCompany_EndToEnd_ShouldPersistAndReturn() { }
```

---

## 📊 Padrões de Banco de Dados

### EF Core

```csharp
// ✅ CERTO - Usar DbSet tipado
public int CreateCompany(Company company)
{
    _context.Company.Add(company);  // Tipado
    _context.SaveChanges();
    return company.Id;
}

// ✅ CERTO - Usar LINQ
var companies = _context.Company
    .Where(c => c.CreatedAt > DateTime.Now.AddDays(-30))
    .OrderBy(c => c.Name)
    .ToList();

// ❌ ERRADO - SQL bruto sem parameterização
var companies = _context.Company
    .FromSqlInterpolated($"SELECT * FROM Company WHERE Name = {userInput}")
    .ToList();
```

### Sempre Use LINQ com Parameterização

```csharp
// ✅ CERTO - Protegido contra SQL Injection
public Company? GetCompanyByEmail(string email)
{
    return _context.Company
        .FirstOrDefault(c => c.Email == email);
}

// ❌ ERRADO - SQL Injection vulnerável
public Company? GetCompanyByEmail(string email)
{
    return _context.Company
        .FromSqlInterpolated($"SELECT * FROM Company WHERE Email = '{email}'")
        .FirstOrDefault();
}
```

---

## 📝 Documentação

### XML Comments em Métodos Públicos

```csharp
// ✅ CERTO - Documentado
/// <summary>
/// Cria uma nova empresa no sistema.
/// </summary>
/// <param name="model">Dados da empresa a ser criada</param>
/// <returns>Resultado contendo a empresa criada ou erro</returns>
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    // implementação
}

// ✅ CERTO - Interface documentada
/// <summary>
/// Serviço de operações com empresas.
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Cria uma nova empresa.
    /// </summary>
    ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model);
}
```

---

## 🚀 Performance

### 1. Usar `async/await` para I/O

```csharp
// ✅ CERTO - Assíncrono
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _companyService.GetCompanyByIdAsync(id);
    return Ok(result);
}

// ❌ ERRADO - Síncrono bloqueia thread
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var result = _companyService.GetCompanyById(id);  // Bloqueia
    return Ok(result);
}
```

### 2. Lazy Loading e Eager Loading

```csharp
// ✅ CERTO - Eager Loading quando necessário
public List<Company> GetCompaniesWithJobs()
{
    return _context.Company
        .Include(c => c.Jobs)  // Carrega relação
        .ToList();
}

// ❌ ERRADO - Lazy Loading causa N+1 queries
public List<Company> GetCompaniesWithJobs()
{
    var companies = _context.Company.ToList();

    // Cada company gera nova query!
    foreach (var company in companies)
    {
        var jobs = company.Jobs;  // Query aqui!
    }

    return companies;
}
```

---

## 🔍 Debugging

### Usar Logging

```csharp
// ✅ CERTO - Logging estruturado
private readonly ILogger<CompanyService> _logger;

public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    _logger.LogInformation("Criando empresa: {CompanyName}", model.Name);

    try
    {
        // implementação
        _logger.LogInformation("Empresa criada com ID: {CompanyId}", company.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao criar empresa: {CompanyName}", model.Name);
        return ResultViewModel<CompanyViewModel>.Error("Erro ao criar empresa", null);
    }
}
```

---

## 📋 Checklist de Implementação

Ao criar um novo recurso, seguir:

- [ ] ✅ Entidade criada em `Core/Entities/`
- [ ] ✅ DTOs criados em `Application/Model/`
- [ ] ✅ Interfaces criadas em `*/Interfaces/`
- [ ] ✅ Service implementado em `Application/Services/`
- [ ] ✅ Repository implementado em `Infrastructure/Persistence/Repositories/`
- [ ] ✅ Dependências registradas em módulos
- [ ] ✅ Controller criado em `API/Controllers/`
- [ ] ✅ Endpoints documentados
- [ ] ✅ Testes criados em `Tests/Integrations/`
- [ ] ✅ Validações implementadas em Service
- [ ] ✅ Dados sensíveis excluídos do ViewModel
- [ ] ✅ Testes passando

---

## 🎯 Resumo

| Aspecto          | Regra                                     |
| ---------------- | ----------------------------------------- |
| **Nomenclatura** | PascalCase públicos, \_camelCase privados |
| **Camadas**      | Cada uma com responsabilidade única       |
| **Dependências** | Via Interface, registradas em módulos     |
| **Entidades**    | Imutáveis com private setters             |
| **DTOs**         | Excluem dados sensíveis                   |
| **Validação**    | Em Service (regras de negócio)            |
| **Respostas**    | Usar ResultViewModel<T>                   |
| **Testes**       | AAA pattern (Arrange-Act-Assert)          |
| **Segurança**    | Validar entrada, proteger dados           |
| **Performance**  | async/await, eager loading                |

---

**Referências de Arquivo:**

- `BancoDeTalentos.API/Controllers/CompanyController.cs`
- `BancoDeTalentos.Application/Services/CompanyService.cs`
- `BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs`
- `BancoDeTalentos.Core/Entities/Company.cs`
- `BancoDeTalentos.Application/Model/CompanyViewModel.cs`
