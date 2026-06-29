using ACC.AccountingSubject;
using ACC.Application;
using ACC.Authority;
using ACC.ChartOfAccounts;
using ACC.Host;
using ACC.Host.Errors;
using ACC.Identity;
using ACC.Ledger;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ACC.NET API",
        Version = "v1",
        Description = "The published interface of the accounting system."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter an authentication token issued by Identity."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<InternalServerErrorOperationFilter>();
});
builder.Services.AddHostErrorHandling();
builder.Services.AddHostAuthentication(builder.Configuration);
builder.Services.AddLedger(builder.Configuration);
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddAccountingSubject(builder.Configuration);
builder.Services.AddAuthority(builder.Configuration);
builder.Services.AddChartOfAccounts(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

app.UseExceptionHandler();
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapLedger();
app.MapIdentity();
app.MapAuthority();
app.MapChartOfAccounts();
app.MapApplication();

app.Run();
