#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Fibo.FiboServer.App/Fibo.FiboServer.App.csproj", "Fibo.FiboServer.App/"]
RUN dotnet restore "Fibo.FiboServer.App/Fibo.FiboServer.App.csproj"
COPY . .
WORKDIR "/src/Fibo.FiboServer.App"
RUN dotnet build "Fibo.FiboServer.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fibo.FiboServer.App.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fibo.FiboServer.App.dll"]