# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 11. Entity Framework e Persistência

## 💡 O que é Entity Framework Core?

**Entity Framework Core (EF Core)** é um ORM (Object-Relational Mapping) que traduz objetos C# para comandos SQL. Ele permite trabalhar com o banco de dados como se fosse uma coleção de objetos em C#.

### Sem EF Core

```csharp
// ❌ SQL manual (ruim)
string sql = "INSERT INTO Company (Name, Document, Email) VALUES (@name, @doc, @email)";
SqlCommand cmd = new SqlCommand(sql, connection);
cmd.Parameters.AddWithValue("@name", "Acme");
cmd.Parameters.AddWithValue("@doc", "123");
cmd.Parameters.AddWithValue("@email", "contact@acme.com");
cmd.ExecuteNonQuery();

// Problemas:
// 1. SQL hardcoded
// 2. Propenso a SQL injection
// 3. Sem tipagem
// 4. Sem intellisense
```

### Com EF Core

```csharp
// ✅ Orientado a objetos (bom)
Company company = new Company("Acme", "123", ..., "contact@acme.com", "pass");
_context.Company.Add(company);
_context.SaveChanges();

// Benefícios:
// 1. Tipo-seguro
// 2. Automático
// 3. Com intellisense
// 4. SQL gerado automaticamente
```

---

## 🏗️ DbContext - O Centro

O **DbContext** é a classe que gerencia a comunicação entre sua aplicação e o banco de dados.

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs
using BancoDeTalentos.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BancoDeTalentos.Infrastructure.Persistence;

public class BancoDeTalentosDbContext : DbContext
{
    // Construtor: recebe opções de configuração
    public BancoDeTalentosDbContext(DbContextOptions<BancoDeTalentosDbContext> options)
        : base(options) { }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // DbSets - Representam tabelas no banco
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    // Empresa
    public DbSet<Company> Company { get; set; }
    public DbSet<Job> Jobs { get; set; }

    // Candidato
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Resume> Resumes { get; set; }

    // Administrador
    public DbSet<Backoffice> Backoffice { get; set; }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Configuração de Mapeamento
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar chave primária para Company
        modelBuilder.Entity<Company>(
            e => e.HasKey(c => c.Id)
        );

        base.OnModelCreating(modelBuilder);
    }
}
```

### DbSet Explicado

```csharp
public DbSet<Company> Company { get; set; }
// ┌─ DbSet genérico
// │  ├─ Representa uma tabela no banco (Company)
// │  ├─ Fornece LINQ para queries
// │  └─ Rastreia mudanças
// │
// └─ Propriedade Company
//    ├─ Nome da propriedade
//    └─ Convenciona nome da tabela (Company)

// Uso:
_context.Company.Add(company);        // INSERT
_context.Company.Find(1);             // SELECT by ID
_context.Company.ToList();            // SELECT *
_context.Company.Where(c => ...).First();  // SELECT com WHERE
_context.Company.Remove(company);     // DELETE
```

---

## 🔄 Ciclo de Vida de uma Entidade

```
1️⃣  Estado: Detached (desconectada)
    Company company = new Company(...);
    └─ Não rastreada pelo EF
    └─ Dados em memória apenas

2️⃣  Estado: Added (adicionada)
    _context.Company.Add(company);
    └─ Rastreada pelo EF
    └─ Marcada para INSERT
    └─ Ainda não no BD

3️⃣  Estado: Unchanged (não modificada)
    _context.SaveChanges();
    └─ INSERT executado
    └─ Agora em sync com BD
    └─ EF conhece versão original

4️⃣  Estado: Modified (modificada)
    company.UpdateInfo(...);
    _context.Company.Update(company);
    └─ EF detecta mudança
    └─ Marcada para UPDATE

5️⃣  Salvando Mudanças
    _context.SaveChanges();
    └─ UPDATE executado
    └─ BD atualizado

6️⃣  Estado: Deleted (deletada)
    _context.Company.Remove(company);
    └─ Marcada para DELETE

7️⃣  Finalizando
    _context.SaveChanges();
    └─ DELETE executado
    └─ Removida do BD
```

### Exemplo Prático

```csharp
// Estado 1: Detached
Company company = new Company("Acme", "123", ..., "contact@acme.com", "pass");
// company não é rastreada

// Estado 2: Added
_context.Company.Add(company);
// EF agora rastreia company
// Console.WriteLine(company.Id);  // 0 (ainda sem ID)

// Estado 3: Unchanged
_context.SaveChanges();
// INSERT executado
// Console.WriteLine(company.Id);  // 1 (ID gerado!)

// Estado 4: Modified
company.UpdateInfo("Acme Inc", "", "");  // Mudar nome
// EF detecta: company.Name mudou
// Console.WriteLine(_context.Entry(company).State);  // Modified

// Estado 5: Salvando
_context.SaveChanges();
// UPDATE executado

// Estado 6: Deleted
_context.Company.Remove(company);

// Estado 7: Finalizando
_context.SaveChanges();
// DELETE executado
// Company removida do BD
```

No projeto atual, a exclusão visível para `Company` é lógica: `SetAsDeleted()` marca `IsDeleted = true`, e os repositórios filtram esse campo para não expor registros excluídos em consultas normais.

---

## 📊 Rastreamento de Mudanças

Entity Framework rastreia o que mudou:

```csharp
Company company = _context.Company.Find(1);

// Estado inicial (sincronizado com BD)
company.Name = "New Name";  // Muda propriedade

// EF detecta:
var entry = _context.Entry(company);
Console.WriteLine(entry.State);  // Modified

// SaveChanges sabe que Name mudou
// Executa: UPDATE Company SET Name = 'New Name' WHERE Id = 1

_context.SaveChanges();

// Agora sincronizado novamente
Console.WriteLine(entry.State);  // Unchanged
```

---

## 🔌 Configuração do DbContext

### Program.cs - Registrar DbContext

```csharp
// Arquivo: BancoDeTalentos.API/Program.cs
using BancoDeTalentos.Application;
using BancoDeTalentos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Injetar services
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);  // ← Registra DbContext aqui

var app = builder.Build();
// ...
app.Run();
```

### InfrastructureModule - Configuração

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
            .AddData(configuration);  // ← Configura DbContext

        return services;
    }

    private static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // DESENVOLVIMENTO: In-Memory Database
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        services.AddDbContext<BancoDeTalentosDbContext>(
            o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
        );

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // PRODUÇÃO: SQL Server (descomente para usar)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        // string connectionString =
        //     configuration.GetConnectionString("SqlConnectionString")!;
        //
        // services.AddDbContext<BancoDeTalentosDbContext>(
        //     o => o.UseSqlServer(connectionString)
        // );

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        return services;
    }
}
```

---

## 💾 In-Memory vs SQL Server

### In-Memory Database (ATUAL)

```csharp
services.AddDbContext<BancoDeTalentosDbContext>(
    o => o.UseInMemoryDatabase("BancoDeTalentosMemoryDb")
);

// Características:
// ✅ Rápido (tudo em memória)
// ✅ Sem instalação de BD
// ✅ Perfeito para testes
// ✅ Perfeito para desenvolvimento
// ❌ Dados perdidos ao reiniciar
// ❌ Não é produção
```

**Banco limpo a cada execução:**

```
Inicia aplicação
    ↓
Cria novo In-Memory DB
    ↓
Tabelas vazias
    ↓
Cria dados de teste
    ↓
Encerra aplicação
    ↓
Banco descartado (tudo perdido)
```

### SQL Server (PRODUÇÃO)

```csharp
string connectionString =
    configuration.GetConnectionString("SqlConnectionString")!;

services.AddDbContext<BancoDeTalentosDbContext>(
    o => o.UseSqlServer(connectionString)
);

// Características:
// ✅ Produção
// ✅ Dados persistentes
// ✅ Escalável
// ✅ Confiável
// ❌ Mais lento que In-Memory
// ❌ Requer instalação/configuração
```

**Banco persiste:**

```
Inicia aplicação
    ↓
Conecta a SQL Server
    ↓
Tabelas já existem com dados
    ↓
Trabalha com dados reais
    ↓
Encerra aplicação
    ↓
Dados salvos no SQL Server
    ↓
Próxima execução acessa mesmos dados
```

### appsettings.json - Connection String

```json
{
  "ConnectionStrings": {
    "SqlConnectionString": "Server=localhost;Database=BancoDeTalentos;User Id=sa;Password=YourPassword;"
  }
}
```

---

## 🔍 Queries com LINQ

Entity Framework permite escrever queries em C# que são traduzidas para SQL:

```csharp
// Exemplo 1: Obter por ID
Company company = _context.Company
    .FirstOrDefault(c => c.Id == 1);

// SQL gerado: SELECT * FROM Company WHERE Id = 1 LIMIT 1

// Exemplo 2: Obter todos
List<Company> companies = _context.Company.ToList();

// SQL gerado: SELECT * FROM Company

// Exemplo 3: Com filtro
List<Company> filtered = _context.Company
    .Where(c => c.Name.Contains("Acme"))
    .ToList();

// SQL gerado: SELECT * FROM Company WHERE Name LIKE '%Acme%'

// Exemplo 4: Ordenação
List<Company> ordered = _context.Company
    .OrderBy(c => c.CreatedAt)
    .ToList();

// SQL gerado: SELECT * FROM Company ORDER BY CreatedAt ASC

// Exemplo 5: Paginação
List<Company> page = _context.Company
    .Skip(10)
    .Take(10)
    .ToList();

// SQL gerado: SELECT * FROM Company OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
```

---

## 📝 Exemplo Completo: Repository com EF Core

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public CompanyRepository(BancoDeTalentosDbContext context)
        => _context = context;

    // CREATE
    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }

    // READ by ID (filtra apenas ativos)
    public Company? GetCompanyById(int id)
    {
        return _context.Company
            .FirstOrDefault(c => c.Id == id && !c.IsDeleted);
    }

    // READ all (apenas ativos)
    public List<Company> GetAllCompanies()
    {
        return _context.Company
            .Where(c => !c.IsDeleted)
            .ToList();
    }

    // UPDATE
    public void UpdateCompany(Company company)
    {
        _context.Company.Update(company);
        _context.SaveChanges();
    }

    // DELETE (Soft Delete)
    // Nota: O Service deve chamar company.SetAsDeleted() antes de invocar este método.
    public void DeleteCompany(Company company)  // ← Recebe entidade já marcada
    {
        _context.Company.Update(company);      // ← Persiste apenas IsDeleted = true
        _context.SaveChanges();
    }
}
```

---

## 🔗 Relacionamentos entre Entidades

Entity Framework Core permite definir relacionamentos entre entidades. O mais comum é o relacionamento **1-N** (um para muitos).

### Relacionamento 1-N: Company → Job

No projeto, uma `Company` pode ter várias `Jobs`, mas cada `Job` pertence a apenas uma `Company`.

#### Passo 1: Adicionar propriedade de navegação na Entidade Filha

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Job.cs
namespace BancoDeTalentos.Core.Entities;

public class Job : BaseEntity
{
    // ... outras propriedades ...

    public int CompanyId { get; private set; }  // Chave estrangeira

    // Propriedade de navegação - aponta para a entidade pai
    public Company Company { get; private set; } = null!;
}
```

#### Passo 2: Adicionar coleção na Entidade Pai

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
namespace BancoDeTalentos.Core.Entities;

public class Company : BaseEntity
{
    // ... outras propriedades ...

    // Coleção de Jobs - inicializada no construtor
    public List<Job> Jobs { get; private set; } = new List<Job>();

    public Company(
        string name,
        string document,
        string telephone,
        string email,
        string password,
        string about
    ) : base()
    {
        // ... outras atribuições ...
        Jobs = new List<Job>();  // Inicializa a coleção
    }
}
```

#### Passo 3: Configurar o relacionamento no DbContext

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configurar chave primária para Company
    modelBuilder.Entity<Company>(e => e.HasKey(c => c.Id));

    // Configurar chave primária para Job
    modelBuilder.Entity<Job>(e => e.HasKey(j => j.Id));

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Relacionamento 1-N: Company → Job
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    modelBuilder.Entity<Job>()
        .HasOne(j => j.Company)        // Um Job tem uma Company
        .WithMany(c => c.Jobs)         // Uma Company tem muitos Jobs
        .HasForeignKey(j => j.CompanyId)  // Chave estrangeira em Job
        .IsRequired();                 // CompanyId é obrigatório

    base.OnModelCreating(modelBuilder);
}
```

### Como usar o relacionamento

#### Criar um Job com Company associado

```csharp
// Criar Company primeiro
Company company = new Company(
    "Acme Corp",
    "123.456.789-00",
    "(11) 99999-9999",
    "contact@acme.com",
    "password123",
    "About us"
);
_context.Company.Add(company);
_context.SaveChanges();  // Gera o ID

// Criar Job associado à Company
Job job = new Job(
    "Desenvolvedor Senior",
    "Descrição da vaga",
    "Benefícios",
    "Requisitos",
    "Diferenciais",
    "Endereço",
    "Remoto",
    8000m,
    DateTime.Now.AddDays(30),
    2,
    company.Id  // Passa o CompanyId
);
_context.Jobs.Add(job);
_context.SaveChanges();
```

#### Carregar dados relacionados (Include)

```csharp
// Carrega Company com todas as Jobs
Company company = _context.Company
    .Include(c => c.Jobs)  // Inclui os Jobs relacionados
    .FirstOrDefault(c => c.Id == 1);

// Agora company.Jobs está populado
foreach (Job job in company.Jobs)
{
    Console.WriteLine($"Vaga: {job.Title}");
}
```

#### Filtrar por relacionamento

```csharp
// Obter todas as Jobs de uma Company
List<Job> jobs = _context.Jobs
    .Where(j => j.CompanyId == 1)
    .ToList();

// Obter Company com Jobs usando LINQ
var result = _context.Company
    .Where(c => c.Id == 1)
    .Select(c => new {
        Company = c,
        Jobs = c.Jobs.ToList()
    })
    .FirstOrDefault();
```

### Resumo do Relacionamento

| Entidade | Propriedade | Tipo | Descrição |
|----------|-------------|------|-----------|
| Company | `Jobs` | `List<Job>` | Coleção de Jobs (navegação) |
| Job | `CompanyId` | `int` | Chave estrangeira |
| Job | `Company` | `Company` | Propriedade de navegação |

### Comandos úteis

```csharp
// Executar migrations para aplicar mudanças no banco
dotnet ef migrations add AdicionarRelacionamentoCompanyJob
dotnet ef database update
```

---

## ⚠️ Boas Práticas

1. ✅ **Sempre inicializar coleções** - `new List<Job>()` no construtor ou inicialização
2. ✅ **Usar Include() quando precisar** - Carrega dados relacionados eficientemente
3. ✅ **Configurar relacionamento no OnModelCreating** - Deixa explícito o schema
4. ✅ **Usar propriedade de navegação** - Não apenas chave estrangeira solta
5. ✅ **Considerar lazy loading** - Apenas se necessário e bem configurado

---

**Referências de Arquivo:**

- `BancoDeTalentos.Core/Entities/Company.cs`
- `BancoDeTalentos.Core/Entities/Job.cs`
- `BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs`

---

## ❌ Anti-Padrões

```csharp
// ❌ NÃO FAZER: Criar DbContext manualmente
public class BadService
{
    public void CreateCompany(Company company)
    {
        var context = new BancoDeTalentosDbContext();  // Ruim!
        context.Company.Add(company);
        context.SaveChanges();
    }
}

// ❌ NÃO FAZER: EF no Service
public class BadService
{
    public void CreateCompany(Company company)
    {
        _context.Company.Add(company);  // Sem Repository!
        _context.SaveChanges();
    }
}

// ❌ NÃO FAZER: DbContext sem DI
services.AddDbContext<BancoDeTalentosDbContext>();
// Sem opções! Como sabe qual banco usar?
```

---

**Referências de Arquivo:**

- `BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs`
- `BancoDeTalentos.Infrastructure/InfrastructureModule.cs`
- `BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs`
- `BancoDeTalentos.API/appsettings.json`
