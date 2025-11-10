# LMOrders

Projeto de pedidos com arquitetura orientada a domínio, mensageria e persistência híbrida.

## Padrões e boas práticas
- **DDD (Domain-Driven Design)**: entidades (`Pedido`, `PedidoItem`) e limites de contexto separados por projetos (`Domain`, `Application`, `Infrastructure`, `Api`).
- **SOLID + DI**: serviços e repositórios expostos via interfaces e registrados com injeção de dependência (`ServiceCollectionExtensions`).
- **DTOs + AutoMapper**: camada de aplicação expõe `CriarPedidoRequest`, `PedidoResponse`, etc. Mapeamentos centralizados em `PedidoProfile`.
- **Validação e exceções de domínio**: regras de negócio lançam `DomainException` com tratamento customizado no middleware global.
- **Mensageria desacoplada**: publicação de eventos com Kafka (`PedidoCriadoEvent`) para integração assíncrona.
- **Persistência híbrida**: cabeçalho do pedido no SQL Server (EF Core), itens no MongoDB (driver oficial). Recuperação compõe os dois lados.
- **Cache distribuído**: respostas de `GET /pedidos/{id}` cacheadas por 2 minutos via Redis (`IDistributedCache`).

## Dependências principais
- .NET 8 (Web API, EF Core, AutoMapper, StackExchange.Redis, Confluent.Kafka, MongoDB.Driver)
- SQL Server
- MongoDB
- Apache Kafka
- Redis

## Estrutura do repositório
```
src/
  LMOrders.Domain           -> Entidades, enums, regras de domínio
  LMOrders.Application      -> DTOs, serviços, eventos, validação
  LMOrders.Infrastructure   -> Repositórios (SQL/Mongo), mensageria, cache
  LMOrders.Api              -> Endpoints ASP.NET Core
  LMOrders.Test             -> Integração com WebApplicationFactory
```

## Como rodar localmente

### 1. Dependências Docker
Crie/edite `docker-compose.yml` (já disponível na raiz) com MongoDB, Kafka/ZooKeeper e Redis.

Suba os serviços:
```powershell
cd "C:\Users\gabri\Documents\LM Orders"
docker compose up -d
```
Verifique:
```powershell
docker ps --filter "name=mongo-lmorders"
docker ps --filter "name=kafka"
docker ps --filter "name=redis"
```

### 2. Banco relacional (SQL Server)
Configure a connection string em `appsettings.Development.json` (já apontando para `LMOrders`).

Aplicar migrations (caso ainda não tenha feito):
```powershell
dotnet ef database update --project src/LMOrders.Infrastructure --startup-project src/LMOrders.Api
```

### 3. Executar a API
```powershell
dotnet run --project src/LMOrders.Api
```
Swagger: `https://localhost:5001/swagger`.

### 4. Fluxo de testes manuais
1. **POST `/api/v1/pedidos`** – grava pedido no SQL, itens no Mongo, publica evento `PedidoCriado`.
2. **GET `/api/v1/pedidos/{id}`** – monta resposta usando SQL + Mongo e cacheia em Redis (2 min).
3. **Mensageria** – acompanhe com:
   ```powershell
   docker exec -it kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic pedidos.criados --from-beginning
   ```
4. **MongoDB** – inspecione itens:
   ```powershell
   docker exec -it mongo-lmorders mongosh
   ```
   Dentro do shell (`>`), execute:
   ```javascript
   show dbs                 // listar bancos
   use lmorders             // acessar banco
   show collections         // listar coleções
   db.pedido_itens.find().pretty()   // visualizar documentos
   db.pedido_itens.countDocuments()  // contar documentos
   db.pedido_itens.drop()            // remover a coleção
   db.dropDatabase()                 // remover o banco (se necessário)
   ```
5. **Redis (cache)** – veja chaves/mensagens:
   ```powershell
   docker exec -it redis redis-cli keys "lmorders:pedido:*"
   ```
5. **SQL Server** – limpar pedidos (se necessário):
   ```sql
   DELETE FROM Pedidos;
   DBCC CHECKIDENT ('Pedidos', RESEED, 0); -- reinicia a sequência da PK
   ```
   > Alternativa: dropar o banco inteiro (`DROP DATABASE LMOrders;`). Ao executar a API ou rodar `dotnet ef database update`, a migration existente recria todas as tabelas. Crie novas migrations apenas quando houver mudanças de esquema (novas colunas/tabelas/relacionamentos).

## Decisões técnicas
- **Persistência híbrida**: permite escalar itens de pedidos (documento) sem comprometer integridade do cabeçalho relacional.
- **Kafka vs. fila tradicional**: mantivemos o termo “fila” nos requisitos, mas optamos por Kafka pela necessidade de integrar múltiplos consumidores com reprocessamento fácil.
- **Cache distribuído**: 2 minutos é suficiente para aliviar carga das leituras imediatamente após a criação sem comprometer consistência forte.
- **Compensação**: caso Mongo falhe, remove-se o pedido recém-criado no SQL para evitar dados órfãos.
- **Testes**: uso de `WebApplicationFactory` com fakes (Kafka, Redis) garante cobertura sem dependências externas, mantendo a API testável.

---
Para resetar o ambiente Docker:
```powershell
docker compose down
```

Boas contribuições! Abra issues/PRs conforme necessário.
