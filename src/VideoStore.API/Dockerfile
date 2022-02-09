#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["./VideoStore.API/VideoStore.API.csproj", "VideoStore.API/"]
COPY ["./VideoStore.Core/VideoStore.Core.csproj", "VideoStore.Core/"]

RUN dotnet restore "VideoStore.API/VideoStore.API.csproj"
COPY . .
WORKDIR "VideoStore.API"
RUN dotnet build "VideoStore.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoStore.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoStore.API.dll"]