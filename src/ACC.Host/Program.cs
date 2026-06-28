using ACC.AccountingSubject;
using ACC.Application;
using ACC.Authority;
using ACC.ChartOfAccounts;
using ACC.Identity;
using ACC.Ledger;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AccountingX API",
        Version = "v1",
        Description = "The published interface of the accounting system."
    });
});
builder.Services.AddLedger(builder.Configuration);
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddAccountingSubject(builder.Configuration);
builder.Services.AddAuthority(builder.Configuration);
builder.Services.AddChartOfAccounts(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.MapLedger();
app.MapIdentity();
app.MapAuthority();
app.MapChartOfAccounts();
app.MapApplication();

app.Run();
