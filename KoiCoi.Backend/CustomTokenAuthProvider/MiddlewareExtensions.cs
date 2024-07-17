namespace KoiCoi.Backend.CustomTokenAuthProvider;

#region snippet1
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseTokenProviderMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenProviderMiddleware>();
    }
}
#endregion
