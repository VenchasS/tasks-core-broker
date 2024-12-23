FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TaskQueue.csproj ./
RUN dotnet restore "TaskQueue.csproj"
COPY . ./
WORKDIR "/src"
RUN dotnet build "TaskQueue.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskQueue.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskQueue.dll"]
