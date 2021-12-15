#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Jenner.Carteira.Agendador.Worker/Jenner.Carteira.Agendador.Worker.csproj", "src/Jenner.Carteira.Agendador.Worker/"]
RUN dotnet restore "src/Jenner.Carteira.Agendador.Worker/Jenner.Carteira.Agendador.Worker.csproj"
COPY . .
WORKDIR "/src/src/Jenner.Carteira.Agendador.Worker"
RUN dotnet build "Jenner.Carteira.Agendador.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jenner.Carteira.Agendador.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jenner.Carteira.Agendador.Worker.dll"]