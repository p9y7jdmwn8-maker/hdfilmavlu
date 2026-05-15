FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["VideoSite.csproj", "./"]
RUN dotnet restore "VideoSite.csproj"

COPY . .
RUN dotnet publish "VideoSite.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DISABLE_HTTPS_REDIRECT=true

COPY --from=build /app/publish .

RUN mkdir -p /app/App_Data /app/wwwroot/uploads/videos /app/wwwroot/uploads/posters

EXPOSE 8080
ENTRYPOINT ["dotnet", "VideoSite.dll"]
