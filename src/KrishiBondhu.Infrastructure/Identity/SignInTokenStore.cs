using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace KrishiBondhu.Infrastructure.Identity;

public interface ISignInTokenStore
{
    string CreateToken(ClaimsPrincipal principal, AuthenticationProperties properties);
    (ClaimsPrincipal Principal, AuthenticationProperties Properties)? ConsumeToken(string token);
}

public class InMemorySignInTokenStore : ISignInTokenStore
{
    private class TokenEntry
    {
        public required ClaimsPrincipal Principal { get; set; }
        public required AuthenticationProperties Properties { get; set; }
        public DateTime ExpiryUtc { get; set; }
    }

    private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new();

    public string CreateToken(ClaimsPrincipal principal, AuthenticationProperties properties)
    {
        var now = DateTime.UtcNow;
        // Purge expired tokens
        foreach (var kvp in _tokens)
        {
            if (kvp.Value.ExpiryUtc < now)
            {
                _tokens.TryRemove(kvp.Key, out _);
            }
        }

        var token = Guid.NewGuid().ToString("N");
        _tokens[token] = new TokenEntry
        {
            Principal = principal,
            Properties = properties,
            ExpiryUtc = now.AddMinutes(5)
        };
        return token;
    }

    public (ClaimsPrincipal Principal, AuthenticationProperties Properties)? ConsumeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        if (_tokens.TryRemove(token, out var entry))
        {
            if (entry.ExpiryUtc >= DateTime.UtcNow)
            {
                return (entry.Principal, entry.Properties);
            }
        }
        return null;
    }
}
