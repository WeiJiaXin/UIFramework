
namespace Lowy.UIFramework
{
    /// <summary>
    /// 上下文优先级类型
    /// </summary>
    public enum UIContentType : int
    {
        //优先级最低，固定视图，后缀：UIView
        UIView,
        //优先级中等，覆盖固定视图，属于覆盖视图，后缀：OverView
        OverView,
        //优先级中上等，覆盖View视图，属于弹窗视图，后缀：DialogView
        DialogView,
        //优先级最高，以绝对可能性覆盖包括弹窗在内的视图，属于阻挡视图，后缀：BlockView
        BlockView
    }
}