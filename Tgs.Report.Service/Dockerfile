#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Report.Service/Tgs.Report.Service.csproj", "Tgs.Report.Service/"]
COPY ["Tgs.Report.Core/Tgs.Report.Core.csproj", "Tgs.Report.Core/"]
COPY ["Tgs.Report.Repository/Tgs.Report.Repository.csproj", "Tgs.Report.Repository/"]
COPY ["Tgs.Report.Enum/Tgs.Report.Enum.csproj", "Tgs.Report.Enum/"]
COPY ["Tgs.Report.ViewModel/Tgs.Report.ViewModel.csproj", "Tgs.Report.ViewModel/"]
RUN dotnet restore "./Tgs.Report.Service/./Tgs.Report.Service.csproj"
COPY . .
WORKDIR "/src/Tgs.Report.Service"
RUN dotnet build "./Tgs.Report.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.Report.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Report.Service.dll"]