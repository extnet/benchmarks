#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /root/.nuget/NugetFeed
COPY ./Ext.Net.Benchmarks.Classic/NugetFeed ./

WORKDIR /Users/ContainerUser/AppData/Roaming/NuGet
COPY ./Ext.Net.Benchmarks.Classic/NugetFeed/NuGet.Config ./NuGet.Config

WORKDIR /src
COPY ["Ext.Net.Benchmarks.Classic/Ext.Net.Benchmarks.Classic.csproj", "Ext.Net.Benchmarks.Classic/"]
COPY ["Ext.Net.Benchmarks.Common/Ext.Net.Benchmarks.Common.csproj", "Ext.Net.Benchmarks.Common/"]
RUN dotnet restore "Ext.Net.Benchmarks.Classic/Ext.Net.Benchmarks.Classic.csproj"
COPY . .
WORKDIR "/src/Ext.Net.Benchmarks.Classic"
RUN dotnet build "Ext.Net.Benchmarks.Classic.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ext.Net.Benchmarks.Classic.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ext.Net.Benchmarks.Classic.dll"]