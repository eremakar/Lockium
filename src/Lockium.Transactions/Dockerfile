#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Lockium.Transactions/Lockium.Transactions.csproj", "Lockium.Transactions/"]
RUN dotnet restore "Lockium.Transactions/Lockium.Transactions.csproj"
COPY . .
WORKDIR "/src/Lockium.Transactions"
RUN dotnet build "Lockium.Transactions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lockium.Transactions.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lockium.Transactions.dll"]
