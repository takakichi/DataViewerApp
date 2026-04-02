namespace DataViewerApp.Models;

/// <summary>
/// query_settings.json の1エントリに対応するモデル。
/// </summary>
public class QuerySetting
{
    /// <summary>クエリを一意に識別するID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>サイドバーに表示するメニュー名。</summary>
    public string MenuName { get; set; } = string.Empty;

    /// <summary>実行する SELECT SQL。</summary>
    public string Sql { get; set; } = string.Empty;

    /// <summary>DB列名 → 日本語表示名の対応辞書。</summary>
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
}
