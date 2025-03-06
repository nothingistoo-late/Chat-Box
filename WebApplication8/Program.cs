var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500") // Chỉ cho phép frontend này
                  .AllowAnyMethod()  // Cho phép mọi phương thức (GET, POST, PUT, DELETE, ...)
                  .AllowAnyHeader()  // Cho phép mọi header
                  .AllowCredentials(); // Cho phép gửi cookies, auth headers
        });
});

// Đăng ký HttpClient và GeminiService
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddSingleton<GeminiService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔥 **Kích hoạt CORS (bạn đã quên bước này)**
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();
app.Run();
