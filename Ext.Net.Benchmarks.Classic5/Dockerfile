#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

WORKDIR /root/.nuget/NugetFeed
COPY ./Ext.Net.Benchmarks.Classic/NugetFeed ./
COPY ./Ext.Net.Benchmarks.Classic/NugetFeed/NuGet.Config /root/.nuget/NuGet/NuGet.Config

WORKDIR /src
COPY ["Ext.Net.Benchmarks.Classic5/Ext.Net.Benchmarks.Classic5.csproj", "Ext.Net.Benchmarks.Classic5/"]
COPY ["Ext.Net.Benchmarks.Common/Ext.Net.Benchmarks.Common.csproj", "Ext.Net.Benchmarks.Common/"]
RUN dotnet restore "Ext.Net.Benchmarks.Classic5/Ext.Net.Benchmarks.Classic5.csproj"
COPY . .
WORKDIR "/src/Ext.Net.Benchmarks.Classic5"
RUN dotnet build "Ext.Net.Benchmarks.Classic5.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ext.Net.Benchmarks.Classic5.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ext.Net.Benchmarks.Classic5.dll"]