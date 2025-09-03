# =========================
# build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj для кэшируемого restore
COPY OnlineStore.sln ./
COPY OnlineStore.Api/OnlineStore.Api.csproj OnlineStore.Api/
COPY OnlineStore.Application/OnlineStore.Application.csproj OnlineStore.Application/
COPY OnlineStore.Domain/OnlineStore.Domain.csproj OnlineStore.Domain/
COPY OnlineStore.Infrastructure/OnlineStore.Infrastructure.csproj OnlineStore.Infrastructure/

RUN dotnet restore OnlineStore.Api/OnlineStore.Api.csproj

# Копируем остальной код и публикуем
COPY . .
RUN dotnet publish OnlineStore.Api/OnlineStore.Api.csproj -c Release -o /app/publish

# =========================
# runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Слушаем внутри контейнера на 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OnlineStore.Api.dll"]
