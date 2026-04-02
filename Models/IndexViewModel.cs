namespace DataViewerApp.Models;

/// <summary>
/// 画面表示用 ViewModel。
/// </summary>
public class IndexViewModel
{
    /// <summary>サイドバー表示に使うクエリ設定一覧。</summary>
    public IReadOnlyList<QuerySetting> MenuItems { get; set; } = Array.Empty<QuerySetting>();

    /// <summary>現在選択中のクエリ設定。未選択時は null。</summary>
    public QuerySetting? SelectedQuery { get; set; }

    /// <summary>表示列名一覧（ColumnMapping があれば日本語名、なければ DB 列名）。</summary>
    public IReadOnlyList<string> DisplayColumns { get; set; } = Array.Empty<string>();

    /// <summary>Dapper が返す dynamic 行データ。</summary>
    public IReadOnlyList<IDictionary<string, object?>> Rows { get; set; } =
        Array.Empty<IDictionary<string, object?>>();
}
