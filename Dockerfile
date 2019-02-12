FROM microsoft/dotnet:sdk
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./

RUN dotnet restore   
RUN dotnet build --no-restore -c Release  
RUN dotnet pack --no-restore --no-build -c Release 