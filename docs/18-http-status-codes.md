# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 18. HTTP Status Codes e Tratamento de Erros em Controllers

## 🚨 O Problema: Sempre Retornando HTTP 200

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    ResultViewModel<CompanyViewModel> company = _companyService.GetCompanyById(id);

    return Ok(company);  // ❌ Retorna 200 (OK) MESMO QUANDO É ERRO!
}
```

**Fluxo Incorreto:**

1. Requisição: `GET /api/empresa/999` (ID inexistente)
2. Service retorna: `{ IsSuccess = false, Message = "Empresa não encontrada" }`
3. Controller: `return Ok(company)` → **HTTP 200** ❌
4. Cliente não sabe que houve erro (HTTP status correto seria 404)

**Por que é problemático:**

- Cliente não consegue validar sucesso/erro pelo status code
- Quebra convenção REST (404 = Not Found, 200 = OK)
- Clientes HTTP/JavaScript não tratam erro automaticamente
- Impossível fazer retry automático

---

## ✅ Solução: Mapeamento Correto de Status HTTP

### Passo 1: Entender Status HTTP Semânticos

| Status  | Significado           | Quando Usar                        |
| ------- | --------------------- | ---------------------------------- |
| **200** | OK                    | Sucesso!                           |
| **201** | Created               | Recurso criado com sucesso         |
| **204** | No Content            | Sucesso, sem conteúdo (delete)     |
| **400** | Bad Request           | Erro de validação, dados inválidos |
| **401** | Unauthorized          | Não autenticado                    |
| **403** | Forbidden             | Sem permissão                      |
| **404** | Not Found             | Recurso não existe                 |
| **500** | Internal Server Error | Erro no servidor                   |

**Mapeamento para nossa API:**

```csharp
ResultViewModel result = _service.GetCompanyById(id);

if (result.IsSuccess)
    return Ok(result);                    // 200

if (result.Message.Contains("não encontrada"))
    return NotFound(result);              // 404

if (result.Message.Contains("inválido"))
    return BadRequest(result);            // 400

if (result.Message.Contains("não autorizado"))
    return Unauthorized(result);          // 401

return StatusCode(500, result);           // 500 - erro genérico
```

---

## 🛠️ Solução 1: Extension Method (Recomendado - Simples)

### Criar Extension Method

**Arquivo: `Extensions/ResultViewModelExtensions.cs`**

```csharp
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Extensions;

/// <summary>
/// Extension methods para converter ResultViewModel em IActionResult com status HTTP correto
/// </summary>
public static class ResultViewModelExtensions
{
    /// <summary>
    /// Converte ResultViewModel<T> para IActionResult com status HTTP apropriado
    /// 404 NotFound se "não encontrada" na mensagem
    /// 400 BadRequest para outros erros
    /// 200 OK se sucesso
    /// </summary>
    public static IActionResult ToActionResult<T>(
        this ResultViewModel<T> result,
        ControllerBase controller)
    {
        // Sucesso
        if (result.IsSuccess)
            return controller.Ok(result);

        // Erros específicos por mensagem
        return MapErrorToStatusCode(result, controller);
    }

    /// <summary>
    /// Converte ResultViewModel (não-genérico) para IActionResult
    /// </summary>
    public static IActionResult ToActionResult(
        this ResultViewModel result,
        ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(result);

        return MapErrorToStatusCode(result, controller);
    }

    /// <summary>
    /// Mapeia mensagem de erro para status HTTP apropriado
    /// </summary>
    private static IActionResult MapErrorToStatusCode(
        ResultViewModel result,
        ControllerBase controller)
    {
        if (string.IsNullOrEmpty(result.Message))
            return controller.BadRequest(result);

        // Verificar padrões comuns na mensagem
        return result.Message.ToLower() switch
        {
            // 404 Not Found
            var msg when msg.Contains("não encontrada") ||
                         msg.Contains("não encontrado") ||
                         msg.Contains("não existe") =>
                controller.NotFound(result),

            // 400 Bad Request - Validação
            var msg when msg.Contains("inválido") ||
                         msg.Contains("obrigatório") ||
                         msg.Contains("já existe") ||
                         msg.Contains("deve ter") =>
                controller.BadRequest(result),

            // 401 Unauthorized
            var msg when msg.Contains("não autorizado") ||
                         msg.Contains("não autenticado") ||
                         msg.Contains("senha") =>
                controller.Unauthorized(result),

            // 403 Forbidden
            var msg when msg.Contains("sem permissão") ||
                         msg.Contains("acesso negado") =>
                controller.StatusCode(StatusCodes.Status403Forbidden, result),

            // Erro genérico 400
            _ => controller.BadRequest(result)
        };
    }
}
```

### Usar nos Controllers

```csharp
using BancoDeTalentos.API.Extensions;
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

[Route("api/empresa")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var company = _companyService.GetCompanyById(id);
        return company.ToActionResult(this);  // ✅ Retorna 404 se não encontrada
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateCompanyModel model)
    {
        var result = _companyService.UpdateCompany(id, model);
        return result.ToActionResult(this);  // ✅ Retorna 400 se erro
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _companyService.DeleteCompany(id);
        return result.ToActionResult(this);  // ✅ Retorna 400 se erro
    }
}
```

---

## 🛠️ Solução 2: Base Controller (Profissional - Escalável)

Para projetos maiores, centralizar a lógica em base controller.

### Criar Base Controller

**Arquivo: `Controllers/BaseController.cs`**

```csharp
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

/// <summary>
/// Base controller com métodos para converter ResultViewModel para status HTTP correto
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Converte ResultViewModel<T> para IActionResult com status HTTP apropriado
    /// </summary>
    protected IActionResult HandleResult<T>(ResultViewModel<T> result)
    {
        // Sucesso
        if (result.IsSuccess)
            return Ok(result);

        // Mapear erro para status HTTP
        return MapErrorToStatusCode(result);
    }

    /// <summary>
    /// Converte ResultViewModel (não-genérico) para IActionResult
    /// </summary>
    protected IActionResult HandleResult(ResultViewModel result)
    {
        if (result.IsSuccess)
            return Ok(result);

        return MapErrorToStatusCode(result);
    }

    /// <summary>
    /// Mapeia mensagem de erro para status HTTP apropriado
    /// </summary>
    private IActionResult MapErrorToStatusCode(ResultViewModel result)
    {
        if (string.IsNullOrEmpty(result.Message))
            return BadRequest(result);

        return result.Message.ToLower() switch
        {
            // 404 Not Found
            var msg when msg.Contains("não encontrada") ||
                         msg.Contains("não encontrado") ||
                         msg.Contains("não existe") =>
                NotFound(result),

            // 400 Bad Request
            var msg when msg.Contains("inválido") ||
                         msg.Contains("obrigatório") ||
                         msg.Contains("já existe") =>
                BadRequest(result),

            // 401 Unauthorized
            var msg when msg.Contains("não autorizado") ||
                         msg.Contains("senha") =>
                Unauthorized(result),

            // 403 Forbidden
            var msg when msg.Contains("sem permissão") =>
                StatusCode(StatusCodes.Status403Forbidden, result),

            // Erro genérico
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Retorna 201 Created com localização do recurso criado
    /// </summary>
    protected IActionResult HandleCreated<T>(
        ResultViewModel<T> result,
        string routeName,
        object routeValues)
    {
        if (!result.IsSuccess)
            return MapErrorToStatusCode(result);

        return CreatedAtAction(routeName, routeValues, result);
    }
}
```

### Atualizar Controller para Herdar de BaseController

```csharp
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

[Route("api/empresa")]
public class CompanyController : BaseController  // ✅ Herdar de BaseController
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        var result = _companyService.CreateCompany(model);
        return HandleCreated(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HttpGet]
    public IActionResult Get()
    {
        var companies = _companyService.GetCompanies();
        return HandleResult(companies);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var company = _companyService.GetCompanyById(id);
        return HandleResult(company);  // ✅ Automático!
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateCompanyModel model)
    {
        var result = _companyService.UpdateCompany(id, model);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _companyService.DeleteCompany(id);
        return HandleResult(result);
    }
}
```

---

## 📊 Comparação: Extension vs Base Controller

| Aspecto               | Extension      | Base Controller |
| --------------------- | -------------- | --------------- |
| **Complexidade**      | Simples        | Moderada        |
| **Reutilização**      | Requer `using` | Automática      |
| **Escalabilidade**    | Boa            | Excelente       |
| **Projetos pequenos** | ⭐⭐⭐         | ⭐⭐            |
| **Projetos grandes**  | ⭐⭐           | ⭐⭐⭐          |
| **Consistência**      | Depende do dev | Garantida       |

**Recomendação:** Use **Base Controller** para novo projeto.

---

## 🧪 Testando

### REST Client (VS Code)

```http
### Teste 1: GET com sucesso
GET http://localhost:5000/api/empresa/1

### Resposta esperada: HTTP 200 OK
# {
#   "isSuccess": true,
#   "data": { "id": 1, "name": "Tech Co", ... },
#   "message": ""
# }

###

### Teste 2: GET - Não encontrado
GET http://localhost:5000/api/empresa/999

### Resposta esperada: HTTP 404 Not Found
# {
#   "isSuccess": false,
#   "data": null,
#   "message": "Empresa não encontrada"
# }

###

### Teste 3: PATCH - Não encontrado
PATCH http://localhost:5000/api/empresa/999
Content-Type: application/json

{
  "name": "New Name",
  "telephone": "123456",
  "email": "test@test.com",
  "about": "About"
}

### Resposta esperada: HTTP 404 Not Found

###

### Teste 4: POST - Dados inválidos (se houver validação)
POST http://localhost:5000/api/empresa
Content-Type: application/json

{
  "name": "",
  "document": "invalid"
}

### Resposta esperada: HTTP 400 Bad Request
```

### Com cURL

```bash
# Teste 1: Sucesso (200)
curl -v http://localhost:5000/api/empresa/1

# Teste 2: Não encontrado (404)
curl -v http://localhost:5000/api/empresa/999

# Teste 3: Erro de validação (400)
curl -X POST http://localhost:5000/api/empresa \
  -H "Content-Type: application/json" \
  -d '{"name":"","document":""}'
```

---

## 💡 Avançado: Adicionar ErrorCode Estruturado

Para APIs mais robustas, adicionar código de erro estruturado:

### Atualizar ResultViewModel

```csharp
public class ResultViewModel
{
    public ResultViewModel(
        string message = "",
        bool isSuccess = true,
        string? errorCode = null)
    {
        Message = message;
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
    }

    public string Message { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }  // ✅ Novo

    public static ResultViewModel Error(string message, string? errorCode = null)
        => new(message, false, errorCode);
}

public class ResultViewModel<T> : ResultViewModel
{
    public ResultViewModel(
        T? data,
        string message = "",
        bool isSuccess = true,
        string? errorCode = null)
        : base(message, isSuccess, errorCode)
        => Data = data;

    public T? Data { get; set; }

    public static ResultViewModel<T> Error(
        string message,
        T? data = default,
        string? errorCode = null)
        => new(data, message, false, errorCode);
}
```

### Usar Código de Erro

```csharp
// No Service
if (company is null)
    return ResultViewModel<CompanyViewModel>.Error(
        "Empresa não encontrada",
        null,
        "COMPANY_NOT_FOUND"  // ✅ Código estruturado
    );

// Response
{
  "isSuccess": false,
  "data": null,
  "message": "Empresa não encontrada",
  "errorCode": "COMPANY_NOT_FOUND"
}
```

---

## 📋 Checklist para Implementar

- [ ] Criar arquivo `Extensions/ResultViewModelExtensions.cs` ou `Controllers/BaseController.cs`
- [ ] Adicionar lógica de mapeamento de status HTTP
- [ ] Atualizar todos os Controllers para usar novo padrão
- [ ] Adicionar testes para validar status HTTP correto
- [ ] Documentar códigos de erro em `ErrorCodes.cs`
- [ ] Testar com REST Client/Postman
- [ ] Validar com clientes HTTP/JavaScript

---

## 🔗 Relação com Outros Documentos

- **[17 - InvalidCastException](17-cast-exceptions-generic-types.md)** - Como evitar erros de tipo
- **[16 - Tratamento de Null](16-tratamento-null-errors.md)** - Como validar dados antes
- **[06 - Modelos e DTOs](06-models-dtos.md)** - Estrutura de ResultViewModel
- **[09 - Controllers](09-controllers-api.md)** - Controllers em detalhes

---
