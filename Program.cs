using System.Text.Json;
using DataViewerApp.Models;
using DataViewerApp.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// サービス登録
// -------------------------------------------------------------------------

builder.Services.AddControllersWithViews();

// --- query_settings.json をシングルトンとして登録 ---
var settingsPath = Path.Combine(AppContext.BaseDirectory, "query_settings.json");
if (!File.Exists(settingsPath))
    throw new FileNotFoundException(
        $"クエリ設定ファイルが見つかりません: {settingsPath}");

List<QuerySetting> items;
try
{
    var json = await File.ReadAllTextAsync(settingsPath);
    items = JsonSerializer.Deserialize<List<QuerySetting>>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    }) ?? [];
}
catch (JsonException ex)
{
    throw new InvalidOperationException(
        $"query_settings.json の解析に失敗しました: {ex.Message}", ex);
}

builder.Services.AddSingleton(new QuerySettings(items));

// --- QueryService をスコープサービスとして登録 ---
builder.Services.AddScoped<IQueryService, QueryService>();

// -------------------------------------------------------------------------
// ミドルウェアパイプライン
// -------------------------------------------------------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
