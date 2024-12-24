FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PencilCase.Web/PencilCase.Web/PencilCase.Web.csproj", "PencilCase.Web/PencilCase.Web/"]
COPY ["PencilCase.Web/PencilCase.Web.Client/PencilCase.Web.Client.csproj", "PencilCase.Web/PencilCase.Web.Client/"]
RUN dotnet restore "PencilCase.Web/PencilCase.Web/PencilCase.Web.csproj"
COPY . .
WORKDIR "/src/PencilCase.Web/PencilCase.Web"
RUN dotnet build "PencilCase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PencilCase.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PencilCase.Web.dll"]
