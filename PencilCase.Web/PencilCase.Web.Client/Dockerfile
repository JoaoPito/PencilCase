FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PencilCase.Web/PencilCase.Web.Client/PencilCase.Web.Client.csproj", "PencilCase.Web/PencilCase.Web.Client/"]
COPY ["PencilCase.Shared.DTOs/PencilCase.Shared.DTOs.csproj", "PencilCase.Shared.DTOs/"]
COPY ["PencilCase.Shared.Models/PencilCase.Shared.Models.csproj", "PencilCase.Shared.Models/"]

RUN dotnet restore "PencilCase.Web/PencilCase.Web.Client/PencilCase.Web.Client.csproj"
COPY . .
WORKDIR "/src/PencilCase.Web/PencilCase.Web.Client"
RUN dotnet build "PencilCase.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PencilCase.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PencilCase.Web.Client.dll"]
