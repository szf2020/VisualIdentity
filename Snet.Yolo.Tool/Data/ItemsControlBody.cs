using Snet.Windows.Core.mvvm;

namespace Snet.Yolo.Tool.Data;

public class ItemsControlBody : BindNotify
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name
    {
        get => GetProperty(() => Name);
        set => SetProperty(() => Name, value);
    }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description
    {
        get => GetProperty(() => Description);
        set => SetProperty(() => Description, value);
    }

    /// <summary>
    /// 路径
    /// </summary>
    public string Path
    {
        get => GetProperty(() => Path);
        set => SetProperty(() => Path, value);
    }
    /// <summary>
    /// 选中
    /// </summary>
    public bool IsSelected
    {
        get => GetProperty(() => IsSelected);
        set => SetProperty(() => IsSelected, value);
    }
}
