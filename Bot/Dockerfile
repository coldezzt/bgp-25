FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Reglamentator.Bot/Reglamentator.Bot.csproj Reglamentator.Bot/

COPY . .

RUN dotnet restore Reglamentator.Bot/Reglamentator.Bot.csproj
RUN dotnet publish Reglamentator.Bot/Reglamentator.Bot.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reglamentator.Bot.dll"]
