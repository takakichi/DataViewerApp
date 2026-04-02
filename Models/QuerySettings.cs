namespace DataViewerApp.Models;

/// <summary>
/// query_settings.json 全体をラップするコンテナ。
/// シングルトンとして DI に登録する。
/// </summary>
public class QuerySettings
{
    public IReadOnlyList<QuerySetting> Items { get; }

    public QuerySettings(IReadOnlyList<QuerySetting> items)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>指定 ID のクエリ設定を返す。見つからない場合は null。</summary>
    public QuerySetting? FindById(string id) =>
        Items.FirstOrDefault(q => q.Id == id);
}
