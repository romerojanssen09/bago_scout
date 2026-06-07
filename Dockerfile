# Use the official Microsoft .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["BagoScout/BagoScout.csproj", "BagoScout/"]
RUN dotnet restore "BagoScout/BagoScout.csproj"

# Copy the remaining source code and build the application
COPY . .
WORKDIR "/src/BagoScout"
RUN dotnet build "BagoScout.csproj" -c Release -o /app/build

# Publish the application binaries
FROM build AS publish
RUN dotnet publish "BagoScout.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate the final runtime container image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the internal container port Railway reads
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BagoScout.dll"]
