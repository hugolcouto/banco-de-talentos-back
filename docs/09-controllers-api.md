# 9. Controllers e API Endpoints

## 💡 O que é um Controller?

Um **Controller** é uma classe que gerencia as requisições HTTP. Ele:

- ✅ Recebe requisições HTTP
- ✅ Delega trabalho ao Service
- ✅ Retorna responses HTTP
- ✅ Define rotas (endpoints)

### Responsabilidades

✅ Extrair dados da requisição  
✅ Chamar o Service apropriado  
✅ Retornar HTTP status correto  
✅ Transformar resultado em response

### O Que NÃO Fazer

❌ Implementar lógica de negócio  
❌ Acessar banco de dados  
❌ Fazer cálculos complexos  
❌ Tratar de persistência

---

## 🏗️ Anatomia de um Controller

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
using BancoDeTalentos.Application.Interfaces;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers;

// Atributo 1: Define a rota base
[Route("api/empresa")]

// Atributo 2: Marca como controlador de API
[ApiController]
public class CompanyController : ControllerBase
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Injeção de Dependência
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private readonly ICompanyService _companyService;

    // Construtor: ASP.NET Core injeta o Service
    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // CREATE - POST
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    // HTTP Method: POST
    [HttpPost]
    // Rota completa: POST /api/empresa
    public IActionResult Create(CreateCompanyModel model)
    {
        // 1. Delega para o Service
        ResultViewModel<CompanyViewModel> companyResult =
            _companyService.CreateCompany(model);

        // 2. Retorna resposta apropriada
        return CreatedAtAction(
            nameof(GetById),              // Nome da ação para GET
            new { id = companyResult.Data?.Id },  // Parâmetros
            companyResult                 // Body da resposta
        );

        // CreatedAtAction retorna:
        // - Status 201 Created
        // - Header Location: /api/empresa/{id}
        // - Body: companyResult em JSON
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // READ - GET (todos)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [HttpGet]
    // Rota completa: GET /api/empresa
    public IActionResult Get()
    {
        // Por enquanto retorna OK vazio
        return Ok();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // READ - GET (por ID)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    // Parâmetro {id} vem da rota
    [HttpGet("{id}")]
    // Rota completa: GET /api/empresa/{id}
    public IActionResult GetById(int id)
    {
        // Por enquanto retorna OK vazio
        return Ok();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // UPDATE - PATCH
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [HttpPatch("{id}")]
    // Rota completa: PATCH /api/empresa/{id}
    public IActionResult Update(int id)
    {
        // Por enquanto retorna OK vazio
        return Ok();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // DELETE - DELETE
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [HttpDelete("{id}")]
    // Rota completa: DELETE /api/empresa/{id}
    public IActionResult Delete(int id)
    {
        // Por enquanto retorna OK vazio
        return Ok();
    }
}
```

---

## 🗺️ Roteamento e Convenções

### Atributos de Rota

```csharp
[Route("api/empresa")]  // Rota base do controller
[ApiController]         // Marca como API controller
public class CompanyController : ControllerBase
{
    [HttpPost]
    // Rota: POST /api/empresa
    public IActionResult Create() { }

    [HttpGet]
    // Rota: GET /api/empresa
    public IActionResult Get() { }

    [HttpGet("{id}")]
    // Rota: GET /api/empresa/123
    // {id} é parâmetro da rota
    public IActionResult GetById(int id) { }

    [HttpPatch("{id}")]
    // Rota: PATCH /api/empresa/123
    public IActionResult Update(int id) { }

    [HttpDelete("{id}")]
    // Rota: DELETE /api/empresa/123
    public IActionResult Delete(int id) { }
}
```

### Métodos HTTP

| Método     | Ação              | Status OK      |
| ---------- | ----------------- | -------------- |
| **GET**    | Ler               | 200 OK         |
| **POST**   | Criar             | 201 Created    |
| **PUT**    | Substituir        | 200 OK         |
| **PATCH**  | Modificar parcial | 200 OK         |
| **DELETE** | Deletar           | 204 No Content |

---

## 📤 Retornando Responses

```csharp
// Status 200 OK com dados
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    return Ok(new { id = 1, name = "Acme" });
}

// Status 201 Created (após criar)
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    // ...
    return CreatedAtAction(
        nameof(GetById),
        new { id = company.Id },
        companyResult
    );
}

// Status 204 No Content (sem corpo)
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    // ... deletar
    return NoContent();
}

// Status 400 Bad Request (entrada inválida)
[HttpPost]
public IActionResult Create(CreateCompanyModel model)
{
    if (string.IsNullOrEmpty(model.Name))
        return BadRequest("Nome é obrigatório");
    // ...
}

// Status 404 Not Found
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var company = _service.GetCompany(id);
    if (company == null)
        return NotFound();
    return Ok(company);
}

// Status 500 Internal Server Error
// (automático em exceções)
```

---

## 🔍 Exemplo Completo: Implementar GetById

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    // 1. Validar entrada
    if (id <= 0)
    {
        return BadRequest(new
        {
            message = "ID deve ser maior que 0",
            isSuccess = false
        });
    }

    // 2. Chamar service
    ResultViewModel<CompanyViewModel> result =
        _companyService.GetCompany(id);

    // 3. Validar resultado
    if (!result.IsSuccess || result.Data == null)
    {
        return NotFound(result);  // 404
    }

    // 4. Retornar sucesso
    return Ok(result);  // 200 com dados
}

// Cliente: GET /api/empresa/1

// Resposta 200 OK:
// {
//   "data": {
//     "id": 1,
//     "name": "Acme",
//     "telephone": "11999",
//     "email": "contact@acme.com",
//     "about": ""
//   },
//   "message": "",
//   "isSuccess": true
// }

// Client: GET /api/empresa/999 (não existe)

// Resposta 404 Not Found:
// {
//   "data": null,
//   "message": "Empresa não encontrada",
//   "isSuccess": false
// }
```

---

## 📝 Exemplo Completo: Implementar Get (todos)

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs

// Antes, no Service (futuro):
public class CompanyService : ICompanyService
{
    public ResultViewModel<List<CompanyViewModel>> GetAllCompanies()
    {
        List<Company> companies = _companyRepository.GetAllCompanies();

        List<CompanyViewModel> viewModels = companies
            .Select(c => CompanyViewModel.FromEntity(c))
            .ToList();

        return ResultViewModel<List<CompanyViewModel>>.Success(viewModels);
    }
}

// No Controller:
[HttpGet]
public IActionResult Get()
{
    // 1. Chamar service
    ResultViewModel<List<CompanyViewModel>> result =
        _companyService.GetAllCompanies();

    // 2. Retornar resultado
    return Ok(result);
}

// Cliente: GET /api/empresa

// Resposta 200 OK:
// {
//   "data": [
//     {
//       "id": 1,
//       "name": "Acme Inc",
//       ...
//     },
//     {
//       "id": 2,
//       "name": "TechCorp",
//       ...
//     }
//   ],
//   "isSuccess": true
// }
```

---

## 📝 Exemplo Completo: Implementar Update

```csharp
// Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs

// Input DTO para atualizar:
public class UpdateCompanyModel
{
    public string Name { get; set; }
    public string About { get; set; }
    public string Telephone { get; set; }
}

// Service:
public interface ICompanyService
{
    ResultViewModel<CompanyViewModel> UpdateCompany(
        int id,
        UpdateCompanyModel model);
}

public class CompanyService : ICompanyService
{
    public ResultViewModel<CompanyViewModel> UpdateCompany(
        int id,
        UpdateCompanyModel model)
    {
        // Validar ID
        if (id <= 0)
            return ResultViewModel<CompanyViewModel>.Error("ID inválido", null);

        // Buscar empresa
        Company? company = _companyRepository.GetCompanyById(id);
        if (company == null)
            return ResultViewModel<CompanyViewModel>.Error(
                "Empresa não encontrada", null);

        // Validar dados
        if (string.IsNullOrWhiteSpace(model.Name))
            return ResultViewModel<CompanyViewModel>.Error(
                "Nome é obrigatório", null);

        // Atualizar (implementar método na entidade)
        company.UpdateInfo(model.Name, model.About, model.Telephone);

        // Persistir
        _companyRepository.UpdateCompany(company);

        // Retornar
        CompanyViewModel? viewModel = CompanyViewModel.FromEntity(company);
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }
}

// Controller:
[HttpPatch("{id}")]
public IActionResult Update(int id, UpdateCompanyModel model)
{
    // 1. Validar entrada
    if (id <= 0)
        return BadRequest("ID deve ser positivo");

    // 2. Chamar service
    ResultViewModel<CompanyViewModel> result =
        _companyService.UpdateCompany(id, model);

    // 3. Validar resultado
    if (!result.IsSuccess)
    {
        return NotFound(result);  // Se empresa não existe
    }

    // 4. Retornar sucesso
    return Ok(result);
}

// Cliente: PATCH /api/empresa/1
// Body: { "name": "Acme Inc Updated", "about": "New about" }

// Resposta 200 OK:
// {
//   "data": {
//     "id": 1,
//     "name": "Acme Inc Updated",
//     "about": "New about",
//     ...
//   },
//   "isSuccess": true
// }
```

---

## 📝 Exemplo Completo: Implementar Delete

```csharp
// Service:
public class CompanyService : ICompanyService
{
    public ResultViewModel DeleteCompany(int id)
    {
        if (id <= 0)
            return ResultViewModel.Error("ID inválido");

        Company? company = _companyRepository.GetCompanyById(id);
        if (company == null)
            return ResultViewModel.Error("Empresa não encontrada");

        _companyRepository.DeleteCompany(id);
        return ResultViewModel.Sucess();
    }
}

// Controller:
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    // 1. Validar entrada
    if (id <= 0)
        return BadRequest("ID deve ser positivo");

    // 2. Chamar service
    ResultViewModel result = _companyService.DeleteCompany(id);

    // 3. Validar resultado
    if (!result.IsSuccess)
        return NotFound(result);

    // 4. Retornar sem conteúdo
    return NoContent();  // 204
}

// Cliente: DELETE /api/empresa/1

// Resposta 204 No Content (sem body)
```

---

## 🎯 Estrutura de Entrada e Saída

```csharp
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Requisição
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

POST /api/empresa
Content-Type: application/json

{
    "name": "Acme Inc",
    "document": "12345678000100",
    "telephone": "1133334444",
    "email": "contact@acme.com",
    "password": "SecurePassword123"
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Fluxo no Servidor
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. ASP.NET Core desserializa JSON → CreateCompanyModel
2. Controller.Create() recebe CreateCompanyModel
3. Chama _companyService.CreateCompany(model)
4. Service valida e cria Company
5. Repository persiste
6. Service converte em CompanyViewModel
7. Controller retorna response

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Resposta
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/empresa/1

{
    "data": {
        "id": 1,
        "name": "Acme Inc",
        "telephone": "1133334444",
        "email": "contact@acme.com",
        "about": ""
    },
    "message": "",
    "isSuccess": true
}
```

---

## 🎯 Boas Práticas

1. ✅ **Uma responsabilidade** - Apenas rotear e delegar
2. ✅ **Injetar Service** - Via construtor
3. ✅ **Retornar HTTP correto** - 200, 201, 404, etc
4. ✅ **Validar entrada** - Ainda no controller
5. ✅ **Usar DTOs** - Não expor entities
6. ✅ **Nomes claros** - Create, GetById, Update, Delete
7. ✅ **Documentar** - Comments para rotas complexas

---

## ❌ Anti-Padrões

```csharp
// ❌ NÃO FAZER: Lógica de negócio no controller
[HttpPost]
public IActionResult Create(Company company)
{
    if (company.Name.Length < 3) { }  // Validação simples, ok
    if (company.Document.IsValidCNPJ()) { }  // Lógica de negócio, ruim!
    _context.Companies.Add(company);  // Acesso direto ao BD, péssimo!
}

// ❌ NÃO FAZER: Retornar Entity diretamente
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    Company company = _repository.GetCompanyById(id);
    return Ok(company);  // Expõe password e dados sensíveis!
}

// ❌ NÃO FAZER: Sem tratamento de erro
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    // Pode retornar null sem tratamento
    Company company = _repository.GetCompanyById(id);
    return Ok(company);
}
```

---

**Referências de Arquivo:**

- `BancoDeTalentos.API/Controllers/CompanyController.cs`
- `BancoDeTalentos.API/Controllers/CandidateController.cs`
- `BancoDeTalentos.Application/Model/CreateCompanyModel.cs`
- `BancoDeTalentos.Application/Model/CompanyViewModel.cs`
