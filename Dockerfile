#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Tautulli.MAL.Updater/Tautulli.MAL.Updater.csproj", "Tautulli.MAL.Updater/"]
COPY ["malAnimeUpdater/malAnimeUpdater.csproj", "malAnimeUpdater/"]
RUN dotnet restore "Tautulli.MAL.Updater/Tautulli.MAL.Updater.csproj"
COPY . .
WORKDIR "/src/Tautulli.MAL.Updater"
RUN dotnet build "Tautulli.MAL.Updater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tautulli.MAL.Updater.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tautulli.MAL.Updater.dll"]