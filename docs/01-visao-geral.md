# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 1. Visão Geral do Projeto

## 📌 Sobre o Banco de Talentos

O **Banco de Talentos** é uma plataforma web que conecta empresas com profissionais qualificados. A API backend foi desenvolvida utilizando .NET 10 com uma arquitetura limpa e moderna, garantindo escalabilidade, manutenibilidade e qualidade de código.

### 🎯 Objetivos do Projeto

1. **Conectar Empresas e Candidatos** - Criar um marketplace onde empresas postam vagas e candidatos se candidatam
2. **Gerenciar Vagas** - Empresas podem criar, editar e visualizar suas vagas em aberto
3. **Gerenciar Candidatos** - Candidatos podem criar perfis com curriculum e experiência
4. **Gerenciar Resumes** - Armazenar histórico educacional, cursos e experiências profissionais
5. **Sistema de Backoffice** - Administradores podem gerenciar a plataforma

---

## 🛠️ Tecnologias Utilizadas

### Framework e Linguagem

- **C# 12** - Linguagem moderna e fortemente tipada
- **.NET 10.0** - Framework mais recente com melhor performance
- **ASP.NET Core 10** - Framework web de alta performance

### Banco de Dados

- **Entity Framework Core 10.0.7** - ORM (Object-Relational Mapping) para acesso a dados
- **SQL Server** - Banco de dados relacional (em produção)
- **In-Memory Database** - Para desenvolvimento e testes

### Testes

- **xUnit** - Framework de testes unitários
- **Bogus** - Gerador de dados fake para testes
- **WebApplicationFactory** - Factory para testes de integração

### Padrões e Arquitetura

- **Clean Architecture** - Organização em camadas isoladas
- **SOLID Principles** - Princípios de design orientado a objetos
- **Dependency Injection** - Inversão de controle nativa do ASP.NET Core
- **Repository Pattern** - Abstração de acesso a dados
- **DTO Pattern** - Transfer Objects para comunicação entre camadas

---

## 📋 Requisitos do Sistema

### Ambiente de Desenvolvimento

- **.NET SDK 10.0** ou superior
- **Visual Studio Code** ou **Visual Studio 2024** (recomendado)
- **Git** para controle de versão
- **SQL Server Express** (opcional, para desenvolvimento local)

### Dependências do Projeto

```xml
<!-- BancoDeTalentos.API.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.7" />
```

---

## 🏗️ Arquitetura de Alto Nível

O projeto é organizado em **4 camadas principais**:

### 1. **API Layer** (`BancoDeTalentos.API`)

- **Responsabilidade:** Exposição de endpoints HTTP
- **Componentes:** Controllers, Routing, Formatação de respostas
- **Arquivo referência:** `BancoDeTalentos.API/BancoDeTalentos.API.csproj`

### 2. **Application Layer** (`BancoDeTalentos.Application`)

- **Responsabilidade:** Lógica de negócios e orquestração
- **Componentes:** Services, DTOs, Interfaces, Application Module
- **Arquivo referência:** `BancoDeTalentos.Application/BancoDeTalentos.Application.csproj`

### 3. **Core/Domain Layer** (`BancoDeTalentos.Core`)

- **Responsabilidade:** Entidades e interfaces de negócio
- **Componentes:** Entities, Value Objects, Interfaces de Repository
- **Arquivo referência:** `BancoDeTalentos.Core/BancoDeTalentos.Core.csproj`

### 4. **Infrastructure Layer** (`BancoDeTalentos.Infrastructure`)

- **Responsabilidade:** Implementação técnica e persistência
- **Componentes:** DbContext, Repositories, Infrastructure Module
- **Arquivo referência:** `BancoDeTalentos.Infrastructure/BancoDeTalentos.Infrastructure.csproj`

---

## 📊 Diagrama da Arquitetura

```
┌─────────────────────────────────────────────────────┐
│              HTTP Client / Frontend                  │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│           API Layer (Controllers)                    │
│  ┌──────────────────────────────────────────────┐  │
│  │ CompanyController | CandidateController      │  │
│  └──────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────┘
                 │ (Dependency Injection)
┌────────────────▼────────────────────────────────────┐
│      Application Layer (Services & DTOs)            │
│  ┌──────────────────────────────────────────────┐  │
│  │ ICompanyService / CompanyService             │  │
│  │ CreateCompanyModel / CompanyViewModel        │  │
│  └──────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────┘
                 │ (Dependency Injection)
┌────────────────▼────────────────────────────────────┐
│    Core/Domain Layer (Entities & Interfaces)        │
│  ┌──────────────────────────────────────────────┐  │
│  │ Company | Candidate | Job | Resume           │  │
│  │ ICompanyRepository (Interface)                │  │
│  └──────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────┘
                 │ (Dependency Injection)
┌────────────────▼────────────────────────────────────┐
│  Infrastructure Layer (DbContext & Repositories)    │
│  ┌──────────────────────────────────────────────┐  │
│  │ BancoDeTalentosDbContext                     │  │
│  │ CompanyRepository (Implementação)            │  │
│  └──────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│         Database (SQL Server / In-Memory)           │
└─────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Requisição

Quando uma requisição HTTP chega à API, ela passa pelo seguinte fluxo:

```
1. Cliente envia requisição HTTP (ex: POST /api/empresa)
                    ↓
2. Controller intercepta (CompanyController.Create)
                    ↓
3. Controller injeta o Service (ICompanyService)
                    ↓
4. Service executa lógica de negócio
                    ↓
5. Service injeta o Repository (ICompanyRepository)
                    ↓
6. Repository persiste dados no Database
                    ↓
7. Retorna os dados transformados em ViewModel (DTO)
                    ↓
8. Response é convertido em JSON
                    ↓
9. Cliente recebe a resposta HTTP
```

---

## 📦 Estrutura de Pastas

```
BancoDeTalentos/
│
├── BancoDeTalentos.API/
│   ├── Controllers/                 # Controladores HTTP
│   │   ├── CompanyController.cs
│   │   └── CandidateController.cs
│   ├── Program.cs                   # Ponto de entrada
│   └── appsettings.json             # Configurações
│
├── BancoDeTalentos.Application/
│   ├── Interfaces/                  # Interfaces de Service
│   │   └── ICompanyService.cs
│   ├── Model/                       # DTOs e ViewModels
│   │   ├── CreateCompanyModel.cs
│   │   ├── CompanyViewModel.cs
│   │   └── ResultViewModel.cs
│   ├── Services/                    # Implementação de Services
│   │   └── CompanyService.cs
│   └── ApplicationModule.cs         # Configuração de DI
│
├── BancoDeTalentos.Core/
│   ├── Entities/                    # Entidades do domínio
│   │   ├── BaseEntity.cs
│   │   ├── Company.cs
│   │   ├── Candidate.cs
│   │   ├── Job.cs
│   │   └── Resume.cs
│   ├── Interfaces/                  # Interfaces de negócio
│   │   └── ICompanyRepository.cs
│   └── ValueObjects/                # Value Objects
│       └── Address.cs
│
├── BancoDeTalentos.Infrastructure/
│   ├── Persistence/
│   │   ├── BancoDeTalentosDbContext.cs    # DbContext
│   │   └── Repositories/
│   │       └── CompanyRepository.cs       # Implementação
│   └── InfrastructureModule.cs      # Configuração de DI
│
├── BancoDeTalentos.Tests/
│   ├── Integrations/
│   │   └── CompanyControllerTests.cs
│   └── BancoDeTalentos.Tests.csproj
│
└── docs/                            # Documentação
    └── README.md
```

---

## 🚀 Próximas Etapas

Para aprofundar seu conhecimento, consulte os seguintes documentos:

1. **[Arquitetura Limpa](02-arquitetura-limpa.md)** - Entenda os princípios fundamentais
2. **[Estrutura do Projeto](03-estrutura-projeto.md)** - Explore cada camada em detalhes
3. **[Fluxo de Dados](10-fluxo-dados.md)** - Acompanhe uma requisição completa
4. **[Guia de Desenvolvimento](13-guia-desenvolvimento.md)** - Comece a desenvolver

---

**Referência de Arquivo:**  
`BancoDeTalentos.slnx` (Solution file)
