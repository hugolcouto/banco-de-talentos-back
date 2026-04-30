# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 2. Arquitetura Limpa - Conceitos Fundamentais

## 🎓 O que é Arquitetura Limpa?

**Arquitetura Limpa** (Clean Architecture) é um padrão de arquitetura de software proposto por Robert C. Martin ("Uncle Bob") que promove a separação de responsabilidades através de camadas concêntricas independentes. O objetivo é criar sistemas que sejam:

- ✅ **Testáveis** - Fáceis de testar automaticamente
- ✅ **Flexíveis** - Fáceis de modificar e estender
- ✅ **Independentes** - Independentes de frameworks, bibliotecas e detalhes técnicos
- ✅ **Mantíveis** - Fáceis de entender e manutenir

### A Regra Fundamental

> **"Um bom arquiteto permite que as decisões técnicas sejam tomadas o mais tarde possível, mantendo opções abertas."**

Isso significa que a arquitetura não deve estar acoplada a detalhes técnicos como bancos de dados, frameworks ou interfaces de usuário.

---

## 🔗 As Camadas da Arquitetura Limpa

A Arquitetura Limpa organiza o código em 4 camadas concêntricas. As camadas externas dependem das camadas internas, **nunca o contrário**.

```
┌─────────────────────────────────────────┐
│                                         │
│    4️⃣  Camada de Frameworks & Drivers   │
│        (UI, Web, Database, External)    │
│                                         │
│    ┌──────────────────────────────────┐ │
│    │                                  │ │
│    │   3️⃣  Camada de Infraestrutura   │ │
│    │   (Repositories, DB Context)     │ │
│    │                                  │ │
│    │  ┌───────────────────────────┐   │ │
│    │  │                           │   │ │
│    │  │  2️⃣  Camada de Aplicação  │   │ │
│    │  │  (Services, DTOs)         │   │ │
│    │  │                           │   │ │
│    │  │  ┌────────────────────┐   │   │ │
│    │  │  │                    │   │   │ │
│    │  │  │  1️⃣  Camada de Core │   │   │ │
│    │  │  │  (Entities, Rules)  │   │   │ │
│    │  │  │                    │   │   │ │
│    │  │  └────────────────────┘   │   │ │
│    │  │                           │   │ │
│    │  └───────────────────────────┘   │ │
│    │                                  │ │
│    └──────────────────────────────────┘ │
│                                         │
└─────────────────────────────────────────┘

Referência: As 4 camadas concêntricas da Arquitetura Limpa
```

### 📍 Descrição de Cada Camada

#### 1️⃣ **Camada de Core (Domínio)**

- **Localização no projeto:** `BancoDeTalentos.Core/`
- **Responsabilidade:** Contém as regras de negócio do sistema
- **Componentes:** Entidades, Interfaces, Value Objects
- **Dependências:** Nenhuma! Esta é a camada mais independente
- **Exemplo:**
  ```
  BancoDeTalentos.Core/
  ├── Entities/
  │   ├── Company.cs       (Entidade de empresa)
  │   └── Candidate.cs     (Entidade de candidato)
  └── Interfaces/
      └── ICompanyRepository.cs (Interface - não implementação!)
  ```

#### 2️⃣ **Camada de Aplicação**

- **Localização no projeto:** `BancoDeTalentos.Application/`
- **Responsabilidade:** Orquestração de casos de uso de negócio
- **Componentes:** Services, DTOs, Interfaces de serviço
- **Dependências:** Depende apenas da camada de Core
- **Exemplo:**
  ```
  BancoDeTalentos.Application/
  ├── Services/
  │   └── CompanyService.cs         (Lógica de caso de uso)
  ├── Model/
  │   ├── CreateCompanyModel.cs     (Input DTO)
  │   └── CompanyViewModel.cs       (Output DTO)
  └── Interfaces/
      └── ICompanyService.cs        (Interface do serviço)
  ```

#### 3️⃣ **Camada de Infraestrutura**

- **Localização no projeto:** `BancoDeTalentos.Infrastructure/`
- **Responsabilidade:** Implementação de detalhes técnicos
- **Componentes:** DbContext, Repositories, Configuration
- **Dependências:** Depende de Core e Application
- **Exemplo:**
  ```
  BancoDeTalentos.Infrastructure/
  ├── Persistence/
  │   ├── BancoDeTalentosDbContext.cs
  │   └── Repositories/
  │       └── CompanyRepository.cs  (Implementação de ICompanyRepository)
  └── InfrastructureModule.cs       (Configuração)
  ```

#### 4️⃣ **Camada de Apresentação (API)**

- **Localização no projeto:** `BancoDeTalentos.API/`
- **Responsabilidade:** Exposição de endpoints HTTP
- **Componentes:** Controllers, Routing, Middleware
- **Dependências:** Depende de Application
- **Exemplo:**
  ```
  BancoDeTalentos.API/
  ├── Controllers/
  │   ├── CompanyController.cs      (Endpoints HTTP)
  │   └── CandidateController.cs
  └── Program.cs                    (Configuração de DI)
  ```

---

## 🔄 Fluxo de Dependências

A regra **fundamental** da Arquitetura Limpa é:

> **"As dependências sempre apontam para dentro, nunca para fora"**

### ✅ CORRETO

```
API → Application → Core
      Infrastructure → Core
```

- API depende de Application
- Application depende de Core
- Infrastructure depende de Core
- **Core nunca depende de nada**

### ❌ INCORRETO

```
Core → Application      ❌ Não fazer!
Core → Infrastructure   ❌ Não fazer!
```

### Por Que Isso Importa?

1. **Testabilidade:** Core pode ser testado sem qualquer dependência externa
2. **Flexibilidade:** Podemos trocar a implementação de Infrastructure sem afetar Core
3. **Independência:** Core não precisa saber sobre bancos de dados ou frameworks

---

## 💡 SOLID Principles

A Arquitetura Limpa é fundamentada em 5 princípios de design orientado a objetos conhecidos como **SOLID**:

### **S** - Single Responsibility Principle (Princípio da Responsabilidade Única)

**Conceito:** Cada classe deve ter uma única razão para mudar. Cada classe deve ser responsável por uma única parte da funcionalidade.

**Exemplo no Projeto:**

```csharp
// ✅ CORRETO - Cada classe tem uma responsabilidade

// CompanyService.cs - RESPONSÁVEL pela lógica de negócio
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
    {
        // Lógica de criação de empresa (SRP: apenas isso)
        Company company = new Company(
            model.Name,
            model.Document,
            model.Telephone,
            model.Email,
            model.Password
        );

        _companyRepository.CreateCompany(company);
        return ResultViewModel<CompanyViewModel>.Success(viewModel!);
    }
}

// CompanyRepository.cs - RESPONSÁVEL pela persistência
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
public class CompanyRepository : ICompanyRepository
{
    private readonly BancoDeTalentosDbContext _context;

    public int CreateCompany(Company company)
    {
        // Responsabilidade: salvar no banco de dados (apenas isso)
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }
}

// ❌ INCORRETO - Múltiplas responsabilidades (anti-padrão)
public class BadService
{
    public void CreateCompanyAndSendEmail()
    {
        // Responsabilidade 1: criar empresa
        // Responsabilidade 2: enviar email
        // Responsabilidade 3: salvar no banco
        // Muito ruim! Viola SRP
    }
}
```

**Benefício:** Cada classe é fácil de testar, entender e modificar.

---

### **O** - Open/Closed Principle (Princípio Aberto/Fechado)

**Conceito:** Software deve ser aberto para extensão, mas fechado para modificação. Você deve ser capaz de adicionar novas funcionalidades sem alterar código existente.

**Exemplo no Projeto:**

```csharp
// ✅ CORRETO - Aberto para extensão através de interfaces

// Interface (abstração)
// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
public interface ICompanyRepository
{
    int CreateCompany(Company company);
}

// Implementação 1 - SQL Server
// Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
public class CompanyRepository : ICompanyRepository
{
    public int CreateCompany(Company company)
    {
        _context.Company.Add(company);
        _context.SaveChanges();
        return company.Id;
    }
}

// Se precisarmos de uma implementação MongoDB no futuro:
public class CompanyMongoRepository : ICompanyRepository
{
    public int CreateCompany(Company company)
    {
        // Implementação com MongoDB
        // Não precisamos alterar código existente!
    }
}

// ❌ INCORRETO - Modificando código existente para adicionar funcionalidade
public class BadRepository
{
    public void CreateCompany(Company company, bool isMongo)
    {
        if (isMongo)
        {
            // Código para MongoDB
        }
        else
        {
            // Código para SQL Server
        }
        // Violação do OCP: precisamos modificar a classe para adicionar funcionalidade
    }
}
```

**Benefício:** Novas funcionalidades podem ser adicionadas sem risco de quebrar código existente.

---

### **L** - Liskov Substitution Principle (Princípio da Substituição de Liskov)

**Conceito:** Objetos de uma classe derivada devem ser substituíveis por objetos da classe base sem quebrar a aplicação.

**Exemplo no Projeto:**

```csharp
// ✅ CORRETO - Qualquer implementação de ICompanyRepository pode ser usada

// Interface
// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
public interface ICompanyRepository
{
    int CreateCompany(Company company);
}

// Service aceita qualquer implementação
// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;  // Pode ser qualquer implementação!
    }
}

// No programa principal:
// Arquivo: BancoDeTalentos.API/Program.cs
// services.AddScoped<ICompanyRepository, CompanyRepository>();  // SQL Server
// services.AddScoped<ICompanyRepository, MockCompanyRepository>(); // Mock para testes

// O Service funciona com qualquer implementação!
```

**Benefício:** Fácil de testar usando mocks e simples trocar implementações.

---

### **I** - Interface Segregation Principle (Princípio da Segregação de Interface)

**Conceito:** Muitas interfaces especializadas são melhores do que uma interface genérica. Clientes não devem ser forçados a depender de interfaces que não usam.

**Exemplo no Projeto:**

```csharp
// ✅ CORRETO - Interfaces especializadas

// Arquivo: BancoDeTalentos.Core/Interfaces/ICompanyRepository.cs
public interface ICompanyRepository
{
    int CreateCompany(Company company);
}

// No futuro, se precisarmos de outra interface:
public interface IJobRepository
{
    int CreateJob(Job job);
}

// ❌ INCORRETO - Interface genérica grande
public interface IRepository
{
    int CreateCompany(Company company);
    int CreateJob(Job job);
    int CreateCandidate(Candidate candidate);
    int CreateResume(Resume resume);
    // ... muitos métodos não relacionados

    // CompanyRepository seria forçado a implementar métodos que não usa
}
```

**Benefício:** Cada interface define apenas o contrato necessário, sem excesso de funcionalidade.

---

### **D** - Dependency Inversion Principle (Princípio da Inversão de Dependência)

**Conceito:** Classes de alto nível não devem depender de classes de baixo nível. Ambas devem depender de abstrações.

**Exemplo no Projeto:**

```csharp
// ✅ CORRETO - Dependendo de abstração (Interface)

// Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
public class CompanyService : ICompanyService
{
    // Depende de INTERFACE, não de implementação concreta
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }
}

// Arquivo: BancoDeTalentos.API/Program.cs
// Configuração de DI - "Injetar" a dependência
builder.Services
    .AddApplication()     // Registra ICompanyService
    .AddInfrastructure(builder.Configuration);  // Registra ICompanyRepository

// ❌ INCORRETO - Dependendo de implementação concreta (anti-padrão)
public class BadService
{
    // Depende DIRETAMENTE da implementação concreta
    // Acoplamento forte!
    private CompanyRepository _repository = new CompanyRepository();

    // Impossível de testar com mock!
    // Impossível trocar implementação sem modificar o código!
}
```

**Benefício:** Baixo acoplamento, alta coesão, fácil de testar e modificar.

---

## 🧪 Aplicação no Projeto

### Fluxo Completo Seguindo SOLID

Vamos acompanhar como o projeto segue esses princípios:

**Cenário:** Criar uma nova empresa via API

```
1. Cliente faz requisição HTTP
   POST /api/empresa
   Body: { name, document, telephone, email, password }

2. CompanyController intercepta (Apresentação)
   Arquivo: BancoDeTalentos.API/Controllers/CompanyController.cs
   ✅ S: Responsável apenas por receber requisição HTTP
   ✅ D: Depende de ICompanyService (interface)

   public IActionResult Create(CreateCompanyModel model)
   {
       ResultViewModel<CompanyViewModel> companyResult =
           _companyService.CreateCompany(model);  // Injeta interface!
       return CreatedAtAction(nameof(GetById), new { id = companyResult.Data?.Id }, companyResult);
   }

3. CompanyService executa lógica (Aplicação)
   Arquivo: BancoDeTalentos.Application/Services/CompanyService.cs
   ✅ S: Responsável apenas pela lógica de criar empresa
   ✅ D: Depende de ICompanyRepository (interface)

   public ResultViewModel<CompanyViewModel> CreateCompany(CreateCompanyModel model)
   {
       Company company = new Company(
           model.Name, model.Document, model.Telephone,
           model.Email, model.Password
       );

       _companyRepository.CreateCompany(company);  // Injeta interface!
       CompanyViewModel viewModel = CompanyViewModel.FromEntity(company);
       return ResultViewModel<CompanyViewModel>.Success(viewModel);
   }

4. CompanyRepository persiste (Infraestrutura)
   Arquivo: BancoDeTalentos.Infrastructure/Persistence/Repositories/CompanyRepository.cs
   ✅ S: Responsável apenas por persistência
   ✅ L: Implementa ICompanyRepository corretamente

   public int CreateCompany(Company company)
   {
       _context.Company.Add(company);
       _context.SaveChanges();
       return company.Id;
   }

5. Resposta retorna ao cliente
   Response: { isSuccess: true, data: { id, name, email, telephone, about } }
```

---

## 🎯 Resumo dos Benefícios

| Benefício        | Como Alcançamos                                    |
| ---------------- | -------------------------------------------------- |
| **Testável**     | Services podem ser testados com mock de Repository |
| **Flexível**     | Mudar banco de dados não afeta Application         |
| **Independente** | Core não depende de Entity Framework               |
| **Mantível**     | Cada arquivo tem responsabilidade clara            |
| **Escalável**    | Adicionar novo recurso sem quebrar existente       |

---

## 📚 Para Aprender Mais

- Leia "Clean Architecture" de Robert C. Martin
- Estude o padrão Repository no projeto
- Explore os testes de integração em `BancoDeTalentos.Tests/`

---

**Referências de Arquivo:**

- `BancoDeTalentos.API/Program.cs` - Configuração de DI
- `BancoDeTalentos.Application/ApplicationModule.cs` - Registro de Services
- `BancoDeTalentos.Infrastructure/InfrastructureModule.cs` - Registro de Repositories
