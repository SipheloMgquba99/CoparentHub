FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CoparentHub.Api/CoparentHub.Api.csproj CoparentHub.Api/
COPY CoparentHub.Application/CoparentHub.Application.csproj CoparentHub.Application/
COPY CoparentHub.Domain/CoparentHub.Domain.csproj CoparentHub.Domain/
COPY CoparentHub.Infrastructure/CoparentHub.Infrastructure.csproj CoparentHub.Infrastructure/
COPY CoparentHub.Persistence/CoparentHub.Persistence.csproj CoparentHub.Persistence/
RUN dotnet restore CoparentHub.Api/CoparentHub.Api.csproj

COPY . .
RUN dotnet publish CoparentHub.Api/CoparentHub.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoparentHub.Api.dll"]
