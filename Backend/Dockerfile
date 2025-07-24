FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Reglamentator.WebAPI/Reglamentator.WebAPI.csproj Reglamentator.WebAPI/
COPY Reglamentator.Application/Reglamentator.Application.csproj Reglamentator.Application/
COPY Reglamentator.Data/Reglamentator.Data.csproj Reglamentator.Data/
COPY Reglamentator.Domain/Reglamentator.Domain.csproj Reglamentator.Domain/

COPY . .

RUN dotnet restore Reglamentator.WebAPI/Reglamentator.WebAPI.csproj
RUN dotnet publish Reglamentator.WebAPI/Reglamentator.WebAPI.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reglamentator.WebAPI.dll"]
