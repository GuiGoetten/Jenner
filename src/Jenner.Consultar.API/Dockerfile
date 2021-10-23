#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Jenner.Consultar.API/Jenner.Consultar.API.csproj", "src/Jenner.Consultar.API/"]
COPY ["src/Jenner.Comum/Jenner.Comum.csproj", "src/Jenner.Comum/"]
RUN dotnet restore "src/Jenner.Consultar.API/Jenner.Consultar.API.csproj"
COPY . .
WORKDIR "/src/src/Jenner.Consultar.API"
RUN dotnet build "Jenner.Consultar.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jenner.Consultar.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jenner.Consultar.API.dll"]