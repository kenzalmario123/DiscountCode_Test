# Use official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY DiscountApp.sln ./
COPY src/DiscountApp.Service/ src/DiscountApp.Service/
COPY src/DiscountApp.Domain/ src/DiscountApp.Domain/
COPY src/DiscountApp.Persistence/ src/DiscountApp.Persistence/

RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish src/DiscountApp.Service/DiscountApp.Service.csproj -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DiscountApp.Service.dll"]
