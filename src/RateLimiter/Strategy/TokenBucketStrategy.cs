using System;
using RateLimiter.Core;
using RateLimiter.Cache;

namespace RateLimiter.Strategy
{
    /// <summary>
    /// An implementation of the token bucket rate limiting algorithm.
    /// See: https://wikipedia.org/wiki/Token_bucket
    /// </summary>
    public class TokenBucketStrategy : IRateLimiterStrategy
    {
        private readonly IRateLimiterCache _cache;

        public TokenBucketStrategy(IRateLimiterCache cache)
        {
            _cache = cache;
        }

        public bool PermitRequest(string key, IRateLimiter service)
        {
            TokenBucketRecord result = (TokenBucketRecord)_cache.Get(key);

            // If the rate limiter has not yet encountered this client, add a new record to the cache
            // with the StartInterval set to the current time
            if (result == null)
            {
                result = ResetInterval(key, service.Permits);
            }

            // If a record was found for the given key, determine if the interval requires resetting.
            var currentTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var intervalEnd = result.StartInterval + service.Seconds;

            if (currentTime > intervalEnd)
            {
                result = ResetInterval(key, service.Permits);
            }

            // If the requestor has no available tokens for the current interval, reject the request.
            var tokens = result.Tokens;

            if (tokens <= 0)
            {
                return false;
            }

            // Remove a token.
            _cache.Set(key, new TokenBucketRecord
            {
                StartInterval = result.StartInterval,
                Tokens = tokens - 1
            });

            return true;
        }

        public long SecondsUntilNextPermittedRequest(string key, int seconds)
        {
            TokenBucketRecord result = (TokenBucketRecord)_cache.Get(key);

            var intervalStart = result.StartInterval;
            var currentTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var intervalEnd = intervalStart + seconds;

            var secondsUntilNextPermittedRequest = intervalEnd - currentTime;

            return secondsUntilNextPermittedRequest;
        }

        /// <summary>
        /// Replenish the tokens for a given requestor.
        /// <summary>
        private TokenBucketRecord ResetInterval(string key, int tokens)
        {
            var record = new TokenBucketRecord
            {
                StartInterval = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                Tokens = tokens
            };

            _cache.Set(key, record);

            return record;
        }
    }
}
