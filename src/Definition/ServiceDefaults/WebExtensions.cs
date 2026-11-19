using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Perigon.AspNetCore.Converters;
using ServiceDefaults.Middleware;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;

namespace ServiceDefaults;

public static class WebExtensions
{
    public static IServiceCollection AddMiddlewareServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddAuthentication(builder.Configuration);
        services.AddThirdAuthentication(builder.Configuration);
        services.AddAuthorize();
        services.AddCors(builder.Configuration);
        services.AddRateLimiter();
        services.AddLocalizer();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost);
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        });
        return services;
    }

    public static WebApplication UseMiddlewareServices(this WebApplication app)
    {
        app.UseForwardedHeaders();
        if (app.Environment.IsProduction())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        app.UseStaticFiles();
        app.UseRequestLocalization();
        app.UseRouting();
        app.UseCors(app.Environment.IsProduction() ? WebConst.Limited : WebConst.Default);
        app.UseStatusCodePages();
        app.UseRateLimiter();
        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();
        }
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>
    /// 添加速率限制
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // for limited policy
            options.AddPolicy(
                WebConst.Limited,
                context =>
                {
                    var remoteIpAddress = context.Connection.RemoteIpAddress;
                    return !IPAddress.IsLoopback(remoteIpAddress!)
                        ? RateLimitPartition.GetFixedWindowLimiter(
                            remoteIpAddress!.ToString(),
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 10,
                                Window = TimeSpan.FromSeconds(60),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 3,
                            }
                        )
                        : RateLimitPartition.GetNoLimiter(remoteIpAddress!.ToString());
                }
            );

            // 全局限制 每10秒100次
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            {
                IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;

                return !IPAddress.IsLoopback(remoteIpAddress!)
                    ? RateLimitPartition.GetFixedWindowLimiter(
                        remoteIpAddress!,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 3,
                        }
                    )
                    : RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
            });
        });
        return services;
    }

    /// <summary>
    /// 添加本地化支持
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLocalizer(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddRequestLocalization(options =>
        {
            //  add more cultures if needed
            var supportedCultures = new[] { "zh-CN", "en-US" };
            options
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        services.AddSingleton<Localizer>();
        return services;
    }

    /// <summary>
    /// 添加 jwt 验证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                var componentOption = configuration.GetSection(ComponentOption.ConfigPath).Get<ComponentOption>();
                var jwtOption = configuration.GetSection(JwtOption.ConfigPath).Get<JwtOption>();
                var oauthOption = configuration.GetSection(OAuthOption.ConfigPath).Get<OAuthOption>();

                if (componentOption?.AuthType == AuthType.Jwt)
                {
                    var sign = jwtOption?.Sign;
                    if (string.IsNullOrEmpty(sign))
                    {
                        throw new Exception("未找到有效的Jwt配置");
                    }
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sign)),
                        ValidIssuer = jwtOption?.ValidIssuer,
                        ValidAudience = jwtOption?.ValidAudiences,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                    };
                }
                else if (componentOption?.AuthType == AuthType.OAuth)
                {
                    options.Authority = oauthOption?.Authority;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudiences = oauthOption?.Audiences,
                        ClockSkew = TimeSpan.FromMinutes(5),
                    };
                    options.RequireHttpsMetadata = oauthOption?.RequireHttpsMetadata ?? true;
                }
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: {0}", context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    },
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
        return services;
    }

    /// <summary>
    /// 添加第三方认证（如微软）
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddThirdAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var section = configuration.GetSection("Authentication");
        var msClientId = section.GetValue<string>("Microsoft:ClientId");
        var msClientSecret = section.GetValue<string>("Microsoft:ClientSecret");
        var msCallBackUrl = section.GetValue<string>("Microsoft:CallbackUrl");

        if (Utils.NoEmptyItem(msClientId, msClientSecret, msCallBackUrl))
        {
            services
                .AddAuthentication()
                .AddMicrosoftAccount(
                    MicrosoftAccountDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.AuthorizationEndpoint =
                            "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                        options.TokenEndpoint =
                            "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                        options.ClientId = msClientId!;
                        options.ClientSecret = msClientSecret!;
                        options.CallbackPath = msCallBackUrl;
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    }
                );
        }

        var googleClientId = section.GetValue<string>("Google:ClientId");
        var googleClientSecret = section.GetValue<string>("Google:ClientSecret");
        var googleCallBackUrl = section.GetValue<string>("Google:CallbackUrl");
        if (Utils.NoEmptyItem(googleClientId, googleClientSecret, googleCallBackUrl))
        {
            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId!;
                    options.ClientSecret = googleClientSecret!;
                    options.CallbackPath = googleCallBackUrl;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });
        }

        return services;
    }

    public static IServiceCollection AddCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var section = configuration.GetSection("Cors");
        //get origins array
        // Read the primitive configuration values without ConfigurationBinder.Get<T>.
        // The generic binder uses reflection and is not trim-safe for Native AOT.
        var origins = section?.GetSection("AllowedOrigins").GetChildren()
            .Select(child => child.Value)
            .OfType<string>()
            .ToArray() ?? [];
        var allowedSubdomains = bool.TryParse(section?["AllowedSubdomains"], out var parsedAllowedSubdomains)
            && parsedAllowedSubdomains;

        services.AddCors(options =>
        {
            options.AddPolicy(
                WebConst.Default,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
            options.AddPolicy(
                WebConst.Limited,
                builder =>
                {
                    builder.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
                    if (allowedSubdomains)
                    {
                        builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    }
                }
            );
        });
        return services;
    }

    public static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                WebConst.Default,
                policy => policy.RequireAuthenticatedUser()
            )
            .AddPolicy(
                WebConst.User,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(WebConst.User, WebConst.AdminUser, WebConst.SuperAdmin)
            );

        return services;
    }
}
