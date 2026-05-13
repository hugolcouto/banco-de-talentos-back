# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 12. Testes de Integração

# 12. Testes de Integração

## 💡 O que são Testes de Integração?

**Testes de Integração** testam como múltiplos componentes trabalham juntos. Diferente de testes unitários (que testam uma classe isolada), os testes de integração testam o fluxo completo:

```
Controller → Service → Repository → Database → Response
```

---

## 🧪 Testes de Integração: Atualização e Deleção de `Company`

Nesta seção há exemplos e recomendações específicas para `Update` e `Delete` de `Company`.

### Teste: Atualizar Empresa (Update)

Objetivos do teste:

- Validar que `PATCH /api/empresa/{id}` retorna `200 OK`.
- Confirmar que os dados foram efetivamente persistidos no banco.

Exemplo (xUnit + WebApplicationFactory):

```csharp
[Fact]
public async Task Update_Company_IntegrationTest()
{
    // Arrange: criar empresa
    var newCompany = new
    {
        name = "CompanyToUpdate",
        document = _faker.Company.Cnpj(),
        telephone = _faker.Phone.PhoneNumber(),
        email = _faker.Internet.Email(),
        password = _faker.Random.Hash()
    };

    var createResponse = await _client.PostAsJsonAsync("/api/empresa", newCompany);
    var created = await createResponse.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();
    int id = created!.Data!.Id;

    // Act: patch
    var updatePayload = new { name = "Updated Name" };
    var patchResponse = await _client.PatchAsJsonAsync($"/api/empresa/{id}", updatePayload);

    // Assert: status e persistência
    Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

    var getResponse = await _client.GetAsync($"/api/empresa/{id}");
    var getResult = await getResponse.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();
    Assert.Equal("Updated Name", getResult!.Data!.Name);
}
```

### Teste: Deletar Empresa (Delete)

Objetivos do teste:

- Confirmar que `DELETE /api/empresa/{id}` retorna `204 No Content`.
- Garantir que `GET /api/empresa/{id}` após a deleção retorne `404 Not Found`.

```csharp
[Fact]
public async Task Delete_Company_IntegrationTest()
{
    // Arrange: criar empresa
    var newCompany = new
    {
        name = "CompanyToDelete",
        document = _faker.Company.Cnpj(),
        telephone = _faker.Phone.PhoneNumber(),
        email = _faker.Internet.Email(),
        password = _faker.Random.Hash()
    };

    var createResponse = await _client.PostAsJsonAsync("/api/empresa", newCompany);
    var created = await createResponse.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();
    int id = created!.Data!.Id;

    // Act: delete
    var deleteResponse = await _client.DeleteAsync($"/api/empresa/{id}");

    // Assert: 204
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    // GET deve retornar 404 porque:
    // 1. Service chamou SetAsDeleted() → IsDeleted = true
    // 2. GetJobById/GetCompanyById filtra WHERE !IsDeleted
    // 3. O registro não existe mais para consultas normais
    var getResponse = await _client.GetAsync($"/api/empresa/{id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
}
```

### Dicas práticas

- Use `WebApplicationFactory<Program>` com `InMemoryDatabase` para velocidade e isolamento.
- Sempre crie (seed) os dados necessários no próprio teste (não dependa de estados externos).
- Para verificar persistência, prefira reconsultar a API (GET) em vez de inspecionar DbContext interno — isso testa o fluxo completo.
- Ao testar delete, considere cenários de dependência (empresa com vagas ativas) e crie testes que validem bloqueios/erros.

### Execução

```bash
# Executar testes de integração (projeto de testes roda todos os testes)
dotnet test BancoDeTalentos.Tests/BancoDeTalentos.Tests.csproj
```

---

## 📦 Integração com CI

- No pipeline, rode `dotnet test` em um job separado para testes de integração (ou com tags `Integration`).
- Configure a pipeline para prover variáveis de ambiente e connection string alternativas, se for usar um banco real.
- Para velocidade, prefira `InMemory` no CI para validações rápidas e teste com o banco real em um job de stage diferente, se necessário.

### Tipos de Testes

```
┌─────────────────────────────────────────────────┐
│ Teste Unitário                                  │
│ - Testa UMA classe                              │
│ - Com mocks para dependências                   │
│ - Rápido                                        │
│ Exemplo: Testar CompanyService com mock        │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ Teste de Integração                             │
│ - Testa múltiplas classes juntas                │
│ - Com dependências reais (mas simplificadas)    │
│ - Mais lento que unitário                       │
│ Exemplo: CompanyController + Service + Repo    │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ Teste E2E (End-to-End)                          │
│ - Testa aplicação completa                      │
│ - Simula cliente real                           │
│ - Mais lento                                    │
│ Exemplo: Cliente HTTP real em browser           │
└─────────────────────────────────────────────────┘

Este documento foca em Testes de Integração (meio termo)
```

---

## 🏗️ Estrutura de Teste

### Padrão AAA (Arrange-Act-Assert)

```csharp
[Fact]
public void DescricaoDoTeste()
{
    // ARRANGE (Preparar)
    // Preparar dados e objetos necessários
    var inputData = new { ... };

    // ACT (Agir)
    // Executar a ação a testar
    var result = await _client.PostAsJsonAsync("/api/endpoint", inputData);

    // ASSERT (Validar)
    // Verificar que o resultado é o esperado
    Assert.Equal(HttpStatusCode.Created, result.StatusCode);
}
```

---

## 🔧 Exemplo: CompanyControllerTests

```csharp
// Arquivo: BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using BancoDeTalentos.Application.Model;
using Bogus;
using Bogus.Extensions.Brazil;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BancoDeTalentos.Tests.Integrations;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Fixture: WebApplicationFactory
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Faker _faker;

    // Construtor: xUnit injeta a factory automaticamente
    public CompanyControllerTests(WebApplicationFactory<Program> factory)
    {
        // ✅ Factory cria uma instância completa da aplicação
        // ✅ Mas com In-Memory Database (não SQL Server)
        // ✅ Cada teste tem seu próprio DbContext
        _client = factory.CreateClient();

        // ✅ Faker gera dados realistas para testes
        _faker = new Faker("pt_BR");
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Teste 1: Criar Empresa com Sucesso
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Fact]
    public async Task Create_Company()
    {
        // 🔵 ARRANGE (Preparar dados)
        var newCompanyObject = new
        {
            name = _faker.Company.CompanyName(),
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash()
        };

        // Exemplo de valores gerados:
        // name: "Acme Corporation"
        // document: "12345678000100"
        // telephone: "(11) 99999-8888"
        // email: "contact@acme.com"
        // password: "abc123def456"

        // 🟡 ACT (Agir - fazer requisição HTTP)
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/empresa",           // URL do endpoint
            newCompanyObject          // Body (será serializado em JSON)
        );

        // A requisição simula:
        // POST /api/empresa
        // Content-Type: application/json
        // Body: { "name": "...", "document": "...", ... }

        // 🟢 ASSERT (Validar - verificar resposta)

        // 1. Validar status HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        // ✅ Deve retornar 201 Created

        // 2. Desserializar resposta
        ResultViewModel<CompanyViewModel?>? result =
            await response.Content.ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        // Response esperado:
        // {
        //   "data": {
        //     "id": 1,
        //     "name": "Acme Corporation",
        //     ...
        //   },
        //   "isSuccess": true,
        //   "message": ""
        // }

        // 3. Validar estrutura da resposta
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.IsSuccess);

        // 4. Validar dados retornados
        Assert.Equal(newCompanyObject.name, result.Data!.Name);
        // Nome retornado deve ser igual ao enviado

        // 5. Validar ID gerado
        Assert.True(result.Data!.Id > 0);
        // ID deve ser positivo (gerado pelo BD)

        // ✅ TESTE PASSOU!
        // Significa que todo o fluxo funcionou:
        // HTTP → Controller → Service → Repository → BD → ViewModel → Response
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Teste 2: Criar Empresa sem Nome (Erro)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Fact]
    public async Task Create_Company_WithoutName_ShouldFail()
    {
        // ARRANGE
        var invalidCompany = new
        {
            name = "",  // ❌ Nome vazio (inválido)
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash()
        };

        // ACT
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/empresa",
            invalidCompany
        );

        // ASSERT
        // Deve retornar erro (400 Bad Request ou 422 Unprocessable Entity)
        Assert.NotEqual(HttpStatusCode.Created, response.StatusCode);

        // Se retornar 201, o teste falha
        // ✅ TESTE VALIDOU: entrada inválida rejeita corretamente
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Teste 3: Obter Empresa por ID
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Fact]
    public async Task GetById_Company_ShouldReturnOk()
    {
        // ARRANGE: Criar uma empresa primeiro
        var newCompanyObject = new
        {
            name = "Test Company",
            document = _faker.Company.Cnpj(),
            telephone = _faker.Phone.PhoneNumber(),
            email = _faker.Internet.Email(),
            password = _faker.Random.Hash()
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/empresa",
            newCompanyObject
        );

        var createdResult = await createResponse.Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        int companyId = createdResult?.Data?.Id ?? 0;

        // ACT: Buscar a empresa criada
        HttpResponseMessage getResponse = await _client.GetAsync(
            $"/api/empresa/{companyId}"
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getResult = await getResponse.Content
            .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

        Assert.NotNull(getResult?.Data);
        Assert.Equal("Test Company", getResult.Data!.Name);

        // ✅ TESTE VALIDOU: criar e depois ler funciona
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Teste 4: Obter Empresa Inexistente
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Fact]
    public async Task GetById_NonExistentCompany_ShouldReturn404()
    {
        // ARRANGE: Não criar nada, tentar ler ID inexistente
        int inexistentId = 9999;

        // ACT
        HttpResponseMessage response = await _client.GetAsync(
            $"/api/empresa/{inexistentId}"
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        // ✅ TESTE VALIDOU: dados inexistentes retornam 404
    }
}
```

---

## 📊 WebApplicationFactory Explicado

```csharp
public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
//                                                    ^^^^^^^^^^^^^^^^^^^^^^^^
//                                                    Factory cria a aplicação
{
    public CompanyControllerTests(WebApplicationFactory<Program> factory)
    {
        // factory.CreateClient() = HttpClient para fazer requisições
        _client = factory.CreateClient();
    }
}

// O que WebApplicationFactory faz:
// 1. Lê Program.cs
// 2. Cria uma instância completa da aplicação
// 3. Mas com In-Memory Database (não SQL Server)
// 4. Retorna um HttpClient para fazer requisições
// 5. Cada teste tem seu próprio DbContext isolado

// Resultado:
// ✅ Testes rápidos (In-Memory)
// ✅ Testes isolados (DbContext próprio)
// ✅ Testes realistas (aplicação completa)
```

---

## 🎯 Boas Práticas de Testes

### 1. Nomes Descritivos

```csharp
// ❌ RUIM
[Fact]
public void Test1() { }

// ✅ BOM
[Fact]
public async Task Create_Company_WithValidData_ShouldReturnCreated()
{
    // Teste descreve o que faz
}
```

### 2. Um Assert Não! (ou Poucos)

```csharp
// ❌ RUIM
[Fact]
public void Test_EverythingTogether()
{
    // Testa 10 coisas diferentes
    // Se falhar, qual é o problema?
}

// ✅ BOM
[Fact]
public void Create_Company_ShouldReturnStatusCreated()
{
    // Testa UMA coisa
}

[Fact]
public void Create_Company_ShouldReturnValidData()
{
    // Testa OUTRA coisa
}
```

### 3. Dados Realistas

```csharp
// ❌ RUIM
var model = new CreateCompanyModel
{
    Name = "A",
    Document = "123",
    // ...
};

// ✅ BOM
var model = new CreateCompanyModel
{
    Name = _faker.Company.CompanyName(),      // "Acme Inc"
    Document = _faker.Company.Cnpj(),         // "12345678000100"
    Telephone = _faker.Phone.PhoneNumber(),   // "(11) 9999-8888"
    Email = _faker.Internet.Email(),          // "contact@acme.com"
    Password = _faker.Random.Hash()           // Hash realista
};
```

### 4. Teste Isolamento

```csharp
// ✅ Cada teste começa limpo
[Fact]
public async Task Test1()
{
    // DbContext novo (vazio)
    var result1 = await _client.PostAsJsonAsync("/api/empresa", data1);
    Assert.Equal(HttpStatusCode.Created, result1.StatusCode);
    // Company com ID 1 criada
}

[Fact]
public async Task Test2()
{
    // Novo DbContext (vazio novamente!)
    // ID 1 não existe (é outro teste)
    var result2 = await _client.GetAsync("/api/empresa/1");
    Assert.Equal(HttpStatusCode.NotFound, result2.StatusCode);
    // ✅ Isolado do Test1
}
```

---

## 🚀 Executar Testes

### Via Terminal

```bash
# Executar todos os testes
dotnet test

# Executar projeto de teste específico
dotnet test BancoDeTalentos.Tests/BancoDeTalentos.Tests.csproj

# Executar com detalhes
dotnet test --verbosity detailed

# Executar com cobertura
dotnet test /p:CollectCoverage=true
```

### Via VS Code

Instale a extensão **C# Dev Kit** ou **.NET Runtime** e verá:

- ▶️ Run (executar teste)
- 🐛 Debug (executar com breakpoints)
- Ao lado de cada `[Fact]`

---

## 🔍 Debugging de Testes

```csharp
[Fact]
public async Task Create_Company()
{
    // ... setup ...

    // 🐛 Adicione breakpoint aqui (F9)
    HttpResponseMessage response = await _client.PostAsJsonAsync(
        "/api/empresa",
        newCompanyObject
    );

    // Inspecione response:
    // - response.StatusCode
    // - response.Content
    // - response.Headers

    var result = await response.Content
        .ReadFromJsonAsync<ResultViewModel<CompanyViewModel?>>();

    // Inspecione result:
    // - result.IsSuccess
    // - result.Data
    // - result.Message
}

// Para debugar:
// 1. Coloque breakpoint
// 2. Clique "Debug" ou pressione F5
// 3. Inspecione variáveis no painel de variáveis
```

---

## ✅ Checklist: Teste de Integração Bom

- ✅ Nome descreve o que testa
- ✅ Segue padrão AAA (Arrange-Act-Assert)
- ✅ Usa dados realistas (Faker)
- ✅ Testa UM fluxo
- ✅ Validações específicas
- ✅ Isola dados de outros testes
- ✅ Testa cenário de sucesso E erro
- ✅ Rápido (< 1 segundo)

---

## ❌ Anti-Padrões

```csharp
// ❌ Testes acoplados
[Fact]
public void Test1_Criar() { /* cria empresa */ }

[Fact]
public void Test2_Ler_DependendoDe_Test1()
{
    // Depende que Test1 rodou primeiro
    // FRÁGIL! Se rodar em ordem diferente, quebra
}

// ❌ Sem isolamento
[Fact]
public void Test1() { _client.PostAsJsonAsync(...); }

[Fact]
public void Test2() { /* reusa dados de Test1 */ }
// Test2 quebra se rodar antes de Test1

// ❌ Assertions genéricas
Assert.True(response.StatusCode == HttpStatusCode.Created);
// Melhor: Assert.Equal(HttpStatusCode.Created, response.StatusCode);

// ❌ Sem tratamento de erro
var result = await response.Content.ReadFromJsonAsync<...>();
// result pode ser null! Adicione Assert.NotNull(result)
```

---

## 🔧 Troubleshooting de Testes de Delete

** Sintoma:** Teste de Delete falha porque `GET /api/recurso/{id}` após o `DELETE` retorna `200 OK` em vez de `404 Not Found`.

**Causa raiz (já vista em `JobControllerTests.Job_Flow_Should_Work`):**
1. Service não chamou `entity.SetAsDeleted()` antes de `repository.DeleteEntity(entity)`
2. **OU** Repository usa `_context.Remove(entity)` (hard delete) em vez de `_context.Update(entity)` (soft delete)
3. **OU** Método `GetById` não filtra por `!IsDeleted`

**Como depurar:**

```csharp
// No teste, adicione logs para inspecionar:
[Fact]
public async Task Delete_Job_ShouldReturn404AfterDelete()
{
    // Arrange: criar job
    int jobId = await CreateJob(...);

    // Act: deletar
    var deleteResponse = await _client.DeleteAsync($"/api/vaga/{jobId}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    // Debug: verificar se IsDeleted está true no banco
    // (Isso requer expor DbContext no teste ou adicionar endpoint de debug)
    // Se IsDeleted == false → Service esqueceu SetAsDeleted()
    // Se IsDeleted == true mas GET retorna 200 → GetById não filtra !IsDeleted

    // Assert: GET deve ser 404
    var getResponse = await _client.GetAsync($"/api/vaga/{jobId}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
}
```

**Checklist de correção:**

- [ ] Service: `entity.SetAsDeleted()` chamado antes de `_repository.Delete(entity)`
- [ ] Repository: `Delete(Entity entity)` usa `_context.Update(entity)`, não `_context.Remove(entity)`
- [ ] Repository: `GetById(id)` inclui `&& !entity.IsDeleted` no filtro
- [ ] Teste valida: DELETE → 204, GET posterior → 404

---

## 📝 Exemplo: Estrutura Completa

```csharp
// Arquivo: BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs
[Collection("Sequential")]  // ← Roda sequencialmente se necessário
public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Faker _faker;

    public CompanyControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _faker = new Faker("pt_BR");
    }

    // Test: Success Cases
    [Fact(DisplayName = "Criar empresa com dados válidos retorna 201")]
    public async Task Create_WithValidData_ReturnCreated() { }

    [Fact(DisplayName = "Obter empresa por ID retorna 200")]
    public async Task GetById_WithValidId_ReturnOk() { }

    // Test: Error Cases
    [Fact(DisplayName = "Criar empresa sem nome retorna 400")]
    public async Task Create_WithoutName_ReturnBadRequest() { }

    [Fact(DisplayName = "Obter empresa com ID inválido retorna 404")]
    public async Task GetById_WithInvalidId_ReturnNotFound() { }
}
```

---

**Referências de Arquivo:**

- `BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs`
- `BancoDeTalentos.Tests/BancoDeTalentos.Tests.csproj`
