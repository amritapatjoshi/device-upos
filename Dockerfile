# Use an official .NET Framework 4.8 SDK runtime as the base image
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .NET Framework application files to the container
COPY . ./

# Build the .NET Framework application
RUN msbuild /p:Configuration=Release

# Expose the port the application will run on
EXPOSE 3000

# Start the .NET Framework application when the container starts
CMD ["dotnet", "upos-device-simulation.dll" ]