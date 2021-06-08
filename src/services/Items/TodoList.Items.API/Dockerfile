FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /usr/local/share/ca-certificates
COPY certificates/todolist.crt .
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/services/Items/TodoList.Items.API/TodoList.Items.API.csproj", "/src/services/Items/TodoList.Items.API/"]
COPY ["src/services/Items/TodoList.Items.Infrastructure/TodoList.Items.Infrastructure.csproj", "/src/services/Items/TodoList.Items.Infrastructure/"]
COPY ["src/services/Items/TodoList.Items.Domain/TodoList.Items.Domain.csproj", "/src/services/Items/TodoList.Items.Domain/"]
COPY ["src/services/Items/TodoList.Items.API.Models/TodoList.Items.API.Models.csproj", "/src/services/Items/TodoList.Items.API.Models/"]
RUN dotnet restore "/src/services/Items/TodoList.Items.API/TodoList.Items.API.csproj"
COPY src .
WORKDIR "/src/services/Items/TodoList.Items.API"
RUN dotnet build "TodoList.Items.API.csproj" -c Release -o /app/build

FROM build as unit-tests
WORKDIR /test
COPY ["test/TodoList.Items.API.UnitTests/TodoList.Items.API.UnitTests.csproj", "/test/TodoList.Items.API.UnitTests/"]
RUN dotnet restore "/test/TodoList.Items.API.UnitTests/TodoList.Items.API.UnitTests.csproj"
COPY test .
WORKDIR "/test/TodoList.Items.API.UnitTests"
RUN dotnet build "TodoList.Items.API.UnitTests.csproj" -c Release -o /app/unit-tests
WORKDIR /app/unit-tests
ENTRYPOINT ["dotnet", "test", "TodoList.Items.API.UnitTests.dll"]

FROM build as integration-tests
WORKDIR /test
COPY ["test/TodoList.Items.API.IntegrationTests/TodoList.Items.API.IntegrationTests.csproj", "/test/TodoList.Items.API.IntegrationTests/"]
RUN dotnet restore "/test/TodoList.Items.API.IntegrationTests/TodoList.Items.API.IntegrationTests.csproj"
COPY test .
WORKDIR "/test/TodoList.Items.API.IntegrationTests"
RUN dotnet build "TodoList.Items.API.IntegrationTests.csproj" -c Release -o /app/integration-tests
WORKDIR /app/integration-tests
ENTRYPOINT ["dotnet", "test", "TodoList.Items.API.IntegrationTests.dll"]

FROM build AS publish
RUN dotnet publish "TodoList.Items.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.Items.API.dll"]
