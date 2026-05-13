# Correção do `InternalServerError` em `POST /api/vagas`

## Resultado da análise

A execução de `dotnet test` no projeto de integração de vagas falha em `BancoDeTalentos.Tests.Integrations.JobControllerTests.Job_Flow_Should_Work` porque a chamada `POST /api/vagas` retorna `500 InternalServerError` em vez de sucesso.

O ponto exato da falha aparece em [BancoDeTalentos.Tests/Integrations/JobControllerTests.cs](../BancoDeTalentos.Tests/Integrations/JobControllerTests.cs) na asserção do método `CreateJob`, onde o teste espera `OK` e recebe `InternalServerError`.

## Causa raiz

O controller de vagas depende de `IJobService`, mas esse serviço não está registrado no módulo de aplicação.

Além disso, o `JobService` depende de `IJobRepository`, e esse repositório também não está registrado no módulo de infraestrutura.

Isso faz com que a requisição para `POST /api/vagas` falhe na resolução de dependências e seja interceptada pelo handler global de exceções, que converte a falha em `500 InternalServerError`.

## Evidências no código

- [BancoDeTalentos.Application/ApplicationModule.cs](../BancoDeTalentos.Application/ApplicationModule.cs): registra apenas `ICompanyService`.
- [BancoDeTalentos.Infrastructure/InfrastructureModule.cs](../BancoDeTalentos.Infrastructure/InfrastructureModule.cs): registra apenas `ICompanyRepository`.
- [BancoDeTalentos.API/Middleware/ApiExceptionHandler.cs](../BancoDeTalentos.API/Middleware/ApiExceptionHandler.cs): converte exceções não tratadas em `500`.
- [BancoDeTalentos.Tests/Integrations/JobControllerTests.cs](../BancoDeTalentos.Tests/Integrations/JobControllerTests.cs): a falha ocorre na criação da vaga.

## Passo a passo para correção

1. Registrar `IJobService` no módulo de aplicação, ao lado de `ICompanyService`.
2. Registrar `IJobRepository` no módulo de infraestrutura, ao lado de `ICompanyRepository`.
3. Executar novamente o teste de integração de vagas com `dotnet test BancoDeTalentos.Tests/BancoDeTalentos.Tests.csproj --filter JobControllerTests`.
4. Confirmar que o `POST /api/vagas` deixa de retornar `500`.
5. Se o teste passar da criação, revisar as próximas asserções do fluxo, porque o teste hoje espera `OK` no `POST`, mas o controller usa `CreatedAtAction`.
6. Validar se a resposta do endpoint deve mesmo ser `200 OK` ou se o teste deve ser ajustado para `201 Created`, caso essa seja a intenção funcional.
7. Reexecutar a suíte completa de testes para garantir que a correção não quebrou os cenários de empresa.

## Observação importante

Mesmo depois da correção da DI, o fluxo pode continuar inconsistente por causa da expectativa do teste em [BancoDeTalentos.Tests/Integrations/JobControllerTests.cs](../BancoDeTalentos.Tests/Integrations/JobControllerTests.cs): o teste espera `OK` na criação da vaga, mas o controller retorna `CreatedAtAction`.

Se o comportamento esperado for criação REST padrão, o mais coerente é manter `201 Created` e ajustar o teste. Se o contrato do projeto exigir `200 OK`, então o controller precisará ser revisado futuramente.
