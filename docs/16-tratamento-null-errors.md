# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 16. Tratamento de Null e Prevenção de NullReferenceException

## 🚨 O Erro: "Object reference not set to an instance of an object"

Este é um dos erros mais comuns em C#. Significa que você está tentando acessar uma propriedade ou método de um objeto que é `null`.

```
System.NullReferenceException: Object reference not set to an instance of an object.
```

---

## 🔍 Como Este Erro Ocorreu no Projeto

### Cenário Real - CompanyService.cs

**Versão com Erro:**

```csharp
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);

    // ❌ PROBLEMA 1: Sem return
    if (company is null)
        return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

    // ❌ PROBLEMA 2: company pode ser null aqui!
    return ResultViewModel<CompanyViewModel>.Success(
        CompanyViewModel.FromEntity(company)!
    );
}
```

**Fluxo da Falha:**

1. **Linha 1:** `GetCompanyById(id)` é chamado com um ID inexistente
2. **Linha 2:** `company = null` (repositório não encontra a empresa)
3. **Linha 5:** `if (company is null)` → **TRUE**
4. **Linha 6:** Executa `ResultViewModel.Error(...)` mas **NÃO RETORNA**
5. **Linha 8:** Continua a execução (erro lógico!)
6. **Linha 9:** Tenta executar `CompanyViewModel.FromEntity(company)` onde `company = null`
7. **Dentro de FromEntity** (linha 25 de CompanyViewModel.cs):
   ```csharp
   entity!.Id  // ❌ entity é null! NullReferenceException
   ```

---

## 📋 Padrões Problemáticos a Evitar

### Padrão 1: Validação Sem Return

```csharp
// ❌ ERRADO - Sem return
if (company is null)
    ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

// Código continua aqui com company = null!
company.SetAsDeleted();  // 💣 NullReferenceException
```

**Versão Correta:**

```csharp
// ✅ CERTO - Com return
if (company is null)
    return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

// Código só chega aqui se company != null
company.SetAsDeleted();  // ✅ Seguro
```

---

### Padrão 2: Null-Forgiving Operator Sem Validação

```csharp
// ❌ ERRADO - ! (null-forgiving) sem verificação
public static CompanyViewModel? FromEntity(Company? entity)
    => new CompanyViewModel(
        entity!.Id,           // Se entity for null → Erro!
        entity.Name,
        entity.Telephone,
        entity.Email,
        entity.About
    );
```

**O que o `!` faz:**

- Diz ao compilador: "Confio que não é null"
- **NÃO valida em runtime** - apenas silencia avisos do compilador
- Se for null, erro na execução

**Versão Correta:**

```csharp
// ✅ CERTO - Valida antes de usar
public static CompanyViewModel? FromEntity(Company? entity)
{
    if (entity is null)
        return null;

    return new CompanyViewModel(
        entity.Id,            // ✅ Seguro - já validou
        entity.Name,
        entity.Telephone,
        entity.Email,
        entity.About
    );
}
```

Ou com ternário:

```csharp
// ✅ CERTO - Ternário com validação
public static CompanyViewModel? FromEntity(Company? entity)
    => entity is null
        ? null
        : new CompanyViewModel(
            entity.Id,
            entity.Name,
            entity.Telephone,
            entity.Email,
            entity.About
        );
```

---

### Padrão 3: Método Retornando Null Sem Documentação

```csharp
// ❌ PROBLEMÁTICO - Não está claro que pode retornar null
public Company? GetCompanyById(int id)
{
    return _context.Company.FirstOrDefault(c => c.Id == id);
}

// Uso arriscado
var company = repository.GetCompanyById(999);
company.SetAsDeleted();  // 💣 Se company é null!
```

**Versão Correta:**

```csharp
// ✅ CERTO - Documentação clara + validação no uso
/// <summary>
/// Obtém uma empresa pelo ID.
/// </summary>
/// <param name="id">ID da empresa</param>
/// <returns>Company se encontrada, null caso contrário</returns>
public Company? GetCompanyById(int id)
{
    return _context.Company.FirstOrDefault(c => c.Id == id);
}

// Uso seguro
var company = repository.GetCompanyById(id);
if (company is null)
    return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

company.SetAsDeleted();  // ✅ Seguro - já validou
```

---

## ✅ Soluções Recomendadas

### Solução 1: Guard Clauses (Recomendado)

Guard clauses são validações no início do método que retornam cedo.

```csharp
// ✅ MELHOR - Guard clauses
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    // Guard 1: Validar entrada
    if (id <= 0)
        return ResultViewModel<CompanyViewModel>.Error("ID inválido");

    // Guard 2: Validar resultado
    Company? company = _companyRepository.GetCompanyById(id);
    if (company is null)
        return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada");

    // Aqui temos certeza que company != null
    return ResultViewModel<CompanyViewModel>.Success(
        CompanyViewModel.FromEntity(company)!
    );
}
```

**Benefícios:**

- Código mais legível
- Reduz aninhamento
- Falhas claras e cedo

---

### Solução 2: Usar Padrão de Null Coalescing

```csharp
// ✅ Para valores padrão
public string GetCompanyName(int id)
{
    var company = _repository.GetCompanyById(id);
    return company?.Name ?? "Sem nome";  // Se null, retorna "Sem nome"
}
```

**Operadores:**

- `??` (null coalescing) - usa valor alternativo se null
- `?.` (null-conditional) - acessa propriedade apenas se não null

---

### Solução 3: Try-Catch para Erros Inesperados

```csharp
// ✅ Para operações que podem falhar
public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
{
    try
    {
        // Validar entrada
        if (model is null)
            return ResultViewModel<CompanyViewModel>.Error("Dados obrigatórios");

        // Criar entidade
        var company = new Company(...);
        _repository.CreateCompany(company);

        return ResultViewModel<CompanyViewModel>.Success(
            CompanyViewModel.FromEntity(company)!
        );
    }
    catch (NullReferenceException ex)
    {
        // Log do erro
        _logger.LogError($"NullReferenceException em CreateCompany: {ex.Message}");
        return ResultViewModel<CompanyViewModel>.Error("Erro ao criar empresa");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Erro inesperado: {ex.Message}");
        return ResultViewModel<CompanyViewModel>.Error("Erro interno");
    }
}
```

---

## 🛡️ Estratégias de Prevenção

### 1. Enable Nullable Reference Types

No `.csproj`:

using System.Net;

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
        return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);
```

**O que faz:**
return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

- Compilador avisa sobre uso de null
- `string` = não-nulo obrigatório
- `string?` = pode ser null
  return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

---

### 2. Validar Antes de Usar

```csharp
// ✅ PADRÃO RECOMENDADO
public ResultViewModel UpdateCompany(int id, UpdateCompanyModel model)
{
    // 1. Validar entrada
    if (model is null)
        return ResultViewModel.Error("Dados obrigatórios", HttpStatusCode.BadRequest);

    // 2. Recuperar recurso
    Company? company = _companyRepository.GetCompanyById(id);

    // 3. Validar se existe
    if (company is null)
        return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

    // 4. Agora é seguro usar company
    company.Update(model.Name, model.Email);
    _companyRepository.UpdateCompany(company);

    return ResultViewModel.Sucess();
}
```

---

### 3. Usar Métodos Defensivos

```csharp
// ❌ Arriscado
var companies = _repository.GetCompanies();
var count = companies.Count();  // Se companies é null?

// ✅ Defensivo
var companies = _repository.GetCompanies() ?? new List<Company>();
var count = companies.Count();  // Seguro

// ✅ Ou com validação
var companies = _repository.GetCompanies();
if (companies is null || companies.Count == 0)
    return ResultViewModel<List<CompanyViewModel>>.Success(new List<CompanyViewModel>());

var model = companies.Select(CompanyViewModel.FromEntity).ToList();
return ResultViewModel<List<CompanyViewModel>>.Success(model);
```

---

### 4. Factory Methods com Validação

```csharp
// ✅ Factory method é mais seguro que construtor
public class Company : BaseEntity
{
    // Construtor privado
    private Company(string name, string email)
    {
        Name = name;
        Email = email;
    }

    // Factory method com validações
    public static Company? Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        if (string.IsNullOrWhiteSpace(email))
            return null;

        return new Company(name, email);
    }
}

// Uso seguro
var company = Company.Create(model.Name, model.Email);
if (company is null)
    return ResultViewModel.Error("Dados inválidos", HttpStatusCode.BadRequest);

_repository.CreateCompany(company);  // ✅ Seguro
```

---

## 🔧 Ferramentas de Debug

### 1. Visual Studio Debugger

```csharp
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);
    // ☝️ Clique no gutter (lado esquerdo) para breakpoint

    if (company is null)
        return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada");

    return ResultViewModel<CompanyViewModel>.Success(
        CompanyViewModel.FromEntity(company)!
    );
}
```

**Como usar:**

1. Clique no gutter (número da linha)
2. Pressione F5 para debug
3. Ao atingir o breakpoint, veja o valor de `company` na janela Locals

---

### 2. Null Coalescing para Debug

```csharp
// Temporário - para entender o fluxo
var company = _companyRepository.GetCompanyById(id);
System.Diagnostics.Debug.WriteLine($"Company: {company?.Name ?? "NULL"}");

if (company is null)
    return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada");
```

---

### 3. Exception Handler Middleware

Em `Middleware/ApiExceptionHandler.cs`:

```csharp
public class ApiExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(RequestDelegate next, ILogger<ApiExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NullReferenceException ex)
        {
            _logger.LogError($"NullReferenceException: {ex.StackTrace}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Erro interno do servidor" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}
```

---

## 📝 Checklist para Code Review

Ao revisar código, verificar:

- [ ] Todas as validações com `if (x is null)` têm `return`?
- [ ] Métodos que retornam tipos nullable (`?`) têm documentação?
- [ ] Null-forgiving operator (`!`) tem justificativa válida?
- [ ] Listas/arrays são inicializados com `??` ou validados?
- [ ] Factory methods validam entradas antes de retornar?
- [ ] Exception handling existe para NullReferenceException?
- [ ] Testes cobrem cenários de null?

---

## 🧪 Testes para Null Handling

```csharp
[TestClass]
public class CompanyServiceTests
{
    private CompanyService _service;
    private Mock<ICompanyRepository> _mockRepository;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ICompanyRepository>();
        _service = new CompanyService(_mockRepository.Object);
    }

    [TestMethod]
    public void GetCompanyById_WithInvalidId_ReturnsError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetCompanyById(It.IsAny<int>()))
            .Returns((Company?)null);

        // Act
        var result = _service.GetCompanyById(999);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Empresa não encontrada", result.Message);
    }

    [TestMethod]
    public void GetCompanyById_WithValidId_ReturnsCompany()
    {
        // Arrange
        var company = new Company("Tech Co", "123", "123", "email@test.com", "pass");
        _mockRepository.Setup(r => r.GetCompanyById(1))
            .Returns(company);

        // Act
        var result = _service.GetCompanyById(1);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Tech Co", result.Data.Name);
    }
}
```

---

## 📚 Referências

- [Microsoft Docs - Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-reference-types)
- [Microsoft Docs - Null-Coalescing Operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator)
- [C# Guard Clauses Pattern](https://refactoring.guru/guard-clause)

---
