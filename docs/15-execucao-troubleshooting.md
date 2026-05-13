# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 15. Guia de Execução e Troubleshooting

## 🚀 Como Executar o Projeto

### Pré-requisitos

- **.NET 10.0 SDK** ou superior
  - Download: https://dotnet.microsoft.com/download
  - Verificar: `dotnet --version`

- **Visual Studio 2022** (recomendado) ou **Visual Studio Code**
  - Extensões recomendadas para VS Code:
    - C# DevKit
    - REST Client

- **Git** (para clonar repositório)

---

## 📥 Passo 1: Clonar e Abrir o Projeto

### Opção 1: Via Command Line

```bash
# Clonar repositório
git clone https://github.com/seu-usuario/bancodetalentos.git

# Entrar na pasta do projeto
cd bancodetalentos/bancodetalentos.back

# Abrir no VS Code
code .

# Ou abrir no Visual Studio
start BancoDeTalentos.slnx
```

### Opção 2: Via Visual Studio

1. Abrir Visual Studio 2022
2. File → Open → Folder → Selecionar pasta `bancodetalentos.back`
3. Ou: File → Clone Repository → Colar URL do repositório

---

## 🔧 Passo 2: Restaurar Dependências

### Via Command Line

```bash
# Restaurar pacotes NuGet
dotnet restore

# Verificar restauração
dotnet build
```

### Via Visual Studio

- As dependências são automaticamente restauradas ao abrir a solution
- Se necessário: Tools → NuGet Package Manager → Package Manager Console
  ```powershell
  Update-Package -Reinstall
  ```

---

## ▶️ Passo 3: Executar o Projeto

### Opção 1: Via Visual Studio

1. Selecionar projeto `BancoDeTalentos.API` como startup project
2. Pressionar `F5` ou clicar no botão "Run"
3. A aplicação abrirá em http://localhost:5000

### Opção 2: Via Command Line

```bash
# Entrar na pasta da API
cd BancoDeTalentos.API

# Executar
dotnet run

# Saída esperada:
# info: Microsoft.Hosting.Lifetime[14]
#      Now listening on: http://localhost:5000
#      Now listening on: https://localhost:5001
```

### Opção 3: Via VS Code

1. Pressionar `Ctrl+Shift+D` (Debug View)
2. Selecionar `.NET 10.0` na dropdown
3. Clicar no ícone Play

---

## 🧪 Passo 4: Executar Testes

### Via Visual Studio

```
Test → Run All Tests (Ctrl+R, A)
```

Ou específico:

```
Test → Run Tests In Current Plane (Ctrl+R, P)
```

### Via Command Line

```bash
# Executar todos os testes
dotnet test

# Teste específico
dotnet test --filter "CompanyControllerTests"

# Com saída verbosa
dotnet test -v d

# Gerar relatório de cobertura
dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
```

---

## 🔍 Verificação Rápida

Após iniciar a aplicação, testar endpoints:

### Via REST Client (VS Code)

```http
### Criar empresa
POST http://localhost:5000/api/empresa
Content-Type: application/json

{
  "name": "Tech Company",
  "document": "12345678000190",
  "telephone": "(11) 98765-4321",
  "email": "contato@techcompany.com",
  "password": "SenhaSegura123!"
}

### Listar empresas
GET http://localhost:5000/api/empresa

### Buscar empresa por ID
GET http://localhost:5000/api/empresa/1
```

Salvar como `BancoDeTalentos.API/requests.http`

### Via cURL

```bash
# Criar empresa
curl -X POST http://localhost:5000/api/empresa \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tech Company",
    "document": "12345678000190",
    "telephone": "(11) 98765-4321",
    "email": "contato@techcompany.com",
    "password": "SenhaSegura123!"
  }'

# Listar empresas
curl http://localhost:5000/api/empresa
```

### Via Postman/Insomnia

1. Abrir Postman/Insomnia
2. Criar nova requisição
3. Método: POST
4. URL: `http://localhost:5000/api/empresa`
5. Body (raw JSON):
   ```json
   {
     "name": "Tech Company",
     "document": "12345678000190",
     "telephone": "(11) 98765-4321",
     "email": "contato@techcompany.com",
     "password": "SenhaSegura123!"
   }
   ```
6. Enviar

---

## 🐛 Troubleshooting

### Problema 1: "Could not find .NET runtime"

**Causa:** .NET 10.0 não instalado

**Solução:**

```bash
# Verificar versão instalada
dotnet --version

# Descarregar .NET 10.0
# https://dotnet.microsoft.com/download

# Ou instalar via Homebrew (Mac)
brew install dotnet

# Ou via chocolatey (Windows)
choco install dotnet-sdk
```

---

### Problema 2: "Unable to resolve service for type 'ICompanyRepository'"

**Causa:** Dependência não registrada no `Program.cs`

**Solução:** Verificar `BancoDeTalentos.API/Program.cs`:

```csharp
// Verificar se está presente:
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services
    .AddApplication()      // ← Deve estar
    .AddInfrastructure(builder.Configuration);  // ← Deve estar

var app = builder.Build();
app.Run();
```

Se não estiver, adicionar:

```csharp
using BancoDeTalentos.Application;
using BancoDeTalentos.Infrastructure;

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

---

### Problema 3: "Connection refused" ao conectar banco de dados

**Causa:** Banco de dados InMemory não inicializado ou SQL Server não rodando

**Solução para InMemory (atual):**

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/InfrastructureModule.cs
// Já está configurado:
services.AddDbContext<BancoDeTalentosDbContext>(options =>
    options.UseInMemoryDatabase("BancoDeTalentos")
);
```

**Se quiser usar SQL Server:**

1. Instalar SQL Server (https://www.microsoft.com/sql-server)

2. Atualizar `BancoDeTalentos.Infrastructure/InfrastructureModule.cs`:

   ```csharp
   services.AddDbContext<BancoDeTalentosDbContext>(options =>
       options.UseSqlServer(
           configuration.GetConnectionString("DefaultConnection")
       )
   );
   ```

3. Adicionar connection string em `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=BancoDeTalentos;Trusted_Connection=true;Encrypt=false;"
     }
   }
   ```

4. Criar migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

---

### Problema 4: Testes falhando com "WebApplicationFactory"

**Causa:** Projeto de testes não tem referência correta

**Solução:**

```bash
# Verificar arquivo BancoDeTalentos.Tests.csproj
# Deve conter:
# <ProjectReference Include="..\BancoDeTalentos.API\BancoDeTalentos.API.csproj" />

# Restaurar pacotes de teste
cd BancoDeTalentos.Tests
dotnet restore

# Executar testes novamente
dotnet test
```

---

### Problema 5: Porta 5000 já em uso

**Causa:** Outra aplicação usando mesma porta

**Solução 1: Liberar a porta**

```bash
# macOS/Linux - Encontrar processo
lsof -i :5000

# Matar processo
kill -9 <PID>

# Windows - Via PowerShell
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

**Solução 2: Mudar porta no `launchSettings.json`**

```json
// Arquivo: BancoDeTalentos.API/Properties/launchSettings.json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5002" // ← Mudar porta
    }
  }
}
```

---

### Problema 6: Build falhando com "obj not found"

**Causa:** Cache de build corrompido

**Solução:**

```bash
# Limpar diretórios de build
dotnet clean

# Restaurar dependências
dotnet restore

# Reconstruir
dotnet build
```

---

### Problema 7: "No DbSet defined" ao tentar Query

**Causa:** DbSet não configurado no DbContext

**Solução:** Verificar `BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs`:

```csharp
public DbSet<Company> Company { get; set; }
public DbSet<Job> Jobs { get; set; }
public DbSet<Candidate> Candidates { get; set; }
public DbSet<Resume> Resumes { get; set; }
public DbSet<Backoffice> Backoffice { get; set; }

// Se faltando DbSet para nova entidade:
public DbSet<MinhaEntidade> MinhaEntidade { get; set; }
```

---

### Problema 8: Validação de DTO não funcionando

**Causa:** Model Binding não validando

**Solução:** Adicionar validação no Controller/Service:

```csharp
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    // Validação manual
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Ou validação em Service
    var result = _companyService.CreateCompany(model);

    if (!result.IsSuccess)
        return BadRequest(result);

    return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
}
```

---

### Problema 9: Erro de CORS ao acessar de Frontend

**Causa:** Frontend em domínio diferente tentando acessar API

**Solução:** Adicionar CORS em `Program.cs`:

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// ADICIONAR CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

var app = builder.Build();

// USAR CORS
app.UseCors("AllowAll");

app.MapControllers();
app.Run();
```

---

### Problema 10: "Password field cannot be null"

**Causa:** Entidade está tentando validar propriedade nula

**Solução:** Adicionar validação:

```csharp
if (string.IsNullOrWhiteSpace(model.Password))
    return ResultViewModel<CompanyViewModel>.Error("Senha obrigatória", null);

// Ou permitir null com ?
public string? Password { get; private set; }
```

---

### Problema 11: Exceções sem tratamento consistente

**Causa:** exceções escapando diretamente sem um handler global no pipeline.

**Solução:** o projeto registra `app.UseExceptionHandler()` em `Program.cs` para centralizar o tratamento de erros e manter respostas mais previsíveis.

---

### Problema 12: DELETE não remove o registro -- GET ainda retorna 200 OK após deletar

**Causa:** O método `SetAsDeleted()` não foi chamado antes de `DeleteJob()`/`DeleteCompany()`, OU o Repository está usando `Remove()` (hard delete) em vez de `Update()` (soft delete). Esta é a causa raiz do bug encontrado em `JobControllerTests.Job_Flow_Should_Work` onde `deletedRes.StatusCode` retorna OK em vez de NotFound.

**Diagnóstico:**

1. Verificar se `entity.IsDeleted == true` após chamar `SetAsDeleted()`
2. Verificar se o Repository usa `Update()` (não `Remove()`)
3. Verificar se `GetJobById()` / `GetCompanyById()` filtra `!j.IsDeleted`

**Solução:**

```csharp
// Service: SEMPRE chamar SetAsDeleted() ANTES do Delete
public ResultViewModel DeleteJob(int id)
{
    Job? job = _jobRepository.GetJobById(id);
    if (job is null)
        return ResultViewModel.Error("Vaga não encontrada", HttpStatusCode.NotFound);

    job.SetAsDeleted();                    // ← OBRIGATÓRIO
    _jobRepository.DeleteJob(job);        // ← Repository persiste o flag
    return ResultViewModel.Sucess();
}

// Repository: usar Update() (nunca Remove)
public void DeleteJob(Job job)
{
    _context.Jobs.Update(job);            // ← Persiste IsDeleted = true
    _context.SaveChanges();
}

// Query: filtrar !IsDeleted
public Job? GetJobById(int id)
{
    return _context.Jobs
        .SingleOrDefault(j => j.Id == id && !j.IsDeleted);
}
```

**Efeito esperado após correção:**

```
1. DELETE /api/vaga/1  →  204 No Content
2. GET    /api/vaga/1  →  404 Not Found (porque IsDeleted = true e filtro !IsDeleted)
```

---

## 📊 Verificação de Saúde (Health Check)

Adicionar health check em `Program.cs`:

```csharp
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
```

Testar:

```bash
curl http://localhost:5000/health
```

---

## 📈 Monitoramento

### Logging

Verificar logs em `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "BancoDeTalentos": "Debug"
    }
  }
}
```

### Performance

Executar com profiler:

```bash
dotnet run --configuration Release
```

---

## 📚 Recursos Úteis

| Recurso               | Link                                    |
| --------------------- | --------------------------------------- |
| Documentação .NET     | https://learn.microsoft.com/dotnet      |
| Entity Framework Core | https://learn.microsoft.com/ef/core     |
| ASP.NET Core          | https://learn.microsoft.com/aspnet/core |
| xUnit                 | https://xunit.net                       |
| Postman               | https://www.postman.com                 |

---

## ✅ Checklist: Primeira Execução

- [ ] .NET 10.0 instalado e verificado
- [ ] Repositório clonado/pasta aberta
- [ ] `dotnet restore` executado
- [ ] `dotnet build` sem erros
- [ ] Testes passam: `dotnet test`
- [ ] Aplicação inicia: `dotnet run`
- [ ] Endpoint responde: GET /api/empresa
- [ ] Criar empresa funciona (POST /api/empresa)
- [ ] Banco de dados em memória working

Se todos os passos estiverem ✅, seu ambiente está pronto para desenvolvimento! 🎉

---

**Referências de Arquivo:**

- `BancoDeTalentos.API/Program.cs`
- `BancoDeTalentos.API/Properties/launchSettings.json`
- `BancoDeTalentos.Infrastructure/InfrastructureModule.cs`
- `BancoDeTalentos.Infrastructure/Persistence/BancoDeTalentosDbContext.cs`
- `BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs`
