FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["incidenskezelo-backend.csproj", "./"]
RUN dotnet restore "incidenskezelo-backend.csproj"

COPY . .
RUN dotnet publish "incidenskezelo-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

RUN dotnet tool restore --tool-manifest dotnet-tools.json
# Inert build-time placeholder: EF needs a connection string to build the
# design-time host; the real one comes from the environment when the bundle runs.
RUN ConnectionStrings__DefaultConnection="Server=placeholder" \
    dotnet ef migrations bundle --project incidenskezelo-backend.csproj --configuration Release -o /app/migrate

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS backend
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
COPY --from=build /app/migrate ./migrate
ENTRYPOINT ["dotnet", "incidenskezelo-backend.dll"]
