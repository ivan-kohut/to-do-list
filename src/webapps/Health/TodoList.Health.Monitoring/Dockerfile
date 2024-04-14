FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /usr/local/share/ca-certificates
COPY certificates/todolist.crt .
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Packages.props .
COPY ["src/webapps/Health/TodoList.Health.Monitoring/TodoList.Health.Monitoring.csproj", "/src/webapps/Health/TodoList.Health.Monitoring/"]
RUN dotnet restore "/src/webapps/Health/TodoList.Health.Monitoring/TodoList.Health.Monitoring.csproj"
COPY src .
WORKDIR "/src/webapps/Health/TodoList.Health.Monitoring"
RUN dotnet build "TodoList.Health.Monitoring.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoList.Health.Monitoring.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.Health.Monitoring.dll"]
