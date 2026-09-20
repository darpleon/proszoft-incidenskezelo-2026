FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["incidenskezelo-backend.csproj", "./"]
RUN dotnet restore "incidenskezelo-backend.csproj"

COPY . .
RUN dotnet publish "incidenskezelo-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "incidenskezelo-backend.dll"]
