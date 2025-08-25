using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using FluentValidation;
using GloboClima.API.Auth;
using GloboClima.API.DTOs;
using GloboClima.API.Exceptions;
using GloboClima.API.Middleware;
using GloboClima.API.Models;
using GloboClima.API.Services;
using GloboClima.API.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text;
using System.Text.Json;
using System.Reflection;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
            });

        builder.Services.AddValidatorsFromAssemblyContaining<FavoriteInputValidator>();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "GloboClima API", Version = "v1" });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira 'Bearer {token}'"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddHttpClient<WeatherService>(client => 
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddHttpClient<CountryService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddHttpClient<CitySearchService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<JwtService>();
        builder.Services.AddScoped<FavoriteService>();
        
        builder.Services.AddValidatorsFromAssemblyContaining<FavoriteInputValidator>();
        builder.Services.AddAWSService<IAmazonDynamoDB>();

        var jwtKey = builder.Configuration["Jwt:Key"]
                     ?? Environment.GetEnvironmentVariable("JWT_KEY")
                     ?? throw new InvalidOperationException("JWT Key is not configured");

        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ClockSkew = TimeSpan.Zero
            };
        });
        builder.Services.AddDefaultAWSOptions(new AWSOptions
        {
            Region = RegionEndpoint.USEast2
        });

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ConfiguredCors", policy =>
            {
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });  

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                if (contextFeature?.Error != null)
                {
                    context.Response.ContentType = "application/json";
                    
                    if (contextFeature.Error is FluentValidation.ValidationException validationException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            title = "Validation Error",
                            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            detail = "One or more validation errors occurred.",
                            errors = validationException.Errors
                        });
                    }
                    else if (contextFeature.Error is ApiException apiException)
                    {
                        context.Response.StatusCode = (int)apiException.StatusCode;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            status = apiException.StatusCode,
                            title = apiException.Title,
                            type = apiException.Type,
                            detail = apiException.Message,
                            errors = apiException.Errors
                        });
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            status = StatusCodes.Status500InternalServerError,
                            title = "Internal Server Error",
                            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                            detail = "An unexpected error occurred.",
                            traceId = context.TraceIdentifier
                        });
                    }
                }
            });
        });

        app.UseSwagger();
        app.UseSwaggerUI();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;
                
                context.Response.ContentType = "application/json";
                
                object response;
                switch (exception)
                {
                    case ApiException apiEx:
                        response = new { StatusCode = (int)apiEx.StatusCode, Message = apiEx.Message };
                        context.Response.StatusCode = (int)apiEx.StatusCode;
                        break;
                    case FluentValidation.ValidationException valEx:
                        response = new { StatusCode = 400, Message = "Validation failed", Errors = valEx.Errors };
                        context.Response.StatusCode = 400;
                        break;
                    default:
                        response = new { StatusCode = 500, Message = "An unexpected error occurred" };
                        context.Response.StatusCode = 500;
                        break;
                }
                
                await context.Response.WriteAsJsonAsync(response);
            });
        });
            // app.UseHsts(); // disabled: running HTTP only
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
        });

        // app.UseHttpsRedirection(); // disabled: running HTTP only
        app.UseCors("ConfiguredCors");
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        
        app.UseMiddleware<ErrorHandlingMiddleware>();
        
        app.MapControllers();
        
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
        app.Run();
    }
}