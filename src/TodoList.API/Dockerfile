FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /usr/local/share/ca-certificates
COPY certificates/todolist.crt .
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/TodoList.API/TodoList.API.csproj", "/src/TodoList.API/"]
COPY ["src/API.Models/API.Models.csproj", "/src/API.Models/"]
COPY ["src/Repositories.Concrete/Repositories.Concrete.csproj", "/src/Repositories.Concrete/"]
COPY ["src/Repositories/Repositories.csproj", "/src/Repositories/"]
COPY ["src/Services.Concrete/Services.Concrete.csproj", "/src/Services.Concrete/"]
COPY ["src/Services/Services.csproj", "/src/Services/"]
RUN dotnet restore "/src/TodoList.API/TodoList.API.csproj"
COPY src .
WORKDIR "/src/TodoList.API"
RUN dotnet build "TodoList.API.csproj" -c Release -o /app/build

FROM build as unit-tests
WORKDIR /test
COPY ["test/TodoList.API.UnitTests/TodoList.API.UnitTests.csproj", "/test/TodoList.API.UnitTests/"]
RUN dotnet restore "/test/TodoList.API.UnitTests/TodoList.API.UnitTests.csproj"
COPY test .
WORKDIR "/test/TodoList.API.UnitTests"
RUN dotnet build "TodoList.API.UnitTests.csproj" -c Release -o /app/unit-tests
WORKDIR /app/unit-tests
ENTRYPOINT ["dotnet", "test", "TodoList.API.UnitTests.dll"]

FROM build as integration-tests
WORKDIR /test
COPY ["test/TodoList.API.IntegrationTests/TodoList.API.IntegrationTests.csproj", "/test/TodoList.API.IntegrationTests/"]
RUN dotnet restore "/test/TodoList.API.IntegrationTests/TodoList.API.IntegrationTests.csproj"
COPY test .
WORKDIR "/test/TodoList.API.IntegrationTests"
RUN dotnet build "TodoList.API.IntegrationTests.csproj" -c Release -o /app/integration-tests
WORKDIR /app/integration-tests
ENTRYPOINT ["dotnet", "test", "TodoList.API.IntegrationTests.dll"]

FROM build AS publish
RUN dotnet publish "TodoList.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.API.dll"]
