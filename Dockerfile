FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ThAmCo.WebApi/ThAmCo.WebApi.csproj", "src/ThAmCo.WebApi/"]
RUN dotnet restore "src/ThAmCo.WebApi/ThAmCo.WebApi.csproj"
COPY . .
WORKDIR "/src/src/ThAmCo.WebApi"
RUN dotnet build "ThAmCo.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ThAmCo.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThAmCo.WebApi.dll"]
