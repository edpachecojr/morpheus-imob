# Imagem da API do morpheus-core. Multi-stage: o SDK compila e publica; a imagem
# final carrega só o runtime ASP.NET, sem o SDK, para ficar pequena e com menor
# superfície de ataque. Contexto de build: a pasta morpheus-core/.

# --- Estágio de build ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /origem

# Copia só os .csproj primeiro para o restore virar camada cacheável: enquanto as
# dependências não mudam, o Docker reaproveita esta camada entre builds.
COPY src/Morpheus.Api/Morpheus.Api.csproj src/Morpheus.Api/
COPY src/Morpheus.Aplicacao/Morpheus.Aplicacao.csproj src/Morpheus.Aplicacao/
COPY src/Morpheus.Dominio/Morpheus.Dominio.csproj src/Morpheus.Dominio/
COPY src/Morpheus.Infraestrutura/Morpheus.Infraestrutura.csproj src/Morpheus.Infraestrutura/
RUN dotnet restore src/Morpheus.Api/Morpheus.Api.csproj

COPY src/ src/
RUN dotnet publish src/Morpheus.Api/Morpheus.Api.csproj -c Release -o /publicado --no-restore

# --- Estágio de runtime ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publicado .

# Não roda como root: $APP_UID é o usuário sem privilégios que a imagem base define.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Morpheus.Api.dll"]
