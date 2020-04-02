FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY src .
RUN dotnet restore TodoList.API
RUN dotnet publish TodoList.API -c Release -o TodoList.API/out

FROM mcr.microsoft.com/dotnet/core/aspnet:5.0-buster-slim
WORKDIR /app
COPY --from=build /src/TodoList.API/out .
ENTRYPOINT ["dotnet", "TodoList.API.dll"]
