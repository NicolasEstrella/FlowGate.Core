# FlowGate.Core

Backend da plataforma FlowGate - API REST em .NET 8 com PostgreSQL.

## Stack
- .NET 8 Web API
- Entity Framework Core 8
- Npgsql (PostgreSQL)
- Docker

## Rodando localmente

### Pre-requisitos
- .NET 8 SDK
- PostgreSQL rodando (ou via Docker em FlowGate.Infra)

### Comandos
`ash
dotnet restore
dotnet run
`
API disponivel em: http://localhost:8080

### Variaveis de ambiente
| Variavel | Descricao |
|----------|-----------|
| ConnectionStrings__DefaultConnection | Connection string do Postgres |
| ASPNETCORE_ENVIRONMENT | Development ou Production |

## Migrations
`ash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
`

## Documentacao
Consulte a documentacao completa no Notion do projeto.
