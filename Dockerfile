#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNET_VERSION=6.0.5
ENV DOTNET_VERSION=6.0.5
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["ESIAppTest/ESIAppTest.csproj", "ESIAppTest/"]
RUN dotnet nuget add source https://pkgs.dev.azure.com/SJI-WebSolutions/_packaging/SignetNuGet/nuget/v3/index.json --name AWSDDSMicroServicesNugets -u anything -p 6pticdhhfv4dgivqbnfnpef5mtosnve2nnq5ccarzvwzc7njjfna --store-password-in-clear-text
RUN dotnet restore "ESIAppTest/ESIAppTest.csproj"
COPY . .
WORKDIR "/src/ESIAppTest"
RUN dotnet build "ESIAppTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ESIAppTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESIAppTest.dll"]