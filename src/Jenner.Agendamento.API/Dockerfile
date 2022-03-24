#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Jenner.Agendamento.API/Jenner.Agendamento.API.csproj", "src/Jenner.Agendamento.API/"]
COPY ["src/Jenner.Comum/Jenner.Comum.csproj", "src/Jenner.Comum/"]
RUN dotnet restore "src/Jenner.Agendamento.API/Jenner.Agendamento.API.csproj"
COPY . .
WORKDIR "/src/src/Jenner.Agendamento.API"
RUN dotnet build "Jenner.Agendamento.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jenner.Agendamento.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jenner.Agendamento.API.dll"]