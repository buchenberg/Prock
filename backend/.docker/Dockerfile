FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app


USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Prock.Backend.csproj", "./"]
RUN dotnet restore "Prock.Backend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Prock.Backend.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Prock.Backend.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 5001 80 443
COPY --from=publish /app/publish .
ADD ./appsettings.Docker.json ./appsettings.json
ENTRYPOINT ["dotnet", "Prock.Backend.dll"]
