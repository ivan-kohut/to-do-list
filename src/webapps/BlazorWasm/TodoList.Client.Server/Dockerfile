FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /usr/local/share/ca-certificates
COPY certificates/todolist.crt .
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Directory.Packages.props .
COPY ["src/webapps/BlazorWasm/TodoList.Client.Server/TodoList.Client.Server.csproj", "/src/webapps/BlazorWasm/TodoList.Client.Server/"]
COPY ["src/webapps/BlazorWasm/TodoList.Client/TodoList.Client.csproj", "/src/webapps/BlazorWasm/TodoList.Client/"]
COPY ["src/services/Items/TodoList.Items.API.Models/TodoList.Items.API.Models.csproj", "/src/services/Items/TodoList.Items.API.Models/"]
RUN dotnet restore "/src/webapps/BlazorWasm/TodoList.Client.Server/TodoList.Client.Server.csproj"
COPY src .
WORKDIR "/src/webapps/BlazorWasm/TodoList.Client.Server"
RUN dotnet build "TodoList.Client.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoList.Client.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.Client.Server.dll"]
