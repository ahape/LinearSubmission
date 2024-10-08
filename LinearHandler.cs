using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Diagnostics.CodeAnalysis;

namespace LinearSubmission;

public class LinearHandler : OAuthHandler<OAuthOptions>
{
    public const string SchemeName = "Linear";

    [Obsolete("ISystemClock is obsolete, use TimeProvider on AuthenticationSchemeOptions instead.")]
    public LinearHandler(IOptionsMonitor<OAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    public LinearHandler(IOptionsMonitor<OAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
#pragma warning disable CS0618
        : base(options, logger, encoder, default!)
#pragma warning restore CS0618
    { }

    protected override async Task<AuthenticationTicket> CreateTicketAsync(
        ClaimsIdentity identity,
        AuthenticationProperties properties,
        OAuthTokenResponse tokens)
    {
        if (string.IsNullOrEmpty(tokens?.AccessToken))
            throw new InvalidOperationException(nameof(tokens.AccessToken));

        if (identity == null)
            throw new ArgumentNullException(nameof(identity));

        var claimValue = tokens.AccessToken;
        if (!identity.HasClaim(ClaimTypes.Name, claimValue))
        {
            identity.AddClaim(new(ClaimTypes.Name, claimValue));
        }
        var principal = new ClaimsPrincipal(identity);
        var context = new OAuthCreatingTicketContext(
            principal,
            properties,
            Context,
            Scheme,
            Options,
            Backchannel,
            tokens,
            default);

        context.RunClaimActions();

        await Events.CreatingTicket(context);

        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}
