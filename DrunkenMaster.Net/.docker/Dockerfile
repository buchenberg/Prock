FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app


USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DrunkenMaster.Net.csproj", "./"]
RUN dotnet restore "DrunkenMaster.Net.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DrunkenMaster.Net.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DrunkenMaster.Net.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://*:5001
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DrunkenMaster.Net.dll"]
