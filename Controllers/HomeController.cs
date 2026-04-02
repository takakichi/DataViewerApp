using DataViewerApp.Models;
using DataViewerApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataViewerApp.Controllers;

public class HomeController : Controller
{
    /// <summary>ファイル名に使用できない文字をアンダースコアに置換する。</summary>
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private readonly QuerySettings _querySettings;
    private readonly IQueryService _queryService;

    public HomeController(QuerySettings querySettings, IQueryService queryService)
    {
        _querySettings = querySettings;
        _queryService = queryService;
    }

    // -------------------------------------------------------------------------
    // GET /  または  GET /?queryId={id}
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Index(string? queryId)
    {
        var vm = new IndexViewModel
        {
            MenuItems = _querySettings.Items,
        };

        if (string.IsNullOrWhiteSpace(queryId))
            return View(vm);

        var setting = _querySettings.FindById(queryId);
        if (setting is null)
            return View(vm);   // 不明な ID は無視して空画面を返す

        var rows = await _queryService.ExecuteQueryAsync(setting);

        // 列名の解決（ColumnMapping があれば日本語名、なければ DB 列名）
        var dbColumns = rows.Count > 0
            ? rows[0].Keys.ToList()
            : setting.ColumnMapping.Keys.ToList();

        var displayColumns = dbColumns
            .Select(col => setting.ColumnMapping.TryGetValue(col, out var jp) ? jp : col)
            .ToList();

        vm.SelectedQuery = setting;
        vm.DisplayColumns = displayColumns;
        vm.Rows = rows;

        return View(vm);
    }

    // -------------------------------------------------------------------------
    // GET /Home/ExportCsv?queryId={id}
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ExportCsv(string queryId)
    {
        var setting = _querySettings.FindById(queryId);
        if (setting is null)
            return NotFound($"クエリID '{queryId}' が見つかりません。");

        var rows = await _queryService.ExecuteQueryAsync(setting);
        var bytes = _queryService.GenerateCsv(setting, rows);

        var fileName = $"{SanitizeFileName(setting.Id)}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    // -------------------------------------------------------------------------
    // GET /Home/ExportExcel?queryId={id}
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ExportExcel(string queryId)
    {
        var setting = _querySettings.FindById(queryId);
        if (setting is null)
            return NotFound($"クエリID '{queryId}' が見つかりません。");

        var rows = await _queryService.ExecuteQueryAsync(setting);
        var bytes = _queryService.GenerateExcel(setting, rows);

        var fileName = $"{SanitizeFileName(setting.Id)}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        const string contentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(bytes, contentType, fileName);
    }
}
