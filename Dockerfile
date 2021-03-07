FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /app
COPY . .
RUN dotnet restore "/app/tests/RateLimiterTests/RateLimiterTests.csproj"
WORKDIR /app/tests/RateLimiterTests
ENTRYPOINT ["dotnet", "test"]