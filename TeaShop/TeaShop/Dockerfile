FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TeaShop.csproj", "./"]
RUN dotnet restore


COPY Program.cs ./
COPY appsettings.json ./
COPY Application/ ./Application/
COPY Domain/ ./Domain/
COPY Infrastructure/ ./Infrastructure/
COPY Migrations/ ./Migrations/
COPY Presentation/ ./Presentation/
COPY Properties/ ./Properties/


RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "TeaShop.dll"]