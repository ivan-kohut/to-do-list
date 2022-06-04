FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Directory.Packages.props .
COPY ["src/services/Identity/TodoList.Identity.API/TodoList.Identity.API.csproj", "/src/services/Identity/TodoList.Identity.API/"]
RUN dotnet restore "/src/services/Identity/TodoList.Identity.API/TodoList.Identity.API.csproj"
COPY src .
WORKDIR "/src/services/Identity/TodoList.Identity.API"
RUN dotnet build "TodoList.Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoList.Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.Identity.API.dll"]
