#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Report.Service/Tgs.Report.Service.csproj", "Tgs.Report.Service/"]
COPY ["Tgs.Report.Core/Tgs.Report.Core.csproj", "Tgs.Report.Core/"]
COPY ["Tgs.Report.Repository/Tgs.Report.Repository.csproj", "Tgs.Report.Repository/"]
COPY ["Tgs.Report.Enum/Tgs.Report.Enum.csproj", "Tgs.Report.Enum/"]
COPY ["Tgs.Report.ViewModel/Tgs.Report.ViewModel.csproj", "Tgs.Report.ViewModel/"]
RUN dotnet restore "Tgs.Report.Service/Tgs.Report.Service.csproj"
COPY . .
WORKDIR "/src/Tgs.Report.Service"
RUN dotnet build "Tgs.Report.Service.csproj" -c ${BuildConfiguration} -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Report.Service.csproj" -c ${BuildConfiguration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Report.Service.dll"]