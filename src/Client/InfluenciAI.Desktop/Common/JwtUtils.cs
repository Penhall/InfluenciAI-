using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace InfluenciAI.Desktop.Common;

public static class JwtUtils
{
    private sealed class JwtPayload
    {
        public string? name { get; set; }
        public string? sub { get; set; }
        public string? tenant { get; set; }
        public string[]? role { get; set; }
    }

    public static (string? Name, Guid? TenantId, string[] Roles) Parse(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt)) return (null, null, Array.Empty<string>());
        var parts = jwt.Split('.');
        if (parts.Length < 2) return (null, null, Array.Empty<string>());
        try
        {
            var payloadJson = Base64UrlDecode(parts[1]);
            var payload = JsonSerializer.Deserialize<JwtPayload>(payloadJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Guid? tenant = null;
            if (Guid.TryParse(payload?.tenant, out var t)) tenant = t;
            var roles = payload?.role ?? Array.Empty<string>();
            return (payload?.name, tenant, roles);
        }
        catch
        {
            return (null, null, Array.Empty<string>());
        }
    }

    private static string Base64UrlDecode(string input)
    {
        string padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        var bytes = Convert.FromBase64String(padded);
        return Encoding.UTF8.GetString(bytes);
    }
}

