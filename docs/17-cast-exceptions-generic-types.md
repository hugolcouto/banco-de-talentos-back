# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 17. InvalidCastException e Tipos Genéricos

## 🚨 O Erro: "Unable to cast object of type..."

```
System.InvalidCastException: 'Unable to cast object of type
'BancoDeTalentos.Application.Model.ResultViewModel' to type
'BancoDeTalentos.Application.Model.ResultViewModel`1[BancoDeTalentos.Application.Model.CompanyViewModel]'.'
```

Este erro ocorre quando você tenta fazer **cast entre tipos genéricos incompatíveis**. É diferente do `NullReferenceException` - é uma operação de conversão de tipo que é impossível em runtime.

---

## 🔍 Entendendo Tipos Genéricos

### O que é um Tipo Genérico?

Um tipo genérico é uma classe que aceita um **tipo como parâmetro**.

```csharp
// ✅ Tipo genérico
public class ResultViewModel<T>
{
    public T? Data { get; set; }
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
}

// Instâncias:
ResultViewModel<Company> company_result;       // T = Company
ResultViewModel<string> string_result;         // T = string
ResultViewModel<List<int>> list_result;        // T = List<int>
```

### Não-Genérico vs Genérico

```csharp
// ❌ Não-genérico (classe base)
public class ResultViewModel
{
    public string Message { get; set; }
    public bool IsSuccess { get; set; }

    public static ResultViewModel Error(string message)
        => new(message, false);
}

// ✅ Genérico (herda de ResultViewModel)
public class ResultViewModel<T> : ResultViewModel
{
    public T? Data { get; set; }  // ← Tipo específico!

    public static ResultViewModel<T> Error(string message, T? data)
        => new(data, message, false);
}
```

---

## 🔴 Como o Erro Ocorreu no Projeto

### Cenário Real - CompanyService.cs

**Versão com Erro:**

```csharp
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);

    // ❌ PROBLEMA: Cast inválido!
    if (company is null)
        return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

    return ResultViewModel<CompanyViewModel>.Success(company);
}
```

### Por que o Cast Falha?

**Passo 1: Chamar método não-genérico**

            return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);

using System.Net;
ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound)

```

        return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound)
ResultViewModel // ← Tipo BASE, sem informação de tipo genérico
ResultViewModel base = ResultViewModel.Error("msg", HttpStatusCode.BadRequest);
Message = "Empresa não encontrada",
IsSuccess = false
// Sem propriedade Data!
return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);

```

**Passo 2: Tentar fazer cast**

        return ResultViewModel.Error(ex.Message, HttpStatusCode.BadRequest);  // InvalidCastException!
            return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);

```

**O problema:**
return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada", HttpStatusCode.NotFound, null);

- `ResultViewModel` é um tipo que **não sabe** o que contém em `Data`
  return ResultViewModel.Error("msg", HttpStatusCode.BadRequest); // Qual erro? Genérico ou não?
- Não há como converter de "desconhecido" para "conhecido"
  ResultViewModel.Error("msg", HttpStatusCode.BadRequest);

**Passo 3: Runtime lança exceção**

```

System.InvalidCastException

```

---
            return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);
### 📊 Hierarquia de Tipos

```

ResultViewModel (classe base)
│
├─ Message : string
├─ IsSuccess : bool
└─ Métodos: Error(), Sucess()
↑
│ Herança
│
ResultViewModel<T> : ResultViewModel
│
├─ Data : T ← ⚠️ Tipo genérico
├─ Métodos: Error(msg, data), Success(data)
│
├─ ResultViewModel<Company>
├─ ResultViewModel<CompanyViewModel>
├─ ResultViewModel<string>
└─ ResultViewModel<List<int>>
...

```

**A questão:**

                    return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);
// ✅ FUNCIONA - mesma classe
ResultViewModel<Company> x = ResultViewModel<Company>.Success(company);

// ✅ FUNCIONA - herança não-genérica
ResultViewModel base = ResultViewModel.Error("msg", HttpStatusCode.BadRequest);

// ❌ NÃO FUNCIONA - cast genérico é impossível
ResultViewModel<Company> y = (ResultViewModel<Company>)base;

// ❌ NÃO FUNCIONA - tipos genéricos diferentes
ResultViewModel<Company> z = (ResultViewModel<Company>)
    ResultViewModel<string>.Success("data");
```

---

## ⚠️ Padrões que Causam Este Erro

            return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);

### Padrão 1: Misturar Métodos Genéricos e Não-Genéricos

```csharp
// ❌ ERRADO - Chama método não-genérico, tenta cast para genérico
public ResultViewModel<CompanyViewModel> GetCompany(int id)
{
    if (id < 0)
        // Chama ResultViewModel.Error() (não-genérico)
        return (ResultViewModel<CompanyViewModel>)ResultViewModel.Error("ID inválido", HttpStatusCode.BadRequest);

    // ... resto do código
}

// Por que falha:
// 1. ResultViewModel.Error() retorna ResultViewModel (tipo base)
// 2. Tenta converter para ResultViewModel<CompanyViewModel> (tipo genérico)
// 3. Tipos incompatíveis em runtime → InvalidCastException
```

**Versão Correta:**

```csharp
// ✅ CORRETO - Chama método genérico
public ResultViewModel<CompanyViewModel> GetCompany(int id)
{
    if (id < 0)
        // Chama ResultViewModel<CompanyViewModel>.Error() (genérico)
        return ResultViewModel<CompanyViewModel>.Error("ID inválido", null);

    // ... resto do código
}
```

---

### Padrão 2: Retornar Tipo Base quando Genérico é Esperado

```csharp
// ❌ ERRADO - Retorno de tipo incompatível
public ResultViewModel<User> CreateUser(UserModel model)
{
    try
    {
        var user = new User(model.Name);
        _repository.Create(user);
        return ResultViewModel<User>.Success(user);  // ✅ Aqui tá certo
    }
    catch (Exception ex)
    {
        // ❌ Tenta retornar tipo base quando genérico é esperado
        return ResultViewModel.Error(ex.Message);  // InvalidCastException!
    }
}

// ✅ CORRETO
public ResultViewModel<User> CreateUser(UserModel model)
{
    try
    {
        var user = new User(model.Name);
        _repository.Create(user);
        return ResultViewModel<User>.Success(user);
    }
    catch (Exception ex)
    {
        // ✅ Retorna tipo genérico correto
        return ResultViewModel<User>.Error(ex.Message, null);
    }
}
```

---

### Padrão 3: Cast Entre Tipos Genéricos Diferentes

```csharp
// ❌ ERRADO - Tipos genéricos diferentes
ResultViewModel<Company> company_result = ResultViewModel<string>.Success("data");
// InvalidCastException: não pode converter string para Company

// ❌ ERRADO - Mesmo com cast explícito
ResultViewModel<Company> company_result =
    (ResultViewModel<Company>)ResultViewModel<string>.Success("data");
// Ainda InvalidCastException!

// ✅ CORRETO - Criar instância do tipo correto
ResultViewModel<Company> company_result =
    ResultViewModel<Company>.Error("Dados inválidos", null);
```

---

## ✅ Soluções

### Solução 1: Usar Métodos Genéricos (RECOMENDADO)

**Regra Simples:**

- Se a **assinatura de método** retorna `ResultViewModel<T>` → use `.Error()` **genérico**
- Se a **assinatura de método** retorna `ResultViewModel` (não-genérico) → use `.Error()` **não-genérico**

```csharp
// ✅ Caso 1: Método retorna genérico
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);

    if (company is null)
        // Usar Error() genérico
        return ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada", null);

    return ResultViewModel<CompanyViewModel>.Success(
        CompanyViewModel.FromEntity(company)!
    );
}

// ✅ Caso 2: Método retorna não-genérico
public ResultViewModel DeleteCompany(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);

    if (company is null)
        // Usar Error() não-genérico
        return ResultViewModel.Error("Empresa não encontrada", HttpStatusCode.NotFound);

    company.SetAsDeleted();
    _companyRepository.DeleteCompany(company);

    return ResultViewModel.Sucess();
}
```

---

### Solução 2: Factory Method Genérico

Adicionar método na classe base para facilitar:

**Em ResultViewModel.cs:**

```csharp
public class ResultViewModel
{
    // ... código existente ...

    // ✅ NOVO - Factory method genérico
    public static ResultViewModel<T> Error<T>(string message)
        => ResultViewModel<T>.Error(message, default);
}
```

**Uso:**

```csharp
// Agora funciona nos dois casos:
return ResultViewModel.Error<CompanyViewModel>("Empresa não encontrada");
return ResultViewModel.Error<User>("Usuário não encontrado");
```

---

### Solução 3: Usar Type-Safe Helpers

Criar métodos auxiliares para cada tipo:

```csharp
// Em CompanyService.cs
private static ResultViewModel<CompanyViewModel> CompanyNotFound()
    => ResultViewModel<CompanyViewModel>.Error("Empresa não encontrada", null);

// Uso:
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
{
    Company? company = _companyRepository.GetCompanyById(id);

    if (company is null)
        return CompanyNotFound();  // ✅ Seguro e legível

    return ResultViewModel<CompanyViewModel>.Success(
        CompanyViewModel.FromEntity(company)!
    );
}
```

---

## 🛡️ Estratégias de Prevenção

### 1. Conhecer a Assinatura do Método

Antes de chamar, entender:

```csharp
// ❌ ANTES - Qual é o tipo de retorno?
if (condition)
    return ResultViewModel.Error("msg", HttpStatusCode.BadRequest);  // Qual erro? Genérico ou não?

// ✅ DEPOIS - Verificar assinatura
public ResultViewModel<CompanyViewModel> GetCompanyById(int id)
// ↑ Retorna GENÉRICO
{
    // Logo, use .Error() genérico
    return ResultViewModel<CompanyViewModel>.Error("msg", null);
}
```

---

### 2. Nunca Fazer Cast Entre Genéricos

```csharp
// ❌ NUNCA FAÇA ISTO
return (ResultViewModel<CompanyViewModel>)
    ResultViewModel.Error("msg", HttpStatusCode.BadRequest);

// ❌ NEM ISTO
return (ResultViewModel<User>)
    ResultViewModel<CompanyViewModel>.Success(company);

// ✅ SEMPRE FAÇA ISTO
return ResultViewModel<CompanyViewModel>.Error("msg", null);
```

---

### 3. Enable Strict Nullable Checks

No `.csproj`:

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

Isso ajuda o compilador a **avisar** sobre mistura de tipos.

---

### 4. Code Review Checklist

Ao revisar código, verificar:

- [ ] Todos os `ResultViewModel.Error()` em métodos **não-genéricos**?
- [ ] Todos os `ResultViewModel<T>.Error()` em métodos **genéricos**?
- [ ] Nenhum cast com `(ResultViewModel<T>)` visível?
- [ ] Se há cast, tem comentário explicando por quê?
- [ ] Testes cobrem ambos os casos (sucesso e erro)?

---

## 🧪 Testes para Evitar Este Erro

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
    public void GetCompanyById_WithNullCompany_ReturnsError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetCompanyById(It.IsAny<int>()))
            .Returns((Company?)null);

        // Act
        var result = _service.GetCompanyById(999);

        // Assert - Verifica que o tipo é correto
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Empresa não encontrada", result.Message);
        // ✅ Se desse InvalidCastException, teste falharia aqui
    }

    [TestMethod]
    public void GetCompanyById_WithValidCompany_ReturnsSuccess()
    {
        // Arrange
        var company = new Company("Tech Co", "123", "phone", "email", "pass");
        _mockRepository.Setup(r => r.GetCompanyById(1))
            .Returns(company);

        // Act
        var result = _service.GetCompanyById(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Tech Co", result.Data.Name);
    }
}
```

---

## 📚 Referências

- [Microsoft Docs - Generic Classes](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [Microsoft Docs - Boxing and Unboxing](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing)
- [Microsoft Docs - InvalidCastException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidcastexception)
- [C# Type Covariance and Contravariance](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/)

---

## 🔗 Relação com Outros Documentos

- **[16 - Tratamento de Null e Errors](16-tratamento-null-errors.md)** - Complementar: como validar antes de erros
- **[06 - Modelos e DTOs](06-models-dtos.md)** - Define ResultViewModel em detalhes
- **[08 - Serviços](08-services-business-logic.md)** - Usa ResultViewModel em serviços

---
