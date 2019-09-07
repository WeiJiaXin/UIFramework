namespace Lowy.UIFramework
{
    /// <summary>
    /// 抽象上下文
    /// </summary>
    public abstract class AbsContent
    {
        /// <summary>
        /// 预制物资源文件路径
        /// </summary>
        protected string _resPath;

        /// <summary>
        /// 上下文类型
        /// </summary>
        public abstract UIContentType ContentType { get; }
        /// <summary>
        /// 上下文的优先级,影响在栈中的位置，默认等于<see cref="ContentType"/>
        /// <para>重写可实现动态优先级</para>
        /// </summary>
        public virtual int Priority => ContentType.ValueToInt();
        /// <summary>
        /// 预制物资源文件路径
        /// </summary>
        public virtual string ViewResPath
        {
            get
            {
                if (string.IsNullOrEmpty(_resPath))
                    _resPath = $"View/{ContentType}/{UIViewName()}";
                return _resPath;
            }
        }

        /// <summary>
        /// UIView脚本名字，脚本名和预制物名保持一致
        /// </summary>
        /// <returns></returns>
        public virtual string UIViewName()
        {
            return GetType().Name.Replace("Content", ContentType.ToString());
        }
    }
}