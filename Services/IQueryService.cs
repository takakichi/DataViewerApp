using DataViewerApp.Models;

namespace DataViewerApp.Services;

/// <summary>
/// クエリ実行・ファイル生成サービスのインターフェース。
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// 指定クエリを実行し、行データを返す。
    /// </summary>
    /// <param name="setting">クエリ設定。</param>
    /// <returns>各行を列名→値の辞書で表したリスト。</returns>
    Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteQueryAsync(QuerySetting setting);

    /// <summary>
    /// 行データを UTF-8 BOM 付き CSV のバイト列に変換する。
    /// </summary>
    /// <param name="setting">ColumnMapping 取得に使うクエリ設定。</param>
    /// <param name="rows">ExecuteQueryAsync の戻り値。</param>
    /// <returns>CSV ファイルのバイト列。</returns>
    byte[] GenerateCsv(QuerySetting setting, IReadOnlyList<IDictionary<string, object?>> rows);

    /// <summary>
    /// 行データを Excel (.xlsx) のバイト列に変換する。
    /// </summary>
    /// <param name="setting">ColumnMapping 取得に使うクエリ設定。</param>
    /// <param name="rows">ExecuteQueryAsync の戻り値。</param>
    /// <returns>Excel ファイルのバイト列。</returns>
    byte[] GenerateExcel(QuerySetting setting, IReadOnlyList<IDictionary<string, object?>> rows);
}
