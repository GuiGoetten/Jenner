#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Jenner.Consultar.Worker/Jenner.Consultar.Worker.csproj", "src/Jenner.Consultar.Worker/"]
COPY ["src/Jenner.Comum/Jenner.Comum.csproj", "src/Jenner.Comum/"]
RUN dotnet restore "src/Jenner.Consultar.Worker/Jenner.Consultar.Worker.csproj"
COPY . .
WORKDIR "/src/src/Jenner.Consultar.Worker"
RUN dotnet build "Jenner.Consultar.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jenner.Consultar.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jenner.Consultar.Worker.dll"]