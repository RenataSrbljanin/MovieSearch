# 1. SDK Image za Build (ovde kompajliramo kod)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiraj .sln i sve .csproj fajlove da bi uradili restore (ovo ubrzava build)
COPY ["MovieSearch.sln", "./"]
COPY ["src/MovieSearch.Api/MovieSearch.Api.csproj", "src/MovieSearch.Api/"]
COPY ["src/MovieSearch.Application/MovieSearch.Application.csproj", "src/MovieSearch.Application/"]
COPY ["src/MovieSearch.Infrastructure/MovieSearch.Infrastructure.csproj", "src/MovieSearch.Infrastructure/"]
COPY ["tests/MovieSearch.Tests/MovieSearch.Tests.csproj", "tests/MovieSearch.Tests/"]

RUN dotnet restore

# Kopiraj ostatak koda i uradi publish
COPY . .

# build puca ako testovi ne prođu, pa ih pokrećem pre publish-a
RUN dotnet test "tests/MovieSearch.Tests/MovieSearch.Tests.csproj" -c Release --no-restore

WORKDIR "/src/src/MovieSearch.Api"
RUN dotnet publish "MovieSearch.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Image (ovaj deo ide na server, lagan je i brz)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Ekspozuj port na kojem aplikacija sluša
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MovieSearch.Api.dll"]