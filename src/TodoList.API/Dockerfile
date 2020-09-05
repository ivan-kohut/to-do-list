FROM mcr.microsoft.com/dotnet/core/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0 AS build
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

FROM build AS publish
RUN dotnet publish "TodoList.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.API.dll"]
