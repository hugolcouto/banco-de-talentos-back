# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 6. Modelos e DTOs (Data Transfer Objects)

## 💡 O que é DTO?

**DTO (Data Transfer Object)** é um padrão de design que cria objetos simples para transferir dados entre camadas ou serviços. Um DTO:

- ✅ Contém apenas propriedades de dados
- ✅ Sem lógica de negócio
- ✅ Serializável (pode virar JSON)
- ✅ Transfere apenas dados necessários

### Problema Sem DTO

```csharp
// ❌ SEM DTO - Expor Entidade diretamente
public class CompanyController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(Company company)
    {
        // ❌ Cliente envia Company com Id e CreatedAt
        // ❌ Dados sensíveis podem ser modificados
        // ❌ Acoplamento com entidade
        _companyService.CreateCompany(company);
        return Ok(company);  // ❌ Retorna Company com Id
    }
}

// Cliente envia:
{
    "id": 999,           // ❌ Não deveria enviar
    "createdAt": "...",  // ❌ Não deveria enviar
    "name": "Acme",
    "document": "123"
}
```

### Solução Com DTO

```csharp
// ✅ COM DTO - Usar objetos específicos
public class CompanyController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        // ✅ Cliente só envia dados necessários
        // ✅ Sem campos sensíveis
        // ✅ Contrato bem definido
        var result = _companyService.CreateCompany(model);
        return Ok(result);
    }
}

// Cliente envia:
{
    "name": "Acme",
    "document": "123",
    "telephone": "...",
    "email": "...",
    "password": "..."
}

// API retorna:
{
    "data": {
        "id": 1,
        "name": "Acme",
        "telephone": "...",
        "email": "...",
        "about": ""
    },
    "isSuccess": true,
    "message": ""
}
```

---

## 📦 DTOs no Projeto

### 1. CreateCompanyModel - Input DTO

```csharp
// Arquivo: BancoDeTalentos.Application/Model/CreateCompanyModel.cs
namespace BancoDeTalentos.Application.Model;

public class CreateCompanyModel
{
    public string Name { get; set; }        // Nome da empresa
    public string Document { get; set; }    // CNPJ
    public string Telephone { get; set; }   // Telefone
    public string Email { get; set; }       // Email
    public string Password { get; set; }    // Senha (será hasheada)
}
```

**Características:**

- ✅ Apenas propriedades necessárias
- ✅ Sem `Id` (será gerado pelo servidor)
- ✅ Sem `CreatedAt` (será preenchido pelo servidor)
- ✅ Propriedades `public get/set` (aceita JSON)

**Uso no Controller:**

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    // ASP.NET Core automaticamente desserializa JSON em CreateCompanyModel
    // model.Name, model.Document, etc. estão preenchidos

    ResultViewModel<CompanyViewModel> companyResult =
        _companyService.CreateCompany(model);

    return CreatedAtAction(
        nameof(GetById),
        new { id = companyResult.Data?.Id },
        companyResult
    );
}
```

**Fluxo:**

```
Client HTTP Request (JSON)
         ↓
{ "name": "Acme", "document": "...", ... }
         ↓
ASP.NET Core Deserialization
         ↓
CreateCompanyModel model
{ Name = "Acme", Document = "...", ... }
         ↓
Controller recebe model já preenchido
```

---

### 2. CompanyViewModel - Output DTO

```csharp
// Arquivo: BancoDeTalentos.Application/Model/CompanyViewModel.cs
namespace BancoDeTalentos.Application.Model;

public class CompanyViewModel
{
    // Construtor
    public CompanyViewModel(
        int id,
        string name,
        string telephone,
        string email,
        string about)
    {
        Id = id;
        Name = name;
        Telephone = telephone;
        Email = email;
        About = about;
    }

    // Propriedades de leitura (para JSON)
    public int Id { get; set; }
    public string Name { get; private set; }
    public string Telephone { get; private set; }
    public string Email { get; private set; }
    public string About { get; private set; }

    // Factory method para conversão
    public static CompanyViewModel? FromEntity(Company? entity)
        => entity is null
            ? null
            : new(
                entity.Id,
                entity.Name,
                entity.Telephone,
                entity.Email,
                entity.About
            );
}
```

**Características:**

- ✅ Apenas dados para retornar ao cliente
- ✅ `Password` NÃO está incluído (segurança)
- ✅ `Document` NÃO está incluído (privacidade)
- ✅ Factory method `FromEntity()` para conversão
- ✅ Sem `CreatedAt` (não necessário para cliente)

**Factory Method Pattern:**

```csharp
// Factory method converte Entity em ViewModel
public static CompanyViewModel? FromEntity(Company? entity)
    => new(
        entity!.Id,
        entity.Name,
        entity.Telephone,
        entity.Email,
        entity.About
    );

// Uso:
Company company = new Company("Acme", "123", "11999", "contact@acme.com", "pass");
CompanyViewModel viewModel = CompanyViewModel.FromEntity(company);

// viewModel pronto para serializar em JSON
```

**Segurança:**

```csharp
// ❌ ERRADO - Expor dados sensíveis
public class BadViewModel
{
    public int Id { get; set; }
    public string Password { get; set; }       // ❌ Nunca!
    public string Document { get; set; }       // ❌ Privacidade
    public string Email { get; set; }          // ❌ Cuidado
}

// ✅ CORRETO - Apenas dados públicos
public class CompanyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }           // ✅ Público
    public string About { get; set; }          // ✅ Público
    public string Telephone { get; set; }      // ✅ Público (geral)
    // Password, Document e outras infos sensíveis não incluídas
}
```

---

### 3. ResultViewModel - Padrão de Resposta

```csharp
using System.Net;

// Arquivo: BancoDeTalentos.Application/Model/ResultViewModel.cs
namespace BancoDeTalentos.Application.Model;

public class ResultViewModel
{
    public ResultViewModel(string message = "", bool isSuccess = true, HttpStatusCode? errorCode = null)
    {
        Message = message;
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
    }

    public string Message { get; set; }
    public bool IsSuccess { get; set; }
    public HttpStatusCode? ErrorCode { get; set; }

    // Factory methods
    public static ResultViewModel Sucess() => new();
    public static ResultViewModel Error(string message, HttpStatusCode errorCode) => new(message, false, errorCode);
}

// Versão genérica com dados
public class ResultViewModel<T> : ResultViewModel
{
    public ResultViewModel(T? data, string message = "", bool isSuccess = true, HttpStatusCode? errorCode = null)
        : base(message, isSuccess, errorCode)
    {
        Data = data;
    }

    public T? Data { get; set; }

    // Factory methods
    public static ResultViewModel<T> Success(T data) => new(data);
    public static ResultViewModel<T> Error(string message, HttpStatusCode errorCode, T? data)
        => new(data, message, false, errorCode);
}
```

**Padrão Consistente de Resposta:**

Todas as respostas seguem o mesmo padrão:

```json
{
    "data": { /* dados específicos */ },
    "message": "Mensagem se houver erro",
    "isSuccess": true/false
}
```

**Exemplos de Uso:**

```csharp
// Sucesso ao criar empresa
var result = ResultViewModel<CompanyViewModel>.Success(viewModel);

// JSON:
{
    "data": {
        "id": 1,
        "name": "Acme",
        "telephone": "11999",
        "email": "contact@acme.com",
        "about": ""
    },
    "message": "",
    "isSuccess": true
}

// Erro
var errorResult = ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada", HttpStatusCode.NotFound, null);

// JSON:
{
    "data": null,
    "message": "Empresa não encontrada",
    "isSuccess": false,
    "errorCode": 404
}

// Sem dados
var simpleResult = ResultViewModel.Sucess();

// JSON:
{
    "message": "",
    "isSuccess": true
}
```

---

## 🔄 Fluxo Completo: Entity → DTO

```
Entity (Core)                Application              Response
┌─────────────────┐
│   Company       │ Create
│                 │─────→ ┌──────────────────┐
│ id: 1           │       │ CompanyService   │
│ name: "Acme"    │       │ CreateCompany()  │
│ password: "..." │       │                  │
│ document: "..." │◀──────┤ Validação        │
│ email: "..."    │       └──────────────────┘
│ about: ""       │             │
│ createdAt: "... │             │ Converte
│                 │             │ (FromEntity)
└─────────────────┘             ▼
                        ┌─────────────────────┐
                        │ CompanyViewModel    │
                        │                     │
                        │ id: 1               │
                        │ name: "Acme"        │
                        │ email: "..."        │
                        │ telephone: "..."    │
                        │ about: ""           │
                        │                     │
                        │ ❌ SEM: password    │
                        │ ❌ SEM: document    │
                        │ ❌ SEM: createdAt   │
                        └─────────────────────┘
                                │
                                │ ResultViewModel<T>.Success()
                                ▼
                        ┌──────────────────────┐
                        │ ResultViewModel<T>   │
                        │                      │
                        │ data: {...}  ────────┼──→ Serializa
                        │ message: ""          │
                        │ isSuccess: true      │
                        └──────────────────────┘
                                │
                                ▼
                        JSON Response
                        ┌──────────────────────┐
                        │ {                    │
                        │   "data": {          │
                        │     "id": 1,         │
                        │     "name": "Acme"   │
                        │     ...              │
                        │   },                 │
                        │   "isSuccess": true  │
                        │ }                    │
                        └──────────────────────┘
                                │
                                ▼
                        Cliente recebe JSON
```

---

## 📊 Múltiplos DTOs

Frequentemente temos diferentes DTOs para diferentes operações:

```csharp
// Input para criar
public class CreateCompanyModel
{
    public string Name { get; set; }
    public string Document { get; set; }
    public string Email { get; set; }
}

// Input para atualizar
public class UpdateCompanyModel
{
    public string Name { get; set; }
    public string About { get; set; }
    // Sem Document, Email (não mudam)
}

// Output listagem (reduzido)
public class CompanyListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Sem email, telefone (economiza bandwidth)
}

// Output detalhe (completo)
public class CompanyDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string About { get; set; }
}
```

**Uso:**

```csharp
// Controller
[HttpPost]
public IActionResult Create(CreateCompanyModel model) { }

[HttpPatch("{id}")]
public IActionResult Update(int id, UpdateCompanyModel model) { }

[HttpGet]
public IActionResult GetAll()
{
    // Retorna lista reduzida
    return Ok(_companyService.GetAll()
        .Select(CompanyListItemViewModel.FromEntity));
}

[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    // Retorna detalhes completos
    var company = _companyService.GetById(id);
    return Ok(CompanyDetailViewModel.FromEntity(company));
}
```

---

## 🎯 Mapeamento Automático com AutoMapper (Futuro)

Para projetos maiores, pode usar **AutoMapper**:

```csharp
// Instalação:
// dotnet add package AutoMapper

// Configuração:
var mapper = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Company, CompanyViewModel>();
    cfg.CreateMap<CreateCompanyModel, Company>();
}).CreateMapper();

// Uso:
var viewModel = mapper.Map<CompanyViewModel>(company);
var company = mapper.Map<Company>(createModel);
```

**Vantagens:**

- Menos código
- Menos erro
- Fácil de manter

---

## 📝 Checklist para DTOs

- ✅ Input DTO tem apenas campos de entrada
- ✅ Output DTO não expõe dados sensíveis
- ✅ ResultViewModel envolve resposta
- ✅ Factory method para conversão
- ✅ Propriedades apropriadas (`public get/set` para entrada, `private set` para saída)
- ✅ Sem lógica de negócio
- ✅ Serializável em JSON

---

## 🔐 Segurança em DTOs

| Campo          | Incluir em Input? | Incluir em Output? | Motivo           |
| -------------- | ----------------- | ------------------ | ---------------- |
| `Id`           | ❌ Não            | ✅ Sim             | Servidor gera    |
| `Password`     | ✅ Entrada        | ❌ Nunca           | Sensível         |
| `Document/CPF` | ✅ Entrada        | ⚠️ Talvez          | Privacidade      |
| `Email`        | ✅ Entrada        | ⚠️ Limitado        | Pode ser público |
| `CreatedAt`    | ❌ Não            | ✅ Sim             | Info do servidor |
| `UpdatedAt`    | ❌ Não            | ✅ Sim             | Info do servidor |

---

**Referências de Arquivo:**

- `BancoDeTalentos.Application/Model/CreateCompanyModel.cs`
- `BancoDeTalentos.Application/Model/CompanyViewModel.cs`
- `BancoDeTalentos.Application/Model/ResultViewModel.cs`
- `BancoDeTalentos.Application/Services/CompanyService.cs` (uso de FromEntity)
