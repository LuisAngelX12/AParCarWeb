# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AParCarWeb.csproj", "."]
RUN dotnet restore "./AParCarWeb.csproj"

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Dependencia requerida por Kerberos/GSSAPI
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AParCarWeb.dll"]