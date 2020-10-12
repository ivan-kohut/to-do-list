FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/TodoList.Client.Server/TodoList.Client.Server.csproj", "/src/TodoList.Client.Server/"]
COPY ["src/TodoList.Client/TodoList.Client.csproj", "/src/TodoList.Client/"]
COPY ["src/API.Models/API.Models.csproj", "/src/API.Models/"]
RUN dotnet restore "/src/TodoList.Client.Server/TodoList.Client.Server.csproj"
COPY src .
WORKDIR "/src/TodoList.Client.Server"
RUN dotnet build "TodoList.Client.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoList.Client.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.Client.Server.dll"]
