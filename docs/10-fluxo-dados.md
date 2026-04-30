# 10. Fluxo de Dados Completo

## 🎯 O Grande Mapa: Uma Requisição HTTP Completa

Neste documento, vamos traçar uma requisição HTTP do início ao fim, entendendo como o dado flui através de todas as camadas do projeto.

---

## 📍 Cenário: Criar uma Empresa

**Cliente envia:**

```
POST /api/empresa
Content-Type: application/json

{
    "name": "Acme Corporation",
    "document": "12345678000100",
    "telephone": "11999887766",
    "email": "contact@acme.com",
    "password": "SecurePass123"
}
```

---

## 🔄 Passo a Passo: Do Cliente ao Banco de Dados

### Passo 1️⃣: Requisição Chega no Servidor

```
POST /api/empresa + JSON
            │
            ▼
ASP.NET Core recebe
            │
            ▼
Mapeia para rota [Route("api/empresa")] [HttpPost]
            │
            ▼
Encontra: CompanyController.Create()
            │
            ▼
Desserializa JSON → CreateCompanyModel
```

**JSON desserializado:**

```csharp
CreateCompanyModel model = new CreateCompanyModel
{
    Name = "Acme Corporation",
    Document = "12345678000100",
    Telephone = "11999887766",
    Email = "contact@acme.com",
    Password = "SecurePass123"
};
```

---

### Passo 2️⃣: Controller Intercepta

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
[Route("api/empresa")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;  // ← Injetado pelo ASP.NET Core
    }

    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        // model está preenchido com os dados JSON
        Console.WriteLine($"Recebido: {model.Name}");  // "Acme Corporation"

        // Passo 3: Delega para Service
        ResultViewModel<CompanyViewModel> companyResult =
            _companyService.CreateCompany(model);

        // ... (continua abaixo)
    }
}
```

**Estado neste ponto:**

```
✅ CreateCompanyModel model preenchido
✅ ICompanyService injetado (DI funcionando)
❌ Dados ainda não no banco
```

---

### Passo 3️⃣: Service Executa Lógica de Negócio

```csharp
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;  // ← Injetado
    }

    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        // 🔍 VALIDAÇÃO
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return ResultViewModel<CompanyViewModel>.Error(
                "Nome é obrigatório",
                null
            );
        }

        // Para este exemplo, validações passaram

        // 🏗️ CRIAR ENTIDADE (Domínio)
        Company company = new Company(
            name: "Acme Corporation",
            document: "12345678000100",
            telephone: "11999887766",
            email: "contact@acme.com",
            password: "SecurePass123"
        );

        // Estado da entidade após construtor:
        // company.Id = 0 (será gerado pelo BD)
        // company.CreatedAt = DateTime.MinValue (será atualizado)
        // company.Name = "Acme Corporation"
        // company.Jobs = [] (lista vazia)

        Console.WriteLine($"Entidade criada. ID temporário: {company.Id}");

        // 💾 PERSISTIR (via Repository)
        int createdId = _companyRepository.CreateCompany(company);

        // Entity Framework atualizou company.Id!
        Console.WriteLine($"Persistida. ID real: {company.Id}");  // ID: 1

        // 🔄 CONVERTER PARA DTO
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);

        // viewModel contém:
        // - Id: 1
        // - Name: "Acme Corporation"
        // - Telephone: "11999887766"
        // - Email: "contact@acme.com"
        // - About: ""
        // ❌ Sem: Password (sensível), Document (privacidade)

        // 📦 RETORNAR RESULTADO
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }
}
```

**Estado neste ponto:**

```
✅ Dados validados
✅ Entidade criada
✅ Dados persistidos
✅ ViewModel criado (sem dados sensíveis)
❌ Resposta ainda não retornou ao cliente
```

---

### Passo 4️⃣: Repository Persiste Dados

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public CompanyRepository(BancoDeTalentosDbContext context)
    {
        _context = context;
    }

    public int CreateCompany(Company company)
    {
        // 1. Adicionar à coleção em memória
        _context.Company.Add(company);

        // Estado: Entity Framework agora rastreia a entidade
        // Sabe que é nova (não tem Id no BD ainda)
        // Entity state: Added

        Console.WriteLine($"Entidade adicionada ao DbSet. ID: {company.Id}");
        // Output: "Entidade adicionada ao DbSet. ID: 0"

        // 2. Executar comando SQL
        _context.SaveChanges();

        // Neste momento:
        // - Entity Framework gera INSERT SQL
        // - SQL é enviado ao banco de dados
        // - Banco de dados executa INSERT
        // - Banco de dados gera o ID auto-increment
        // - Banco de dados retorna o novo ID
        // - Entity Framework atualiza company.Id

        Console.WriteLine($"Salvo no BD. ID atualizado: {company.Id}");
        // Output: "Salvo no BD. ID atualizado: 1"

        // 3. Retornar ID criado
        return company.Id;  // Retorna 1
    }
}
```

**SQL Gerado:**

```sql
INSERT INTO Company (Name, Document, Telephone, Email, Password, About, CreatedAt)
VALUES ('Acme Corporation', '12345678000100', '11999887766', 'contact@acme.com', 'SecurePass123', '', GETDATE())

-- Banco retorna: Id = 1
```

**Estado neste ponto:**

```
✅ INSERT executado
✅ Dados no banco de dados
✅ ID gerado: 1
✅ company.Id atualizado para 1
```

---

### Passo 5️⃣: DbContext - Banco de Dados

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs
public class BancoDeTalentosDbContext : DbContext
{
    public BancoDeTalentosDbContext(DbContextOptions<BancoDeTalentosDbContext> options)
        : base(options) { }

    // Tabela Company
    public DbSet<Company> Company { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(
            e => e.HasKey(c => c.Id)
        );

        base.OnModelCreating(modelBuilder);
    }
}
```

**Banco de Dados (In-Memory):**

```
Antes do INSERT:
┌─────────────────────────────────────┐
│           Company Table             │
├─────────┬──────────┬─────────────────┤
│  Id     │  Name    │  Document       │
├─────────┼──────────┼─────────────────┤
│ (empty) │ (empty)  │ (empty)         │
└─────────┴──────────┴─────────────────┘

Depois do INSERT:
┌─────────────────────────────────────┐
│           Company Table             │
├─────────┬──────────┬─────────────────┤
│  Id     │  Name    │  Document       │
├─────────┼──────────┼─────────────────┤
│ 1       │ Acme ... │ 12345678000100  │
└─────────┴──────────┴─────────────────┘
```

---

### Passo 6️⃣: Resposta Retorna ao Cliente

```csharp
// Voltando ao Controller...
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs

[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    // ... (service foi chamado acima)

    ResultViewModel<CompanyViewModel> companyResult =
        _companyService.CreateCompany(model);

    // Neste ponto, companyResult contém:
    /*
    {
        Data = {
            Id = 1,
            Name = "Acme Corporation",
            Telephone = "11999887766",
            Email = "contact@acme.com",
            About = ""
        },
        Message = "",
        IsSuccess = true
    }
    */

    // Retorna 201 Created com header Location
    return CreatedAtAction(
        nameof(GetById),                          // Nome da ação GET
        new { id = companyResult.Data?.Id },      // Parâmetros (Id=1)
        companyResult                             // Body da resposta
    );

    // CreatedAtAction:
    // - Status HTTP: 201 Created
    // - Header: Location: /api/empresa/1
    // - Body: JSON serializado de companyResult
}
```

---

## 🔌 Resposta HTTP Final

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/empresa/1
Content-Length: 234

{
  "data": {
    "id": 1,
    "name": "Acme Corporation",
    "telephone": "11999887766",
    "email": "contact@acme.com",
    "about": ""
  },
  "message": "",
  "isSuccess": true
}
```

**O cliente recebe:**

```javascript
// JavaScript/Frontend
response.status; // 201
response.headers.location; // "/api/empresa/1"
response.body.data.id; // 1
response.body.data.name; // "Acme Corporation"
response.body.isSuccess; // true
```

---

## 📊 Diagrama Completo: Fluxo de Dados

```
┌─────────────────────────────────────────────────────────────────┐
│  CLIENT (Frontend/Postman/etc)                                  │
│                                                                 │
│  POST /api/empresa + JSON                                      │
│  { "name": "Acme", "document": "...", ... }                   │
└────────────────────────┬────────────────────────────────────────┘
                         │ HTTP Request
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  API LAYER (BancoDeTalentos.API)                                │
│                                                                 │
│  CompanyController.Create(CreateCompanyModel model)            │
│  - model.Name = "Acme"                                         │
│  - model.Document = "..."                                      │
│  - Chama: _companyService.CreateCompany(model)                │
└────────────────────────┬────────────────────────────────────────┘
                         │ Delega ao Service
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  APPLICATION LAYER (BancoDeTalentos.Application)               │
│                                                                 │
│  CompanyService.CreateCompany(CreateCompanyModel model)       │
│  1. Valida: model.Name, model.Document                         │
│  2. Cria Entidade: Company company = new(...)                 │
│  3. Chama: _companyRepository.CreateCompany(company)          │
│  4. Converte: CompanyViewModel.FromEntity(company)            │
│  5. Retorna: ResultViewModel<CompanyViewModel>.Success(vm)    │
└────────────────────────┬────────────────────────────────────────┘
                         │ Entity + ResultViewModel
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER (BancoDeTalentos.Infrastructure)         │
│                                                                 │
│  CompanyRepository.CreateCompany(Company company)              │
│  1. _context.Company.Add(company)                              │
│  2. _context.SaveChanges()  ← Executa SQL                      │
│  3. Retorna: company.Id (1)                                    │
└────────────────────────┬────────────────────────────────────────┘
                         │ SQL INSERT
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  DATA ACCESS LAYER (Entity Framework + DbContext)              │
│                                                                 │
│  BancoDeTalentosDbContext                                      │
│  INSERT INTO Company (Name, Document, ...)                     │
│  VALUES ('Acme', '...', ...)                                   │
└────────────────────────┬────────────────────────────────────────┘
                         │ SQL Command
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  DATABASE (In-Memory / SQL Server)                              │
│                                                                 │
│  Executa INSERT                                                │
│  Gera ID: 1                                                    │
│  Retorna ID para Entity Framework                              │
│  company.Id agora = 1                                          │
└────────────────────────┬────────────────────────────────────────┘
                         │ Retorna dados
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  RESPONSE CHAIN (Voltando)                                      │
│                                                                 │
│  Repository retorna id=1 para Service                          │
│  Service retorna ResultViewModel para Controller               │
│  Controller retorna HTTP 201 Created                           │
│                                                                 │
│  JSON Response:                                                │
│  {                                                            │
│    "data": { "id": 1, "name": "Acme", ... },                 │
│    "isSuccess": true                                          │
│  }                                                            │
└────────────────────────┬────────────────────────────────────────┘
                         │ HTTP Response (201)
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  CLIENT (recebe resposta)                                       │
│                                                                 │
│  HTTP 201 Created                                              │
│  Location: /api/empresa/1                                      │
│  Body: { "data": { "id": 1, ... }, "isSuccess": true }        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Pontos-Chave do Fluxo

### 1. **Desserialização Automática**

```
JSON do cliente → CreateCompanyModel (automático)
```

ASP.NET Core automaticamente converte JSON em objeto

### 2. **Injeção de Dependência Funciona**

```
Controller recebe ICompanyService
Service recebe ICompanyRepository
Repository recebe BancoDeTalentosDbContext
```

Cadeia de DI construída automaticamente

### 3. **Entidade com Id = 0**

```
Company company = new Company(...)
company.Id = 0  (temporário)
```

Entity Framework gera o ID quando SaveChanges()

### 4. **Conversão de DTO**

```
Company (Entity)  →  CompanyViewModel (DTO)
Inclui: password  →  Exclui: password
```

Segurança: dados sensíveis não retornam

### 5. **ResultViewModel Padroniza**

```
Tudo retorna em um padrão:
{
  "data": {...},
  "message": "",
  "isSuccess": true
}
```

Consistência entre todos os endpoints

### 6. **HTTP Status Correto**

```
Create: 201 Created
Read: 200 OK
Delete: 204 No Content
Error: 404/400/500
```

Semântica HTTP respeitada

---

## 🔍 Rastreando Erros

Se algo der errado, onde quebrou?

```
Se erro em validação:
  ↓
  Service retorna: ResultViewModel.Error("mensagem")
  ↓
  Controller retorna: BadRequest(result)  ou NotFound(result)
  ↓
  Cliente recebe: HTTP 400/404 com mensagem de erro

Se erro em SQL:
  ↓
  SaveChanges() lança exceção
  ↓
  ASP.NET Core pega exceção
  ↓
  Cliente recebe: HTTP 500 Internal Server Error

Se erro em desserialização JSON:
  ↓
  ASP.NET Core não consegue mapear
  ↓
  Cliente recebe: HTTP 400 Bad Request
```

---

## 📝 Checklist: Compreender o Fluxo

- ✅ JSON vem do cliente
- ✅ ASP.NET Core desserializa automaticamente
- ✅ Controller roteia para Service
- ✅ Service valida e cria Entity
- ✅ Repository persiste no BD
- ✅ Entity Framework atualiza Id
- ✅ Service converte para ViewModel
- ✅ ResultViewModel padroniza resposta
- ✅ Controller retorna HTTP correto
- ✅ JSON retorna para cliente

Se entendeu todos os passos, parabéns! Você domina o fluxo.

---

**Referências de Arquivo:**

- `BancoDeTalentos.API/Controllers/CompanyController.cs` (Passo 2)
- `BancoDeTalentos.Application/Services/CompanyService.cs` (Passo 3)
- `BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs` (Passo 4)
- `BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs` (Passo 5)
