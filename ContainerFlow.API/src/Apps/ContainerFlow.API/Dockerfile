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