# Use the official Microsoft image that contains the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file and restore any dependencies (via the dotnet restore command)
COPY *.csproj ./
RUN dotnet restore

# Copy the entire project and build it
COPY . ./
RUN dotnet publish -c Release -o out

# Use the runtime image for a smaller image size
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the build output to the runtime image
COPY --from=build /app/out ./

# Define environment variables
ENV ZuneNetContext__ConnectionString="mongodb://mongo:27017"
ENV ZuneNetContext__DatabaseName="Zune"

# Specify the command to run the application
ENTRYPOINT ["dotnet", "YourApp.dll"]
