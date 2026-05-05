# Documentação Completa - Banco de Talentos API

## 📋 Índice

Bem-vindo à documentação completa do projeto **Banco de Talentos API**. Este guia foi elaborado para ajudar você a entender a arquitetura, o fluxo de dados, os padrões de design e as melhores práticas utilizadas neste projeto.

### 📚 Documentos Principais

1. **[Visão Geral do Projeto](01-visao-geral.md)**
   - Propósito e objetivos
   - Tecnologias utilizadas
   - Requisitos do sistema

2. **[Arquitetura Limpa - Conceitos Fundamentais](02-arquitetura-limpa.md)**
   - O que é Arquitetura Limpa?
   - Princípios SOLID
   - Camadas da arquitetura
   - Isolamento de dependências

3. **[Estrutura do Projeto em Detalhes](03-estrutura-projeto.md)**
   - Organização de pastas
   - Responsabilidades de cada camada
   - Convenções de nomenclatura
   - Estrutura de namespaces

4. **[Injeção de Dependência (DI)](04-injecao-dependencia.md)**
   - Conceitos de DI
   - Configuração no projeto
   - Application Module e Infrastructure Module
   - Ciclos de vida (Scoped, Transient, Singleton)

5. **[Entidades e Value Objects](05-entities-value-objects.md)**
   - Padrão de Entidades
   - BaseEntity - Classe base para todas as entidades
   - Modelos de dados (Company, Candidate, Job, Resume)
   - Propriedades privadas e encapsulamento

6. **[Modelos e DTOs](06-models-dtos.md)**
   - Diferença entre Entities e ViewModels/DTOs
   - CreateCompanyModel - Input DTO
   - CompanyViewModel - Output DTO
   - ResultViewModel - Padrão de resposta
   - Mapeamento de Entidades

7. **[Padrão Repository](07-repositories-pattern.md)**
   - O que é o padrão Repository?
   - Interface ICompanyRepository
   - Implementação CompanyRepository
   - Benefícios e abstração de dados

8. **[Camada de Serviços (Application)](08-services-business-logic.md)**
   - Responsabilidades dos Services
   - Interface ICompanyService
   - CompanyService - Implementação
   - Lógica de negócios

9. **[Controllers e API Endpoints](09-controllers-api.md)**
   - Estrutura de Controllers
   - Routing e convenções
   - CompanyController - Exemplo completo
   - CandidateController - Estrutura básica

10. **[Fluxo de Dados Completo](10-fluxo-dados.md)**
    - Traçando uma requisição HTTP
    - Exemplo prático: Criar uma empresa
    - Fluxo de dados entre camadas
    - Diagrama do fluxo

11. **[Entity Framework e Persistência](11-entity-framework-persistencia.md)**
    - DbContext - BancoDeTalentosDbContext
    - Configuração de banco de dados
    - In-Memory vs SQL Server
    - Mapeamento de entidades

12. **[Testes de Integração](12-testes-integracao.md)**
    - O que são testes de integração?
    - Estrutura de testes
    - WebApplicationFactory
    - Exemplo prático: CompanyControllerTests
    - Assertivas e validações

13. **[Guia Passo-a-Passo de Desenvolvimento](13-guia-desenvolvimento.md)**
    - Como adicionar um novo recurso
    - Criando uma nova entidade
    - Implementando um novo service
    - Adicionando um novo controller
    - Criando testes de integração

14. **[Boas Práticas e Padrões](14-boas-praticas.md)**
    - Convenções de código
    - Encapsulamento e privacidade
    - Tratamento de erros
    - Nomeação de variáveis
    - Comentários úteis

15. **[Guia de Execução e Troubleshooting](15-execucao-troubleshooting.md)**
    - Como executar o projeto
    - Pré-requisitos e instalação
    - Execução via Visual Studio, Command Line e VS Code
    - Testes automatizados

16. **[Tratamento de Null e Prevenção de Erros](16-tratamento-null-errors.md)**
    - "Object reference not set to an instance of an object"
    - Padrões problemáticos e soluções
    - Guard clauses e validação
    - Null coalescing e null-conditional operators
    - Estratégias de prevenção e testes

17. **[InvalidCastException e Tipos Genéricos](17-cast-exceptions-generic-types.md)**
    - "Unable to cast object of type..."
    - Entendendo tipos genéricos vs não-genéricos
    - Padrões que causam o erro
    - Soluções e factory methods
    - Prevenção com code review

18. **[HTTP Status Codes e Tratamento de Erros em Controllers](18-http-status-codes.md)**
    - Retornando status HTTP correto (200, 404, 400, etc)
    - Mapeamento semântico de erros
    - Extension Methods vs Base Controller
    - Exemplos práticos com REST Client
    - ErrorCodes estruturados
    - Resolução de problemas comuns
    - Troubleshooting detalhado

---

## 🎯 Como Usar Esta Documentação

### Para Iniciantes

Se você é novo no projeto, recomendamos seguir esta ordem:

1. Comece com a [Visão Geral](01-visao-geral.md)
2. Entenda a [Arquitetura Limpa](02-arquitetura-limpa.md)
3. Explore a [Estrutura do Projeto](03-estrutura-projeto.md)
4. Siga o [Guia de Desenvolvimento](13-guia-desenvolvimento.md)

### Para Desenvolvedores Experientes

Se você conhece .NET e arquitetura, pode focar em:

1. [Fluxo de Dados](10-fluxo-dados.md) para entender a conectividade
2. [Testes de Integração](12-testes-integracao.md) para entender como validar seu código
3. [Boas Práticas](14-boas-praticas.md) para manter a consistência

### Para Code Review

Para revisar código ou contribuir:

1. [Boas Práticas](14-boas-praticas.md)
2. [Guia de Desenvolvimento](13-guia-desenvolvimento.md)
3. [Testes de Integração](12-testes-integracao.md)

---

## 📁 Estrutura de Diretórios

```
BancoDeTalentos/
├── BancoDeTalentos.API/              # Camada de Apresentação (Controllers, endpoints HTTP)
├── BancoDeTalentos.Application/      # Camada de Aplicação (Services, DTOs, interfaces)
├── BancoDeTalentos.Core/             # Camada de Domínio (Entidades, interfaces de negócio)
├── BancoDeTalentos.Infrastructure/   # Camada de Infraestrutura (Repositórios, DbContext)
├── BancoDeTalentos.Tests/            # Testes (Testes de integração, unitários)
└── docs/                             # Documentação (Este arquivo e outros)
```

---

## 🏗️ Conceitos-Chave

### Arquitetura Limpa

O projeto segue os princípios de Arquitetura Limpa, que organiza o código em camadas independentes com responsabilidades bem definidas. Cada camada conhece apenas a camada abaixo dela, criando um isolamento de dependências.

### SOLID Principles

- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Aberto para extensão, fechado para modificação
- **L**iskov Substitution: Interfaces bem definidas permitem substituição
- **I**nterface Segregation: Interfaces específicas ao invés de gerais
- **D**ependency Inversion: Depender de abstrações, não de implementações

### Padrões de Design Utilizados

1. **Repository Pattern** - Abstração de acesso aos dados
2. **Service Layer Pattern** - Lógica de negócios centralizada
3. **Dependency Injection** - Inversão de controle
4. **DTO (Data Transfer Object)** - Transferência de dados entre camadas
5. **MVC** - Model-View-Controller para API REST

---

## 🚀 Quick Start

### Executar o Projeto

```bash
# Restaurar dependências
dotnet restore

# Executar migrations (se aplicável)
dotnet ef database update

# Executar a API
dotnet run --project BancoDeTalentos.API
```

### Executar os Testes

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test /p:CollectCoverage=true
```

---

## 📞 Endpoints Principais

### Company (Empresa)

- `POST /api/empresa` - Criar nova empresa
- `GET /api/empresa` - Listar todas as empresas
- `GET /api/empresa/{id}` - Obter detalhes da empresa
- `PATCH /api/empresa/{id}` - Atualizar empresa
- `DELETE /api/empresa/{id}` - Deletar empresa

### Candidate (Candidato)

- `POST /api/candidate` - Criar novo candidato
- `GET /api/candidate` - Listar todos os candidatos
- `GET /api/candidate/{id}` - Obter detalhes do candidato
- `PATCH /api/candidate/{id}` - Atualizar candidato
- `DELETE /api/candidate/{id}` - Deletar candidato

---

## 💡 Dicas Importantes

1. **Sempre use interfaces** - Isso facilita a injeção de dependência e testes
2. **Mantenha as entidades do domínio puras** - Sem dependências de frameworks
3. **Use DTOs para transferência de dados** - Não exponha entidades diretamente
4. **Escreva testes de integração** - Valide o fluxo completo
5. **Documente casos de uso complexos** - Adicione comentários quando necessário

---

## 📖 Próximas Leituras Recomendadas

- Martin Fowler - Patterns of Enterprise Application Architecture
- "Clean Architecture" de Robert C. Martin
- "The Clean Code" de Robert C. Martin
- Documentação oficial do ASP.NET Core
- Documentação do Entity Framework Core

---

**Versão:** 1.0  
**Data de Atualização:** Abril de 2026  
**Autores:** Time de Desenvolvimento - Banco de Talentos

Para dúvidas ou sugestões sobre esta documentação, entre em contato com o time.
