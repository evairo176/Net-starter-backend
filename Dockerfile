# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NetStarter.Api/NetStarter.Api.csproj NetStarter.Api/
RUN dotnet restore NetStarter.Api/NetStarter.Api.csproj
COPY NetStarter.Api/ NetStarter.Api/
RUN dotnet publish NetStarter.Api/NetStarter.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetStarter.Api.dll"]