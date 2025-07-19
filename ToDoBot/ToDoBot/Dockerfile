## См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.
#
## Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
#FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#
#
## Этот этап используется для сборки проекта службы
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["ToDoBot/ToDoBot.csproj", "ToDoBot/"]
#RUN dotnet restore "./ToDoBot/ToDoBot.csproj"
#COPY . .
#WORKDIR "/src/ToDoBot"
#RUN dotnet build "./ToDoBot.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./ToDoBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ToDoBot.dll"]


# Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ToDoBot/ToDoBot.csproj", "ToDoBot/"]
RUN dotnet restore "ToDoBot/ToDoBot.csproj"
COPY . .
WORKDIR "/src/ToDoBot"
RUN dotnet build "ToDoBot.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ToDoBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoBot.dll"]
