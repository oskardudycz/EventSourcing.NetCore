ARG dotnet_version=10.0
########################################
#  First stage of multistage build
########################################
#  Use Build image with label `builder
########################################
FROM mcr.microsoft.com/dotnet/sdk:${dotnet_version}-alpine AS builder
# Project file name, mandatory parameter, e.g. Helpdesk.Api
ARG project_name

# Setup working directory for project
WORKDIR /app

COPY --from=root Directory.Packages.props ./
COPY ./${project_name}.csproj ./

# Restore nuget packages
RUN dotnet restore ./${project_name}.csproj

# Copy project files
COPY ./ ./

# Generate Wolverine handlers and endpoints in Debug, where the compiler is available.
RUN dotnet run --configuration Debug -- codegen write && dotnet run --configuration Debug -- codegen test

# Restore and publish Release so Debug-only compiler dependencies are excluded.
RUN dotnet publish --configuration Release --output out ./${project_name}.csproj

# Verify that the production output contains no runtime compiler assemblies.
RUN if find out -type f \( -name 'Wolverine.RuntimeCompilation.dll' -o -name 'JasperFx.RuntimeCompiler.dll' -o -name 'Microsoft.CodeAnalysis*.dll' \) | grep -q .; then exit 1; fi

########################################
#  Second stage of multistage build
########################################
#  Use other build image as the final one
#    that won't have source codes
########################################
FROM mcr.microsoft.com/dotnet/aspnet:${dotnet_version}-alpine
ARG project_name
ARG dotnet_version=10.0

# Setup working directory for project
WORKDIR /app

# Copy published in previous stage binaries
# from the `builder` image
COPY --from=builder /app/out .

# Set URL that App will be exposed
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://*:5000"
ENV PROJECT_DLL="${project_name}.dll"

# sets entry point command to automatically
# run application on `docker run`
ENTRYPOINT dotnet $PROJECT_DLL
