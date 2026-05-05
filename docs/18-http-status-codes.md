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

### O que o projeto faz hoje

O projeto não usa um `BaseController`. A abordagem atual é uma combinação de `CreatedAtAction` para criação, `ToActionResult()` para consultas e retorno direto para atualização/exclusão.

```csharp
using System.Net;
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

    [HttpPost]
    public IActionResult Create(CreateCompanyModel model)
    {
        ResultViewModel<CompanyViewModel> companyResult = _companyService.CreateCompany(model);

        return CreatedAtAction(
            nameof(GetById),
            new { id = companyResult.Data?.Id },
            companyResult
        );
    }

    [HttpGet]
    public IActionResult Get()
    {
        ResultViewModel<List<CompanyViewModel>> companies = _companyService.GetCompanies();

        return companies.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        ResultViewModel<CompanyViewModel> company = _companyService.GetCompanyById(id);

        return company.ToActionResult(this);
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateCompanyModel model)
    {
        _companyService.UpdateCompany(id, model);

        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _companyService.DeleteCompany(id);

        return NoContent();
    }
}
```

### Status usados na prática

| Status  | Onde aparece no projeto                                        |
| ------- | -------------------------------------------------------------- |
| **200** | `Get()` e `GetById()` quando o resultado é sucesso             |
| **201** | `Create()` via `CreatedAtAction`                               |
| **204** | `Delete()` via `NoContent()`                                   |
| **404** | `GetById()` quando o serviço retorna `HttpStatusCode.NotFound` |
| **400** | `ToActionResult()` quando o erro não é `NotFound`              |

### Extension Method real do projeto

```csharp
using System.Net;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Extensions;

public static class ResultViewModelExtensions
{
    public static IActionResult ToActionResult<T>(this ResultViewModel<T> result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result);

        return MapErrorToStatusCode(result, controller);
    }

    public static IActionResult ToActionResult(this ResultViewModel result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result);

        return MapErrorToStatusCode(result, controller);
    }

    private static IActionResult MapErrorToStatusCode(ResultViewModel result, ControllerBase controller)
    {
        if (string.IsNullOrEmpty(result.Message))
            return controller.BadRequest(result);

        return result.ErrorCode switch
        {
            HttpStatusCode.NotFound => controller.NotFound(result),
            _ => controller.BadRequest(result)
        };
    }
}
```

---

## 🧪 Exemplos reais do projeto

### Get por ID

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    ResultViewModel<CompanyViewModel> company = _companyService.GetCompanyById(id);

    return company.ToActionResult(this);
}
```

### Service que alimenta o 404

```csharp
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);

    if (company is null) return ResultViewModel<CompanyViewModel>
        .Error(
            "Empresa não encontrada",
            HttpStatusCode.NotFound,
            null
        );

    return ResultViewModel<CompanyViewModel>.Success(
        CompanyViewModel.FromEntity(company)!
    );
}
```

### Listagem

```csharp
[HttpGet]
public IActionResult Get()
{
    ResultViewModel<List<CompanyViewModel>> companies = _companyService.GetCompanies();

    return companies.ToActionResult(this);
}
```

### Criação

```csharp
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    ResultViewModel<CompanyViewModel> companyResult = _companyService.CreateCompany(model);

    return CreatedAtAction(
        nameof(GetById),
        new { id = companyResult.Data?.Id },
        companyResult
    );
}
```

### Atualização e exclusão

```csharp
[HttpPatch("{id}")]
public IActionResult Update(int id, UpdateCompanyModel model)
{
    _companyService.UpdateCompany(id, model);

    return Ok();
}

[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    _companyService.DeleteCompany(id);

    return NoContent();
}
```

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

### O que os testes de integração validam

O projeto já tem um teste que cobre esse fluxo completo em [BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs](../BancoDeTalentos.Tests/Integrations/CompanyControllerTests.cs):

```csharp
Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
Assert.Equal(HttpStatusCode.NotFound, tryGetDeleted.StatusCode);
```

Esse teste mostra o comportamento real da API hoje: criação com `201`, consultas com `200`, exclusão com `204` e consulta ao item deletado com `404`.

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

## 📋 Checklist para Implementar

- [ ] Criar arquivo `Extensions/ResultViewModelExtensions.cs`
- [ ] Adicionar lógica de mapeamento de status HTTP
- [ ] Atualizar todos os Controllers para usar novo padrão
- [ ] Adicionar testes para validar status HTTP correto
- [ ] Garantir que os serviços retornem `HttpStatusCode.NotFound` para recurso ausente
- [ ] Testar com REST Client/Postman
- [ ] Validar com clientes HTTP/JavaScript

---

## 🔗 Relação com Outros Documentos

- **[17 - InvalidCastException](17-cast-exceptions-generic-types.md)** - Como evitar erros de tipo
- **[16 - Tratamento de Null](16-tratamento-null-errors.md)** - Como validar dados antes
- **[06 - Modelos e DTOs](06-models-dtos.md)** - Estrutura de ResultViewModel
- **[09 - Controllers](09-controllers-api.md)** - Controllers em detalhes

---
