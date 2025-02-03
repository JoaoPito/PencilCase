FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5147
EXPOSE 7106

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PencilCase.API/PencilCase.API.csproj", "PencilCase.API/"]
COPY ["PencilCase.Shared.Data/PencilCase.Shared.Data.csproj", "PencilCase.Shared.Data/"]
COPY ["PencilCase.Shared.DTOs/PencilCase.Shared.DTOs.csproj", "PencilCase.Shared.DTOs/"]
COPY ["PencilCase.Shared.Models/PencilCase.Shared.Models.csproj", "PencilCase.Shared.Models/"]
COPY ["PencilCase.Telemetry.Data/PencilCase.Telemetry.Data.csproj", "PencilCase.Telemetry.Data/"]
COPY ["PencilCase.LLM/PencilCase.LLM.csproj", "PencilCase.LLM/"]

RUN dotnet restore "PencilCase.API/PencilCase.API.csproj"
COPY . .
WORKDIR "/src/PencilCase.API/"
RUN dotnet build "PencilCase.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PencilCase.API.dll"]
