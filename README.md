# ContainerFlow API

Este repositório serve como um projeto de estudo e aplicação prática avançada de conceitos de **Domain-Driven Design (DDD)**, **Monólito Modular**, **Transactional Outbox/Inbox** e **Event-Driven Architecture (EDA)** no ecossistema **.NET 8**.

Para uma análise detalhada dos Bounded Contexts, fluxo de eventos e diagramas de domínio, consulte a [Documentação de Arquitetura (ARCHITECTURE.md)](ARCHITECTURE.md).

---

# Guia de Execução Local e Debug com Docker Compose

Este documento fornece as instruções oficiais de engenharia para provisionar, executar, manter e depurar localmente o ambiente completo da aplicação (**API .NET 8** e **Microsoft SQL Server 2022**) utilizando contêineres Docker.

---

## Índice
1. [Visão Geral e Arquitetura de Execução](#1-visão-geral-e-arquitetura-de-execução)
2. [Pré-requisitos](#2-pré-requisitos)
3. [Arquivos de Configuração de Referência](#3-arquivos-de-configuração-de-referência)
   - [3.1. `docker-compose.yml` ](#31-docker-composeyml)
   - [3.2. `docker-compose.override.yml` ](#32-docker-composeoverrideyml)
   - [3.3. `Dockerfile` ](#33-dockerfile-multi-stage-otimizado)
   - [3.4. `.env.example`](#34-envexample)
4. [Instruções de Execução Passo a Passo](#4-instruções-de-execução-passo-a-passo)
   - [4.1. Setup Inicial](#41-setup-inicial)
   - [4.2. Inicialização dos Serviços](#42-inicialização-dos-serviços)
   - [4.3. Execução de Migrações do Banco de Dados (EF Core)](#43-execução-de-migrações-do-banco-de-dados-ef-core)
5. [Guia Completo de Debugging](#5-guia-completo-de-debugging)
   - [Estratégia 1: Debug Local Nativo no Host + Banco no Container](#estratégia-1-debug-local-nativo-no-host--banco-no-container)
   - [Estratégia 2: Remote Debugging (API e Banco em Containers)](#estratégia-2-remote-debugging-api-e-banco-em-containers)
   - [Debug e Inspeção Direta do Banco de Dados](#debug-e-inspeção-direta-do-banco-de-dados)
   - [Análise de Logs e Troubleshooting de Rede](#análise-de-logs-e-troubleshooting-de-rede)
6. [Comandos Úteis de Manutenção (Cheat Sheet)](#6-comandos-úteis-de-manutenção-cheat-sheet)
7. [Troubleshooting / Resolução de Problemas](#7-troubleshooting--resolução-de-problemas)

---

## 1. Visão Geral e Arquitetura de Execução

A aplicação é composta por uma API ASP.NET Core (.NET 8) modular e um banco relacional **Microsoft SQL Server 2022** que abriga os esquemas segregados de domínio (`clientes`, `vendas`, `engenharia`, `financeiro`, `eventos`), além das tabelas do `Hangfire` e `ASP.NET Core Identity`.

### Topologia de Rede: Host vs. Docker Bridge Network

```
 ┌────────────────────────────────────────────────────────────────────────┐
 │                              HOST MACHINE                              │
 │                                                                        │
 │   Navegador / Swagger: http://localhost:8080/swagger                  │
 │   IDE Local / Debugger: http://localhost:5104 ou Remote Attach (4026) │
 │   DBeaver / Azure Data Studio: localhost:1433                          │
 └───────────────────┬─────────────────────────────────┬──────────────────┘
                     │ Porta Mapeada 8080:8080         │ Porta Mapeada 1433:1433
                     │                                 │
 ┌───────────────────▼─────────────────────────────────▼──────────────────┐
 │               DOCKER BRIDGE NETWORK (`containerflow-net`)              │
 │                                                                        │
 │   ┌───────────────────────────┐      ┌─────────────────────────────┐   │
 │   │      Serviço: `api`       │      │   Serviço: `sqlserver`      │   │
 │   │                           │ DNS  │                             │   │
 │   │  ASP.NET Core Web API     ├─────►│  MS SQL Server 2022         │   │
 │   │  Porta interna: 8080      │      │  Porta interna: 1433        │   │
 │   │  Debugger (vsdbg): 4026   │      │  Healthcheck ativo          │   │
 │   └───────────────────────────┘      └──────────────┬──────────────┘   │
 │                                                     │                  │
 └─────────────────────────────────────────────────────┼──────────────────┘
                                                       │ Volume Persistente
                                                ┌──────▼──────┐
                                                │ mssql_data  │
                                                └─────────────┘
```

### Regras Críticas de Resolução de Nomes (DNS):
1. **Comunicação Container ➔ Container:** A API dentro do container resolve o banco através do **hostname do serviço** (`sqlserver`) fornecido pelo DNS interno do Docker Compose:  
   `Server=sqlserver,1433;Database=ContainerFlowDB;...`
2. **Comunicação Host ➔ Container:** Qualquer processo rodando fora do Docker (sua IDE local, Azure Data Studio, DBeaver, terminal executando `dotnet ef`) **não enxerga** o nome de host `sqlserver`. O Host deve sempre usar a porta mapeada no loopback:  
   `Server=localhost,1433;Database=ContainerFlowDB;...`
3. **Loopback (`localhost`) dentro do container:** Se a API dentro do container tentar conectar em `localhost:1433`, a requisição será direcionada para a própria pilha de rede do container da API, falhando com conexão recusada.

---

## 2. Pré-requisitos

Certifique-se de possuir instalado em sua estação de desenvolvimento:

- **Docker Desktop** (Windows/macOS com engine WSL2 ativada) ou **Docker Engine + Docker Compose Plugin v2+** (Linux).
- **.NET 8.0 SDK** (obrigatório caso utilize a Estratégia 1 de Debug Local Nativo).
- **Git**.
- **Ferramenta de visualização de banco de dados**: [Azure Data Studio](https://azure.microsoft.com/pt-br/products/data-studio), [DBeaver Community](https://dbeaver.io/) ou [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms).
- **IDEs recomendadas**:
  - **VS Code**: Com as extensões *C# Dev Kit* (`ms-dotnettools.csdevkit`) e *Docker* (`ms-azuretools.vscode-docker`).
  - **Visual Studio 2022** (v17.8+) com cargas *ASP.NET e desenvolvimento Web* e *Ferramentas de Desenvolvimento de Contêiner*.
  - **JetBrains Rider** (2023.3+).

---

## 3. Arquivos de Configuração de Referência

### 3.1. `docker-compose.yml`

Arquivo de orquestração padrão, com suporte a healthcheck integrado no SQL Server, rede isolada e controle determinístico de inicialização (`depends_on` com `condition: service_healthy`):

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: containerflow-sqlserver
    hostname: sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "${DB_PASSWORD:-YourStrong@Passw0rd}"
      MSSQL_PID: "Developer"
    ports:
      - "${DB_PORT:-1433}:1433"
    volumes:
      - mssql_data:/var/opt/mssql
    networks:
      - containerflow-net
    healthcheck:
      test: [
        "CMD-SHELL",
        "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"${DB_PASSWORD:-YourStrong@Passw0rd}\" -C -Q 'SELECT 1' || /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P \"${DB_PASSWORD:-YourStrong@Passw0rd}\" -Q 'SELECT 1'"
      ]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 15s

  api:
    image: containerflow-api:local
    container_name: containerflow-api
    build:
      context: ./ContainerFlow.API
      dockerfile: src/Apps/ContainerFlow.API/Dockerfile
      target: final
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: "${ASPNETCORE_ENVIRONMENT:-Development}"
      ASPNETCORE_HTTP_PORTS: "8080"
      ConnectionStrings__ContainerFlowDB: "Server=sqlserver,1433;Database=${DB_NAME:-ContainerFlowDB};User Id=sa;Password=${DB_PASSWORD:-YourStrong@Passw0rd};TrustServerCertificate=True;MultipleActiveResultSets=true;"
      ConnectionStrings__IdentityDB: "Server=sqlserver,1433;Database=${IDENTITY_DB_NAME:-IdentityDB};User Id=sa;Password=${DB_PASSWORD:-YourStrong@Passw0rd};TrustServerCertificate=True;MultipleActiveResultSets=true;"
    ports:
      - "${API_PORT:-8080}:8080"
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - containerflow-net

networks:
  containerflow-net:
    driver: bridge

volumes:
  mssql_data:
    driver: local
```

---

### 3.2. `docker-compose.override.yml`

> **Como funciona:** O Docker Compose mescla automaticamente o `docker-compose.override.yml` com o `docker-compose.yml`. Este arquivo configura o serviço `api` para o estágio de desenvolvimento com o SDK, ativando **Hot Reload (`dotnet watch`)** via volume e expondo a porta de depuração remota (`4026`).

```yaml
services:
  api:
    build:
      context: ./ContainerFlow.API
      dockerfile: src/Apps/ContainerFlow.API/Dockerfile
      target: dev
    command: dotnet watch run --project src/Apps/ContainerFlow.API/ContainerFlow.API.csproj --non-interactive --urls "http://0.0.0.0:8080"
    ports:
      - "${DEBUG_PORT:-4026}:4026"
    volumes:
      # Montagem do código-fonte para hot reload
      - ./ContainerFlow.API:/app/src
      # Volumes anônimos: evitam que binários gerados pelo Host (Windows) colidam com binários Linux dentro do container
      - /app/src/src/Apps/ContainerFlow.API/bin
      - /app/src/src/Apps/ContainerFlow.API/obj
      - /app/src/src/Contextos/ContainerFlow.Clientes/bin
      - /app/src/src/Contextos/ContainerFlow.Clientes/obj
      - /app/src/src/Contextos/ContainerFlow.Vendas/bin
      - /app/src/src/Contextos/ContainerFlow.Vendas/obj
      - /app/src/src/Contextos/ContainerFlow.Engenharia/bin
      - /app/src/src/Contextos/ContainerFlow.Engenharia/obj
      - /app/src/src/Contextos/ContainerFlow.Financeiro/bin
      - /app/src/src/Contextos/ContainerFlow.Financeiro/obj
      - /app/src/src/ContainerFlow.Contracts/bin
      - /app/src/src/ContainerFlow.Contracts/obj
      - /app/src/src/ContainerFlow.DDD/bin
      - /app/src/src/ContainerFlow.DDD/obj
    environment:
      DOTNET_USE_POLLING_FILE_WATCHER: "1"
      ASPNETCORE_DETAILEDERRORS: "1"
```

---

### 3.3. `Dockerfile` Multi-Stage Otimizado

Localização: `ContainerFlow.API/src/Apps/ContainerFlow.API/Dockerfile`  
Contexto de build do Docker: `./ContainerFlow.API` (raiz da solução .NET, garantindo acesso aos projetos de contratos e bounded contexts).

```dockerfile
# ==============================================================================
# ESTÁGIO 1: Runtime Base para Produção (mínimo e seguro)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ==============================================================================
# ESTÁGIO 2: Estágio de Desenvolvimento e Debug Remoto (com SDK e vsdbg)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /app/src

# Instala ferramentas essenciais e o depurador remoto oficial da Microsoft (vsdbg)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl procps unzip \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

EXPOSE 8080
EXPOSE 4026

# ==============================================================================
# ESTÁGIO 3: Compilação e Cache Seletivo de Dependências (Build Cache-Friendly)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia a solução e os arquivos .csproj isoladamente para maximizar o cache do 'dotnet restore'
COPY Directory.Build.props ./
COPY ContainerFlow.sln ./
COPY src/Apps/ContainerFlow.API/ContainerFlow.API.csproj src/Apps/ContainerFlow.API/
COPY src/ContainerFlow.Contracts/ContainerFlow.Contracts.csproj src/ContainerFlow.Contracts/
COPY src/ContainerFlow.DDD/ContainerFlow.DDD.csproj src/ContainerFlow.DDD/
COPY src/Contextos/ContainerFlow.Clientes/ContainerFlow.Clientes.csproj src/Contextos/ContainerFlow.Clientes/
COPY src/Contextos/ContainerFlow.Vendas/ContainerFlow.Vendas.csproj src/Contextos/ContainerFlow.Vendas/
COPY src/Contextos/ContainerFlow.Engenharia/ContainerFlow.Engenharia.csproj src/Contextos/ContainerFlow.Engenharia/
COPY src/Contextos/ContainerFlow.Financeiro/ContainerFlow.Financeiro.csproj src/Contextos/ContainerFlow.Financeiro/
COPY tests/ContainerFlow.Testes/ContainerFlow.Testes.csproj tests/ContainerFlow.Testes/

# Restaura pacotes NuGet em camada isolada de cache
RUN dotnet restore ContainerFlow.sln

# Copia todo o código-fonte restante e compila
COPY . .
WORKDIR /src/src/Apps/ContainerFlow.API
RUN dotnet build ContainerFlow.API.csproj -c $BUILD_CONFIGURATION -o /app/build

# ==============================================================================
# ESTÁGIO 4: Publicação dos Binários Otimizados
# ==============================================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish ContainerFlow.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==============================================================================
# ESTÁGIO 5: Imagem Final de Execução
# ==============================================================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "ContainerFlow.API.dll"]
```

---

### 3.4. `.env.example`

```env
# ==============================================================================
# BANCO DE DADOS (MICROSOFT SQL SERVER)
# ==============================================================================
DB_PASSWORD=YourStrong@Passw0rd
DB_NAME=ContainerFlowDB
IDENTITY_DB_NAME=IdentityDB
DB_PORT=1433

# ==============================================================================
# API (.NET 8)
# ==============================================================================
API_PORT=8080
DEBUG_PORT=4026
ASPNETCORE_ENVIRONMENT=Development
```

---

## 4. Instruções de Execução Passo a Passo

### 4.1. Setup Inicial

No terminal, na raiz do projeto:

```bash
# Copie o arquivo de variáveis de ambiente
cp .env.example .env

# No Windows PowerShell:
Copy-Item .env.example .env
```

> [!IMPORTANT]
> A senha padrão do SQL Server (`YourStrong@Passw0rd`) atende à política obrigatória de complexidade (letras maiúsculas, minúsculas, números e caracteres especiais). Senhas simples impedem a inicialização do container do SQL Server.

---

### 4.2. Inicialização dos Serviços

Inicie os contêineres:

```bash
docker compose up -d --build
```

#### Verificação de Status e Saúde dos Containers:

```bash
docker compose ps
```

Saída esperada:
```text
NAME                     IMAGE                     COMMAND                  SERVICE      STATUS                    PORTS
containerflow-sqlserver  mssql/server:2022-latest  "/opt/mssql/bin/sqls…"   sqlserver    Up 30s (healthy)          0.0.0.0:1433->1433/tcp
containerflow-api        containerflow-api:local   "dotnet watch run --…"   api          Up 20s                    0.0.0.0:8080->8080/tcp, 0.0.0.0:4026->4026/tcp
```

Após o status do `sqlserver` indicar **`(healthy)`**, acesse a documentação interativa Swagger:  
👉 **http://localhost:8080/swagger**

---

### 4.3. Execução de Migrações do Banco de Dados (EF Core)

A solução conta com dois contextos de banco de dados (`AppDbContext` e `IdentityDbContext`).

#### Método 1: Pelo Host (Recomendado se possuir o .NET SDK instalado)
```bash
cd ContainerFlow.API

# 1. Aplicar migrações de Domínio, Bounded Contexts, Outbox e Inbox
dotnet ef database update --project src/Apps/ContainerFlow.API/ContainerFlow.API.csproj --context AppDbContext --connection "Server=localhost,1433;Database=ContainerFlowDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"

# 2. Aplicar migrações do ASP.NET Identity (Usuários e Autenticação)
dotnet ef database update --project src/Apps/ContainerFlow.API/ContainerFlow.API.csproj --context IdentityDbContext --connection "Server=localhost,1433;Database=IdentityDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
```

#### Método 2: Via Container Efêmero do Docker (Sem ter o .NET instalado no Host)
```bash
docker compose run --rm api dotnet ef database update \
  --project src/Apps/ContainerFlow.API/ContainerFlow.API.csproj \
  --context AppDbContext \
  --connection "Server=sqlserver,1433;Database=ContainerFlowDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
```

---

## 5. Guia Completo de Debugging

### Estratégia 1: Debug Local Nativo no Host + Banco no Container
> **Melhor para o dia a dia:** Sem emulação, com IntelliSense em velocidade máxima e breakpoints instantâneos no Visual Studio, Rider ou VS Code.

1. **Suba apenas o serviço do SQL Server:**
   ```bash
   docker compose up -d sqlserver
   ```
2. **Defina as Connection Strings locais via User Secrets:**
   ```bash
   cd ContainerFlow.API/src/Apps/ContainerFlow.API

   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:ContainerFlowDB" "Server=localhost,1433;Database=ContainerFlowDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;"
   dotnet user-secrets set "ConnectionStrings:IdentityDB" "Server=localhost,1433;Database=IdentityDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;"
   ```
3. **Execute o Debug:**
   - No **Visual Studio 2022 / Rider**: Selecione o perfil `http` (porta local `5104`) e pressione **`F5`**.
   - No **VS Code**: Pressione **`F5`** com o profile `.NET Core Launch (web)`.
   - Seus breakpoints serão atingidos instantaneamente no código dos Bounded Contexts e nos jobs em segundo plano do Hangfire.

---

### Estratégia 2: Remote Debugging (API e Banco em Containers)
> **Melhor para paridade de ambiente:** Execução idêntica a contêineres de produção em Linux.

#### Configuração para VS Code (`.vscode/launch.json`):
Crie o arquivo `.vscode/launch.json` na raiz:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core: Attach to Docker Container",
      "type": "coreclr",
      "request": "attach",
      "pipeTransport": {
        "pipeProgram": "docker",
        "pipeArgs": ["exec", "-i", "containerflow-api"],
        "debuggerPath": "/vsdbg/vsdbg",
        "pipeCwd": "${workspaceFolder}"
      },
      "sourceFileMap": {
        "/app/src": "${workspaceFolder}/ContainerFlow.API"
      }
    }
  ]
}
```

1. Suba o ambiente: `docker compose up -d`
2. No VS Code, abra o painel **Run and Debug** (`Ctrl+Shift+D`), escolha **`.NET Core: Attach to Docker Container`** e pressione **`F5`**.
3. Selecione o processo `dotnet ContainerFlow.API.dll` (ou processo filho do `dotnet watch`).
4. Seus breakpoints locais serão ativados via mapeamento de diretórios do `sourceFileMap`.

#### Configuração para Visual Studio 2022:
1. Abra a solução `ContainerFlow.sln`.
2. Vá em **Depurar (Debug) > Anexar ao Processo (Attach to Process...)** (`Ctrl+Alt+P`).
3. Em **Tipo de Conexão**, selecione: **`Docker (Contêiner do Linux)`**.
4. Em **Destino da Conexão**, selecione `containerflow-api`.
5. Localize o processo `dotnet` e clique em **Anexar (Attach)**.

---

### Debug e Inspeção Direta do Banco de Dados

#### Terminal Interativo do SQL Server (SQLCMD):
```bash
docker compose exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C
```

Consultas úteis (digite o comando e finalize com `GO`):
```sql
-- 1. Verificar bases de dados
SELECT name FROM sys.databases;
GO

-- 2. Conectar na base ContainerFlowDB
USE ContainerFlowDB;
GO

-- 3. Inspecionar tabelas criadas
SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
GO

-- 4. Inspecionar eventos pendentes no Outbox (Transactional Outbox)
SELECT TOP 5 Id, TipoEvento, DataCriacaoUtc FROM eventos.OutboxMessages ORDER BY DataCriacaoUtc DESC;
GO

-- 5. Inspecionar eventos consumidos no Inbox (Consumo Idempotente)
SELECT TOP 5 Id, TipoLeitor, ProcessadoEmUtc FROM eventos.InboxMessages ORDER BY ProcessadoEmUtc DESC;
GO

QUIT
```

#### Conectando Ferramentas Visuais (Azure Data Studio, DBeaver, SSMS):
- **Host:** `localhost`
- **Port:** `1433`
- **User:** `sa`
- **Password:** `YourStrong@Passw0rd`
- **Criptografia / SSL:** Marque explicitamente a opção **Trust Server Certificate** (`TrustServerCertificate=True`).

---

### Análise de Logs e Troubleshooting de Rede

```bash
# Seguir logs da API em tempo real
docker compose logs -f --tail=100 api

# Seguir logs do SQL Server
docker compose logs -f --tail=100 sqlserver

# Inspecionar detalhes da rede virtual
docker network inspect containerflow-net

# Acessar o bash do container da API para testes de conectividade
docker compose exec -it api bash

# Teste 1: Resolver DNS do banco
getent hosts sqlserver

# Teste 2: Validar socket TCP na porta 1433
cat < /dev/tcp/sqlserver/1433 && echo "Porta 1433 acessível!"

# Teste 3: Chamar o Swagger internamente
curl -I http://localhost:8080/swagger/index.html
```

---

## 6. Comandos Úteis de Manutenção (Cheat Sheet)

| Operação | Comando |
| :--- | :--- |
| **Subir todo o ambiente em segundo plano** | `docker compose up -d` |
| **Subir apenas o banco de dados** | `docker compose up -d sqlserver` |
| **Parar containers mantendo o estado** | `docker compose stop` |
| **Derrubar containers mantendo dados dos volumes** | `docker compose down` |
| **Reset Completo (Derrubar containers e APAGAR volumes de banco)** | `docker compose down -v` |
| **Reconstruir imagens sem cache** | `docker compose build --no-cache` |
| **Reiniciar apenas a API** | `docker compose restart api` |
| **Limpar containers e imagens órfãs** | `docker system prune -f` |

---

## 7. Troubleshooting / Resolução de Problemas

### 1. Conflito de Porta: `bind: address already in use: 1433`
- **Causa:** Uma instância local do SQL Server já está em execução no host.
- **Solução:** No Windows, encerre o serviço local em `services.msc` (*SQL Server (MSSQLSERVER)*) ou altere a porta externa no seu `.env`:
  ```env
  DB_PORT=14333
  ```
  *(A API no container continuará comunicando na porta interna 1433 sem impactos)*.

### 2. Erro de Certificado SSL: `The certificate chain was issued by an authority that is not trusted`
- **Causa:** No .NET 8 e EF Core 9, o driver `Microsoft.Data.SqlClient` exige conexões criptografadas por padrão. Em ambientes de desenvolvimento locais sem certificado comercial emitido por autoridade certificadora (CA), a conexão falha.
- **Solução:** Adicione sempre `TrustServerCertificate=True;` na Connection String (conforme configurado em nosso `docker-compose.yml` e nos exemplos de User Secrets).

### 3. Falha de Conexão na Inicialização da API (Crash no Startup)
- **Causa:** A API tentou se conectar antes de o SQL Server estar pronto para aceitar requisições.
- **Solução:** O `docker-compose.yml` utiliza `condition: service_healthy` vinculado ao `healthcheck` oficial do SQL Server. Nunca remova esse bloco para assegurar que a API aguarde o término do processo de boot do banco.

### 4. Breakpoints não são atingidos no Remote Debugger
- **Causa 1:** O `sourceFileMap` no `.vscode/launch.json` deve apontar obrigatoriamente para `"${workspaceFolder}/ContainerFlow.API"`.
- **Causa 2:** Os binários foram compilados em modo `Release` (otimizações do compilador removem os símbolos de depuração `.pdb`). Assegure que a imagem utilize o target `dev` com configuração `Debug`.
- **Causa 3:** Conflito de binários locais gerados no Windows com os binários montados no container Linux. Verifique se os volumes anônimos para `/bin` e `/obj` continuam presentes no `docker-compose.override.yml`.

### 5. Hot Reload (`dotnet watch`) não detecta arquivos modificados no Windows/WSL2
- **Causa:** Notificações de alteração do sistema de arquivos podem não ser transmitidas entre o host Windows e o contêiner Linux.
- **Solução:** A variável de ambiente `DOTNET_USE_POLLING_FILE_WATCHER: "1"` (já inclusa no `docker-compose.override.yml`) força o .NET a monitorar periodicamente os arquivos em disco por amostragem.
