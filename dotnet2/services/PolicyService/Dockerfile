FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PolicyService.csproj", "./"]
RUN dotnet restore "PolicyService.csproj"
COPY . .
RUN dotnet build "PolicyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PolicyService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PolicyService.dll"]
