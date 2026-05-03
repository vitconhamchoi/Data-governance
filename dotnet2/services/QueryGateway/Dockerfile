FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QueryGateway.csproj", "./"]
RUN dotnet restore "QueryGateway.csproj"
COPY . .
RUN dotnet build "QueryGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QueryGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueryGateway.dll"]
