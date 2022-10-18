#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Jenner.Comum/Jenner.Comum.csproj", "src/Jenner.Comum/"]
COPY ["src/Jenner.Aplicacao.API/Jenner.Aplicacao.API.csproj", "src/Jenner.Aplicacao.API/"]
COPY ["src/Jenner.Aplicacao.API.Tests/Jenner.Aplicacao.API.Tests.csproj", "src/Jenner.Aplicacao.API.Tests/"]
RUN dotnet restore "src/Jenner.Aplicacao.API.Tests/Jenner.Aplicacao.API.Tests.csproj"
COPY . .
WORKDIR "/src/src/Jenner.Aplicacao.API"
RUN dotnet build "Jenner.Aplicacao.API.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR "/src/src/Jenner.Aplicacao.API.Tests"
RUN dotnet test "Jenner.Aplicacao.API.Tests.csproj"

FROM test AS publish
WORKDIR "/src/src/Jenner.Aplicacao.API"
RUN dotnet publish "Jenner.Aplicacao.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jenner.Aplicacao.API.dll"]