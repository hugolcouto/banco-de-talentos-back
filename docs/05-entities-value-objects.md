# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 5. Entidades e Value Objects

## 🎯 O que são Entidades?

**Entidades** são classes que representam conceitos do domínio do negócio. Elas possuem:

- ✅ **Identidade** - Um `Id` único que as distingue de outras
- ✅ **Ciclo de vida** - Criadas, modificadas, deletadas
- ✅ **Estado** - Propriedades que descrevem seus atributos
- ✅ **Comportamento** - Métodos que alteramou consultam o estado

### Exemplo no Projeto

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
public class Company : BaseEntity  // Herda Id e CreatedAt
{
    public string Name { get; private set; }           // Estado
    public string Document { get; private set; }       // Estado
    public string Telephone { get; private set; }      // Estado
    public List<Job> Jobs { get; private set; }        // Estado

    // Comportamento: construtor valida e inicializa
    public Company(string name, string document, string telephone, ...)
    {
        Name = name;
        Document = document;
        Telephone = telephone;
        // ...
    }
}
```

---

## 🏗️ BaseEntity - Classe Base para Todas as Entidades

### O Problema

Muitas tabelas no banco de dados têm coluna de `Id` e `CreatedAt`. Repetir isso em cada entidade é **DRY Violation** (Don't Repeat Yourself).

### A Solução

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/BaseEntity.cs
namespace BancoDeTalentos.Core.Entities;

public class BaseEntity
{
    // Propriedade de Id privada
    // Apenas leitura e definição controlada
    public int Id { get; private set; }

    // Data de criação
    public DateTime CreatedAt { get; private set; }

    // Exclusão lógica
    public bool IsDeleted { get; private set; }

    public void SetAsDeleted() => IsDeleted = true;
}
```

### Benefícios

1. **DRY** - Id e CreatedAt em um só lugar
2. **Consistência** - Todas as entidades têm o mesmo padrão
3. **Encapsulamento** - Propriedades `private set`
4. **Manutenção** - Mudança em um lugar afeta todas
5. **Exclusão lógica** - `IsDeleted` e `SetAsDeleted()` centralizados

### Como Usar

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
public class Company : BaseEntity  // ← Herda Id e CreatedAt
{
    public string Name { get; private set; }

    public Company(string name)
    {
        Name = name;
        // BaseEntity gerencia Id e CreatedAt
    }
}

// Ao usar:
var company = new Company("Acme Inc");

// Automaticamente tem:
// company.Id          (do BaseEntity)
// company.CreatedAt   (do BaseEntity)
// company.IsDeleted   (do BaseEntity)
// company.Name        (próprio)
```

---

## 🔐 Propriedades Privadas e Encapsulamento

### ✅ CORRETO - Propriedades com `private set`

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
public class Company : BaseEntity
{
    public string Name { get; private set; }
    public string Document { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }

    public Company(string name, string document, string email, string password)
    {
        Name = name;
        Document = document;
        Email = email;
        Password = password;
    }
}

// Uso correto:
var company = new Company("Acme", "123456", "contact@acme.com", "pass123");
Console.WriteLine(company.Name);  // ✅ Leitura funciona

// Uso incorreto:
company.Name = "NewName";  // ❌ Erro! Não pode atribuir (private set)
```

**Benefícios:**

- ✅ Protege integridade dos dados
- ✅ Força uso do construtor para validação
- ✅ Evita modificações acidentais

### ❌ INCORRETO - Propriedades com `public set`

```csharp
public class BadCompany
{
    public string Name { get; set; }  // ❌ Pode ser modificado de fora

    public BadCompany(string name)
    {
        Name = name;
    }
}

// Uso:
var company = new BadCompany("Acme");
company.Name = "";  // ❌ Válido! Mas dados inconsistentes
company.Name = null;  // ❌ Válido! Mas dados inválidos
```

**Problemas:**

- ❌ Estado pode ficar inválido
- ❌ Sem controle sobre modificações
- ❌ Lógica pode ser contornada

---

## 📋 Entidades do Projeto

### 1. Company (Empresa)

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
public class Company : BaseEntity
{
    public Company() { }  // Necessário para EF Core

    public Company(
        string name,
        string document,
        string telephone,
        string email,
        string password
    ) : base()
    {
        Name = name;
        Document = document;
        Telephone = telephone;
        Email = email;
        Password = password;
        About = "";
        Jobs = new List<Job>();
    }

    public string Name { get; private set; }
    public string Document { get; private set; }
    public string Telephone { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string About { get; private set; }

    // Relacionamento one-to-many com Job
    public List<Job> Jobs { get; private set; } = new List<Job>();
}
```

**Responsabilidades:**

- Armazena dados de empresa
- Valida dados no construtor
- Gerencia lista de vagas

**Campos:**

- `Name` - Nome da empresa (obrigatório)
- `Document` - CNPJ (obrigatório, único)
- `Telephone` - Telefone de contato
- `Email` - Email de contato
- `Password` - Senha para login
- `About` - Descrição da empresa
- `Jobs` - Vagas publicadas pela empresa

### 2. Candidate (Candidato)

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Candidate.cs
public class Candidate : BaseEntity
{
    public Candidate() { }

    public Candidate(
        string fullName,
        DateTime birthdate,
        string address,
        string description,
        string phoneNumber,
        string email,
        string document
    ) : base()
    {
        FullName = fullName;
        Birthdate = birthdate;
        Address = address;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        Document = document;
        Resume = null;
    }

    public string FullName { get; private set; }
    public DateTime Birthdate { get; private set; }
    public string Address { get; private set; }
    public string Description { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public string Document { get; private set; }

    // Relacionamento one-to-one com Resume
    public Resume? Resume { get; private set; }
}
```

**Responsabilidades:**

- Armazena dados pessoais do candidato
- Mantém referência ao curriculum

**Campos:**

- `FullName` - Nome completo
- `Birthdate` - Data de nascimento
- `Address` - Endereço
- `Description` - Sobre o candidato
- `PhoneNumber` - Telefone
- `Email` - Email
- `Document` - CPF
- `Resume` - Curriculum (opcional)

### 3. Resume (Curriculum)

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Resume.cs
public class Resume : BaseEntity
{
    public Resume() { }

    public Resume(
        List<string> scholarity,
        List<string> courses,
        List<string> experiences
    ) : base()
    {
        Scholarity = scholarity;
        Courses = courses;
        Experiences = experiences;
    }

    // Histórico educacional
    public List<string> Scholarity { get; private set; }

    // Cursos realizados
    public List<string> Courses { get; private set; }

    // Experiências profissionais
    public List<string> Experiences { get; private set; }
}
```

**Responsabilidades:**

- Armazena formação educacional
- Armazena cursos complementares
- Armazena experiências profissionais

### 4. Job (Vaga de Emprego)

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Job.cs
public class Job : BaseEntity
{
    public Job() { }

    public Job(
        string title,
        string description,
        string benefits,
        string requirements,
        string optionalRequirements,
        string address,
        string modality,
        decimal salary,
        int myProperty,
        DateTime dueDate,
        int openedVacancies,
        int companyId
    ) : base()
    {
        Title = title;
        Description = description;
        Benefits = benefits;
        Requirements = requirements;
        OptionalRequirements = optionalRequirements;
        Address = address;
        Modality = modality;
        Salary = salary;
        ShowSalary = true;
        MyProperty = myProperty;
        DueDate = dueDate;
        OpenedVacancies = openedVacancies;
        CompanyId = companyId;
    }

    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Benefits { get; private set; }
    public string Requirements { get; private set; }
    public string OptionalRequirements { get; private set; }
    public string Address { get; private set; }
    public string Modality { get; private set; }  // "Remoto", "Híbrido", "Presencial"
    public decimal Salary { get; private set; }
    public bool ShowSalary { get; private set; }  // Se exibe salário publicamente
    public int MyProperty { get; private set; }
    public DateTime DueDate { get; private set; } // Data limite para candidaturas
    public int OpenedVacancies { get; private set; }  // Quantas vagas abertas
    public int CompanyId { get; private set; }      // ID da empresa (Foreign Key)
}
```

**Responsabilidades:**

- Armazena dados da vaga
- Valida campo de data
- Garante consistência de dados

### 5. Backoffice (Administrador)

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Backoffice.cs
public class Backoffice : BaseEntity
{
    // Ainda não implementado neste exemplo
}
```

---

## 🔗 Relacionamentos Entre Entidades

```
┌─────────────────────┐
│    Company          │
│  (Empresa)          │
│                     │
│  - Name             │
│  - Document         │
│  - Jobs ────────┐   │
└─────────────────┼───┘
                  │ one-to-many
                  │
                  ▼
        ┌─────────────────────┐
        │    Job              │
        │  (Vaga)             │
        │                     │
        │  - Title            │
        │  - Description      │
        │  - Salary           │
        │  - CompanyId (FK)     │
        └─────────────────────┘


┌─────────────────────┐
│   Candidate         │
│  (Candidato)        │
│                     │
│  - FullName         │
│  - Email            │
│  - Resume ──────┐   │
└─────────────────┼───┘
                  │ one-to-one
                  │
                  ▼
        ┌─────────────────────┐
        │    Resume           │
        │  (Curriculum)       │
        │                     │
        │  - Scholarity       │
        │  - Courses          │
        │  - Experiences      │
        └─────────────────────┘
```

---

## 📝 Value Objects (Objetos de Valor)

**Value Objects** são objetos que representam um conceito único e são definidos por seus atributos (não por identidade).

### Exemplo - Address (Endereço)

```csharp
// Arquivo: BancoDeTalentos.Core/ValueObjects/Address.cs
public class Address
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }

    public Address(string street, string number, string city,
                   string state, string zipCode)
    {
        Street = street;
        Number = number;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Address address)
            return false;

        return Street == address.Street
            && Number == address.Number
            && City == address.City
            && State == address.State
            && ZipCode == address.ZipCode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, Number, City, State, ZipCode);
    }

    public override string ToString()
    {
        return $"{Street}, {Number} - {City}, {State} - {ZipCode}";
    }
}
```

**Diferenças entre Entities e Value Objects:**

| Aspecto           | Entidade           | Value Object       |
| ----------------- | ------------------ | ------------------ |
| **Identidade**    | Tem Id único       | Sem Id             |
| **Igualdade**     | Por Id             | Por atributos      |
| **Ciclo de vida** | Longo (BD)         | Curto (em memória) |
| **Mutabilidade**  | Geralmente mutável | Imutável           |
| **Exemplo**       | Company, Candidate | Address, Money     |

### Usando Value Objects

```csharp
// Exemplo de uso (futuro)
public class Company : BaseEntity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }  // Value Object

    public Company(string name, Address address)
    {
        Name = name;
        Address = address;
    }
}

// Ao usar:
var address = new Address("Rua X", "123", "São Paulo", "SP", "01234-567");
var company = new Company("Acme", address);

// Dois endereços com mesmos atributos são considerados iguais
var address2 = new Address("Rua X", "123", "São Paulo", "SP", "01234-567");
Console.WriteLine(address == address2);  // true (igualdade por valor)

// Diferente de Entity:
var company1 = new Company("Acme", address);
var company2 = new Company("Acme", address);
Console.WriteLine(company1 == company2);  // false (igualdade por Id)
```

---

## 🏛️ Padrão de Construtor

### Dois Construtores Necessários

```csharp
// Arquivo: BancoDeTalentos.Core/Entities/Company.cs
public class Company : BaseEntity
{
    // 1️⃣ Construtor sem parâmetros (obrigatório para Entity Framework)
    public Company() { }

    // 2️⃣ Construtor com parâmetros (para aplicação usar)
    public Company(
        string name,
        string document,
        string telephone,
        string email,
        string password
    ) : base()
    {
        // Validação e inicialização
        Name = name;
        Document = document;
        Telephone = telephone;
        Email = email;
        Password = password;
        About = "";
        Jobs = new List<Job>();
    }
}
```

**Por que dois construtores?**

1. **EF Core precisa de construtor sem parâmetros** - Para criar instâncias ao ler do BD
2. **Aplicação precisa de construtor com parâmetros** - Para criar novas entidades com validação

---

## 🎯 Regras de Ouro

1. ✅ **Herdar de BaseEntity** - Todos têm Id e CreatedAt
2. ✅ **Propriedades `private set`** - Protege integridade
3. ✅ **Construtor com parâmetros** - Valida ao criar
4. ✅ **Construtor sem parâmetros** - Para EF Core
5. ✅ **Sem lógica de negócio complexa** - Apenas dados
6. ✅ **Imutável quando possível** - Reduz bugs

---

**Referências de Arquivo:**

- `BancoDeTalentos.Core/Entities/BaseEntity.cs`
- `BancoDeTalentos.Core/Entities/Company.cs`
- `BancoDeTalentos.Core/Entities/Candidate.cs`
- `BancoDeTalentos.Core/Entities/Resume.cs`
- `BancoDeTalentos.Core/Entities/Job.cs`
- `BancoDeTalentos.Core/ValueObjects/Address.cs`
