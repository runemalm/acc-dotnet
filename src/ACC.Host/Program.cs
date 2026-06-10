using ACC.Identity;
using ACC.Ledger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddLedger(builder.Configuration);
builder.Services.AddIdentity(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.MapLedger();
app.MapIdentity();

app.Run();
