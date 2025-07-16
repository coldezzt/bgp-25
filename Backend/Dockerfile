FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend/Reglamentator.WebAPI/Reglamentator.WebAPI.csproj Backend/Reglamentator.WebAPI/
COPY Backend/Reglamentator.Application/Reglamentator.Application.csproj Backend/Reglamentator.Application/
COPY Backend/Reglamentator.Data/Reglamentator.Data.csproj Backend/Reglamentator.Data/
COPY Backend/Reglamentator.Domain/Reglamentator.Domain.csproj Backend/Reglamentator.Domain/
RUN dotnet restore Backend/Reglamentator.WebAPI/Reglamentator.WebAPI.csproj

COPY . .
RUN dotnet publish Backend/Reglamentator.WebAPI/Reglamentator.WebAPI.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reglamentator.WebAPI.dll"]
