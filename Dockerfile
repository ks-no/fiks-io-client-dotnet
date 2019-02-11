FROM microsoft/dotnet:sdk
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore

