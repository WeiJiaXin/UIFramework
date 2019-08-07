using System;
using JetBrains.Annotations;
using Lowy.Bind;
using UnityEngine;

namespace Lowy.UIFramework
{
    /// <summary>
    /// 视图基类
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class UIView : MonoBehaviour
    {
        protected AbsContent _content;
        private Canvas _canvas;

        public Canvas Canvas => _canvas = _canvas == null ? GetComponent<Canvas>() : _canvas;

        /// <summary>
        /// 注入视图
        /// </summary>
        public UIView()
        {
            Binder.Bind(GetType()).To(this);
        }

        public virtual void Awake()
        {
            Binder.InjectObj(this);
        }

        /// <summary>
        /// 展示窗口
        /// </summary>
        /// <param name="content"></param>
        public void Display([NotNull] AbsContent content)
        {
            _content = content;
            PlayEnterAnim(content, EnterAnimEnd);
        }

        /// <summary>
        /// 播放进入的动画，可重写动画
        /// </summary>
        /// <param name="content"></param>
        /// <param name="end">动画结束的回调</param>
        protected virtual void PlayEnterAnim([NotNull] AbsContent content, Action<AbsContent> end)
        {
            gameObject.SetActive(true);
            end?.Invoke(content);
        }

        /// <summary>
        /// 进入动画结束时调用
        /// </summary>
        /// <param name="content"></param>
        public virtual void EnterAnimEnd([NotNull] AbsContent content)
        {
        }

        /// <summary>
        /// 当有视图覆盖到此视图上时调用，如果展示窗口时优先级没能在第一个，则会立即调用此函数
        /// </summary>
        /// <param name="content"></param>
        public virtual void Pause([NotNull] AbsContent content)
        {
            _content = content;
        }

        /// <summary>
        /// 覆盖在自己上面的视图被Disable后，调用
        /// </summary>
        /// <param name="content"></param>
        public virtual void Resume([NotNull] AbsContent content)
        {
            _content = content;
        }

        /// <summary>
        /// 强制禁用时调用，比如两个同视图的上下文在一起，上面的上下文Disable时会调用这个函数，而不是Disable
        /// </summary>
        /// <param name="content"></param>
        public virtual void ForcedDisable([NotNull] AbsContent content)
        {
            _content = content;
        }

        /// <summary>
        /// 禁用视图，可被强制禁用函数<see cref="ForcedDisable"/>截断，导致禁用时不执行
        /// </summary>
        /// <param name="content"></param>
        public void Disable([CanBeNull] AbsContent content = null)
        {
            _content = content;
            PlayExitAnim(content, ExitAnimEnd);
        }

        /// <summary>
        /// 播放退出动画
        /// </summary>
        /// <param name="content"></param>
        /// <param name="end"></param>
        protected virtual void PlayExitAnim([CanBeNull] AbsContent content, Action<AbsContent> end)
        {
            gameObject.SetActive(false);
            end?.Invoke(content);
        }

        /// <summary>
        /// 退出动画结束后调用
        /// </summary>
        /// <param name="content"></param>
        public virtual void ExitAnimEnd([CanBeNull] AbsContent content = null)
        {
        }
    }
}