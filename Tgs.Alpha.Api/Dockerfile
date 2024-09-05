FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Alpha.Api/Tgs.Alpha.Api.csproj", "Tgs.Alpha.Api/"]
COPY ["Tgs.Report.Core/Tgs.Report.Core.csproj", "Tgs.Report.Core/"]
COPY ["Tgs.Report.Repository/Tgs.Report.Repository.csproj", "Tgs.Report.Repository/"]
COPY ["Tgs.Report.Enum/Tgs.Report.Enum.csproj", "Tgs.Report.Enum/"]
COPY ["Tgs.Report.ViewModel/Tgs.Report.ViewModel.csproj", "Tgs.Report.ViewModel/"]
RUN dotnet restore "./Tgs.Alpha.Api/./Tgs.Alpha.Api.csproj"
COPY . .
WORKDIR "/src/Tgs.Alpha.Api"
RUN dotnet build "./Tgs.Alpha.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.Alpha.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Alpha.Api.dll"]