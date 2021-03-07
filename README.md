## Introduction

An implementation of a [rate limiter](https://en.wikipedia.org/wiki/Rate_limiting#Web_servers).

## Design

The rate limiter was designed to act as a gateway between the client and the server. This was achieved by leveraging [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0). This approach allows the rate limiter to block requests from moving down the application pipeline if the request is invalid.

The rate limiter implementation has been designed to be easily extended. User defined rate limiting strategies and caching implementations can be specified.

## Limitations

Currently, generics are not supported for caching implementations. This makes implementations of some caching strategies nonviable.

For the sake of brevity, clients are idenfitied by their IP address.

## Usage

Include the following in the `Configure` method in `Startup.cs`

This **must** come before the `UseEndpoints` middleware.

```csharp
app.UseRateLimiter(
	new RateLimiterService(
		100,
		3600,
		new TokenBucketStrategy(new InMemoryCache(new MemoryCache("RateLimiterCache")))
	)
);
```

## Setup

A sample application has been included for demonstrative purposes, you can run it using the following command in the root directory:

`docker-compose up --build`

You can issue requests to the demo application using curl:

`curl http://localhost:5000/WeatherForecast`

By default the demo application permits 100 requests each hour, however this can be modified to a lower number of permits or a shorter timeframe to demonstrate how the rate limiter drops requests.

You can also run the test container using the following command in the root directory:

`docker build -t ratelimitertests . && docker run -it ratelimitertests`
