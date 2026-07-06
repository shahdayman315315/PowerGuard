FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PowerGuard.WebApi/PowerGuard.WebApi.csproj PowerGuard.WebApi/
COPY PowerGuard.Application/PowerGuard.Application.csproj PowerGuard.Application/
COPY PowerGuard.Domain/PowerGuard.Domain.csproj PowerGuard.Domain/
COPY PowerGuard.Infrastructure/PowerGuard.Infrastructure.csproj PowerGuard.Infrastructure/

RUN dotnet restore PowerGuard.WebApi/PowerGuard.WebApi.csproj

COPY . .
RUN dotnet publish PowerGuard.WebApi/PowerGuard.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PowerGuard.WebApi.dll"]