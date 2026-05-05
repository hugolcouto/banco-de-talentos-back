# <p><a href="README.md"><button>⬅ Voltar ao índice</button></a></p>

# 19. Configuração de Banco de Dados In-Memory para Testes

## 💡 O que é e por que usar?

Ao escrever **Testes de Integração**, precisamos validar se nossa API se comunica corretamente com o banco de dados (salvando, editando e deletando registros). Porém, usar um banco de dados real (como SQL Server) durante os testes traz problemas:

- Os testes ficam **lentos**.
- Dados de testes diferentes podem **entrar em conflito** entre si.
- É necessário limpar o banco após cada execução.

A solução ideal é utilizar o **Entity Framework Core In-Memory Database**. Ele simula um banco de dados que vive inteiramente na memória RAM da aplicação, sendo extremamente rápido, descartável e isolado.

---

## 📐 A Regra de Ouro da Arquitetura

> **A sua aplicação principal NÃO DEVE saber que está sendo testada.**

É um anti-padrão colocar `if (ambiente_de_teste)` dentro do seu `InfrastructureModule.cs` ou ler variáveis de ambiente para decidir qual banco usar. A responsabilidade de alterar o banco de dados deve ser **exclusiva do projeto de testes**.

A nossa abordagem sequestra (intercepta) a Injeção de Dependência da API antes dela rodar no teste, remove a configuração do SQL Server e coloca o In-Memory no lugar.

---

## 🛠️ Passo a Passo da Implementação

### Passo 1: Limpar a configuração da API principal

Garanta que o seu `InfrastructureModule.cs` (na camada de Infrastructure) apenas registre o banco de dados oficial (SQL Server), sem condicionais de teste.

```csharp
// Arquivo: BancoDeTalentos.Infrastructure/InfrastructureModule.cs
private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
{
    string dbConnectionString = configuration.GetConnectionString("SqlConnectionString")!;

    // Configuração única e limpa para produção/desenvolvimento
    services.AddDbContext<BancoDeTalentosDbContext>(
        o => o.UseSqlServer(dbConnectionString)
    );

    return services;
}
```

### Passo 2: Criar a Factory de Testes

No projeto de testes, usamos a classe `WebApplicationFactory<Program>`. Ela serve para subir a API inteira em memória. Vamos herdá-la e sobrescrever o método `ConfigureWebHost`.

```csharp
// Arquivo: BancoDeTalentos.Tests/Factories/TestingWebApplicationFactory.cs
using BancoDeTalentos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BancoDeTalentos.Tests.Factories;

public class TestingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Criamos um nome único para o banco em memória para ESTA execução
            string dbName = Guid.NewGuid().ToString();

            // 2. Buscamos todas as configurações de banco registradas pela API oficial
            List<ServiceDescriptor> descriptors = services.Where(
                d => d.ServiceType.Name.Contains("DbContextOptions")
            ).ToList();

            // 3. Removemos as configurações do SQL Server
            foreach (ServiceDescriptor descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // 4. Adicionamos o EF Core apontando para o banco In-Memory
            services.AddDbContext<BancoDeTalentosDbContext>(
                options => options.UseInMemoryDatabase(dbName)
            );
        });
    }
}
```

---

## 🚨 Possíveis Erros e Como Evitá-los

Abaixo listamos os dois problemas mais comuns ao configurar bancos In-Memory e como o nosso código foi desenhado para evitá-los.

### ❌ Erro 1: Múltiplos provedores de banco registrados

**O Erro:**

> _"Services for database providers 'Microsoft.EntityFrameworkCore.SqlServer', 'Microsoft.EntityFrameworkCore.InMemory' have been registered... Only a single database provider can be registered."_

**Por que acontece?**
O Entity Framework Core "esconde" várias opções dentro do container de dependências quando você chama `.AddDbContext()`. Se você tentar remover apenas typeof(`DbContextOptions<MeuContexto>`), o provedor original (SQL Server) ainda ficará acumulado internamente. Quando o In-Memory entra, o EF Core tenta rodar ambos e gera o conflito.

**A Solução Segura:**
Buscar e remover **tudo** que contenha "DbContextOptions" no nome do serviço. Isso varre as configurações genéricas e as internas de providers acumulados.

```csharp
// ✅ JEITO CERTO
List<ServiceDescriptor> descriptors = services.Where(
    d => d.ServiceType.Name.Contains("DbContextOptions")
).ToList();
```

### ❌ Erro 2: Dados somem entre requisições (Ex: POST retorna 201, mas GET retorna 404)

**O Erro:**
Você faz um `POST` no teste para criar uma Empresa e ela retorna HTTP 201 (Sucesso). Logo em seguida, faz um `GET` buscando o ID criado e a API retorna HTTP 404 (Não Encontrado).

**Por que acontece?**
O ciclo de vida do `DbContext` em APIs .NET é **Scoped** (uma nova instância é gerada para cada requisição HTTP).
Se você gerar o `Guid.NewGuid().ToString()` diretamente **dentro** da função lambda do `UseInMemoryDatabase`, o EF Core vai criar um banco com um nome diferente para _cada chamada HTTP_.

- O seu `POST` salva no banco "db-123".
- O seu `GET` busca no banco "db-456" (que está vazio).

**A Solução Segura:**
Gerar o nome do banco **fora** da lambda. Assim, o `Guid` é resolvido apenas uma vez quando a Factory inicializa. Todas as requisições HTTP disparadas por aquele client de teste cairão no mesmo banco.

```csharp
// ✅ JEITO CERTO
string dbName = Guid.NewGuid().ToString(); // Resolvido fora

services.AddDbContext<MeuContexto>(
    options => options.UseInMemoryDatabase(dbName)
);


// ❌ JEITO ERRADO
services.AddDbContext<MeuContexto>(
    options => options.UseInMemoryDatabase(Guid.NewGuid().ToString())
);
```

---

## ✅ Checklist de Sucesso

- [ ] `InfrastructureModule.cs` limpo, sem `if` para testes.
- [ ] `TestingWebApplicationFactory` varrendo dependências com `.Contains("DbContextOptions")`.
- [ ] `TestingWebApplicationFactory` gerando o nome do banco fora do delegate `.UseInMemoryDatabase()`.
- [ ] Testes de controlador herdando de `IClassFixture<TestingWebApplicationFactory>`.

---

**Referências de Arquivo:**

- `BancoDeTalentos.Tests/Factories/TestingWebApplicationFactory.cs`
- `BancoDeTalentos.Infrastructure/InfrastructureModule.cs`
