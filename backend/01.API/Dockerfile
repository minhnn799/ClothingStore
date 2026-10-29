FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["01.API/01. API.csproj", "01.API/"]
COPY ["02. Application/02. Application.csproj", "02. Application/"]
COPY ["03. Infrastructure/03. Infrastructure.csproj", "03. Infrastructure/"]
COPY ["04.Domain/04. Domain.csproj", "04.Domain/"]
RUN dotnet restore "01.API/01. API.csproj"
COPY . .
RUN dotnet publish "01.API/01. API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/uploads && chown -R app:app /app/uploads
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "01. API.dll"]
