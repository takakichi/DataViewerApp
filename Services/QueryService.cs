using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using DataViewerApp.Models;
using Microsoft.Data.SqlClient;

namespace DataViewerApp.Services;

/// <summary>
/// Dapper によるクエリ実行と CsvHelper / ClosedXML によるファイル生成の実装。
/// </summary>
public class QueryService : IQueryService
{
    private readonly string _connectionString;

    public QueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
    }

    // -------------------------------------------------------------------------
    // IQueryService 実装
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteQueryAsync(QuerySetting setting)
    {
        // SQL は管理者が query_settings.json で定義した固定クエリであり、エンドユーザー入力は含まれない。
        // 運用時はファイルへのアクセス権を適切に制限すること。
        await using var connection = new SqlConnection(_connectionString);
        var dynamics = await connection.QueryAsync(setting.Sql);

        // IDapperRow は IDictionary<string, object> を実装しているため直接キャスト可能。
        // 安全のため明示的にディクショナリへ変換する。
        var result = dynamics
            .Cast<IDictionary<string, object?>>()
            .ToList()
            .AsReadOnly();

        return result;
    }

    /// <inheritdoc />
    public byte[] GenerateCsv(QuerySetting setting, IReadOnlyList<IDictionary<string, object?>> rows)
    {
        if (rows.Count == 0)
            return GenerateEmptyCsv(setting);

        var dbColumns = rows[0].Keys.ToList();
        var displayHeaders = dbColumns
            .Select(col => setting.ColumnMapping.TryGetValue(col, out var jp) ? jp : col)
            .ToList();

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);

        using var ms = new MemoryStream();
        // UTF-8 BOM 付き
        using var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        using var csv = new CsvWriter(writer, csvConfig);

        // ヘッダー行書き込み
        foreach (var header in displayHeaders)
            csv.WriteField(header);
        csv.NextRecord();

        // データ行書き込み
        foreach (var row in rows)
        {
            foreach (var col in dbColumns)
                csv.WriteField(row.TryGetValue(col, out var val) ? val : null);
            csv.NextRecord();
        }

        writer.Flush();
        return ms.ToArray();
    }

    /// <inheritdoc />
    public byte[] GenerateExcel(QuerySetting setting, IReadOnlyList<IDictionary<string, object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Data");

        if (rows.Count == 0)
        {
            WriteEmptyExcelHeader(sheet, setting);
            using var emptyMs = new MemoryStream();
            workbook.SaveAs(emptyMs);
            return emptyMs.ToArray();
        }

        var dbColumns = rows[0].Keys.ToList();

        // ヘッダー行
        for (var i = 0; i < dbColumns.Count; i++)
        {
            var col = dbColumns[i];
            var header = setting.ColumnMapping.TryGetValue(col, out var jp) ? jp : col;
            var cell = sheet.Cell(1, i + 1);
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // データ行
        for (var rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var row = rows[rowIdx];
            for (var colIdx = 0; colIdx < dbColumns.Count; colIdx++)
            {
                var col = dbColumns[colIdx];
                var val = row.TryGetValue(col, out var v) ? v : null;
                SetCellValue(sheet.Cell(rowIdx + 2, colIdx + 1), val);
            }
        }

        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // -------------------------------------------------------------------------
    // ヘルパー
    // -------------------------------------------------------------------------

    private static byte[] GenerateEmptyCsv(QuerySetting setting)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        foreach (var header in setting.ColumnMapping.Values)
            csv.WriteField(header);
        csv.NextRecord();
        writer.Flush();
        return ms.ToArray();
    }

    private static void WriteEmptyExcelHeader(IXLWorksheet sheet, QuerySetting setting)
    {
        var col = 1;
        foreach (var header in setting.ColumnMapping.Values)
        {
            var cell = sheet.Cell(1, col++);
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }
    }

    /// <summary>型に応じて適切な ClosedXML セット方法を選択する。</summary>
    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                break;
            case bool b:
                cell.Value = b;
                break;
            case DateTime dt:
                cell.Value = dt;
                cell.Style.DateFormat.Format = "yyyy/MM/dd HH:mm:ss";
                break;
            case DateTimeOffset dto:
                cell.Value = dto.DateTime;
                cell.Style.DateFormat.Format = "yyyy/MM/dd HH:mm:ss";
                break;
            case int or long or short or byte or sbyte or uint or ulong or ushort:
                cell.Value = Convert.ToInt64(value);
                break;
            case float or double or decimal:
                cell.Value = Convert.ToDouble(value);
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }
}
