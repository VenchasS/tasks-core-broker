FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskExecutor/TaskExecutor.csproj", "./TaskExecutor/"]
COPY Shared/Shared.csproj ./Shared/
RUN dotnet restore "TaskExecutor/TaskExecutor.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "TaskExecutor/TaskExecutor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskExecutor/TaskExecutor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskExecutor.dll"]
