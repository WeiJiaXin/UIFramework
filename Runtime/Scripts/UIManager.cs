using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lowy.Bind;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Lowy.UIFramework
{
    /// <summary>
    /// UI管理
    /// </summary>
    public class UIManager
    {
        private static UIManager _ins;
        public static UIManager Ins
        {
            get
            {
                if (_ins == null) _ins = new UIManager();
                return _ins;
            }
        }

        private UIFrameworkConfig _config;

        private UIFrameworkConfig Config
        {
            get
            {
                if (_config == null)
                    _config = Resources.Load<UIFrameworkConfig>("UIFramework/Config");
                return _config;
            }
        }
        
        private Transform _UIRoot;

        private Camera _uiCamera;

        //ui
        private Dictionary<Type, UIView> _uiDic;

        //stack
        private List<AbsContent> _stack;

        public UIManager()
        {
            _uiDic = new Dictionary<Type, UIView>();
            _stack = new List<AbsContent>();
            if (_UIRoot == null)
            {
                _UIRoot = new GameObject("UIRoot").transform;
                Object.DontDestroyOnLoad(_UIRoot);
            }

            EventSystem es = Object.FindObjectOfType<EventSystem>();
            if (es == null)
                es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule))
                    .GetComponent<EventSystem>();
            es.transform.SetParent(_UIRoot);
        }
        /// <summary>
        /// 显示窗口，使用此上下文
        /// </summary>
        /// <param name="content">上下文</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static UIView Push<T>([NotNull] T content) where T : AbsContent
        {
            return Ins.Display(content);
        }
        /// <summary>
        /// 关闭一个窗口
        /// </summary>
        /// <param name="content">上下文，关闭这个上下文的窗口，如果下一个仍是不同上下问的同一个窗口，将会直接调用<see cref="UIView.ForcedDisable"/>函数，并直接调用下面的上下文的窗口的<see cref="UIView.Resume"/>操作</param>
        /// <param name="onlyTop">当<see cref="content"/>为null时，是否进关闭最上面的上下文的窗口，如果有多个上下文的同一窗口，则会保留其他的上下文之后进行<see cref="UIView.Resume"/>操作</param>
        /// <typeparam name="T">上下文类型</typeparam>
        public static void Pop<T>([CanBeNull] T content = null, bool onlyTop = true) where T : AbsContent
        {
            Ins.Disable(content, onlyTop);
        }
        /// <summary>
        /// 禁用所有窗口
        /// </summary>
        /// <param name="ignores">忽略列表</param>
        public static void PopAll(params Type[] ignores)
        {
            Ins.DisableAll(ignores);
        }
        /// <summary>
        /// 查找正在显示的视图
        /// </summary>
        /// <typeparam name="T">视图对应的上下文类型</typeparam>
        /// <returns></returns>
        public UIView FindDisplayUIView<T>() where T : AbsContent
        {
            var view = FindUIView<T>(null);
            return view.gameObject.activeSelf ? view : null;
        }
        /// <summary>
        /// 查找一个视图，如果没有就创建
        /// </summary>
        /// <param name="content">上下文，如果为空，则不会创建，并返回null</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private UIView FindUIView<T>(T content) where T : AbsContent
        {
            Type t = typeof(T);
            if (content != null)
                t = content.GetType();
            if (_uiDic.ContainsKey(t))
                return _uiDic[t];
            if (content == null)
                return null;
            var obj = Resources.Load<GameObject>(content.ViewResPath);
            //TODO 解除绑定
            Binder.GetBind(obj.GetComponent<UIView>().GetType()).UnBind();
            var res = Object.Instantiate(obj, _UIRoot).GetComponent<UIView>();
            res.name = content.UIViewName();
            res.Canvas.worldCamera = GetUICamera();
            int layer = content.ContentType <= UIContentType.UIView ? GetOutUILayer() : GetUILayer();
            foreach (var child in res.GetComponentsInChildren<Transform>())
                child.gameObject.layer = (int) Mathf.Log(layer,2);
            //
            _uiDic.Add(t, res);
            return res;
        }
        /// <summary>
        /// 重设UI的渲染顺序
        /// </summary>
        private void RefreshOrder()
        {
            int i = 10;
            foreach (var content in _stack)
            {
                FindUIView(content).Canvas.sortingOrder = i++;
            }
        }
        /// <summary>
        /// 将上下文入栈，但是会受到<see cref="AbsContent.Priority"/>的优先级限制
        /// </summary>
        /// <param name="content"></param>
        private void PushStack([NotNull] AbsContent content)
        {
            //如果有覆盖层则剔除这个层
            if (content.Priority == UIContentType.OverView.ValueToInt()&&
                (GetUICamera().cullingMask |= GetOutUILayer())==GetUICamera().cullingMask)
                GetUICamera().cullingMask ^= GetOutUILayer();
            if (_stack.Count <= 0)
            {
                _stack.Add(content);
                return;
            }

            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i].Priority <= content.Priority)
                {
                    if (i == _stack.Count - 1)
                        _stack.Add(content);
                    else
                        _stack.Insert(i + 1, content);
                    return;
                }
            }

            _stack.Insert(0, content);
        }

        private void PopStack([NotNull] AbsContent content)
        {
            _stack.Remove(content);
            //如果没有覆盖层则增加显示剔除层
            if (content.Priority == UIContentType.OverView.ValueToInt())
            {
                if (_stack.Find(c => c.Priority == UIContentType.OverView.ValueToInt()) == null&&
                    (GetUICamera().cullingMask |= GetOutUILayer())==GetUICamera().cullingMask)
                    GetUICamera().cullingMask ^= GetOutUILayer();
            }
        }

        /// <summary>
        /// 展示一个窗口
        /// </summary>
        /// <param name="content"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private UIView Display<T>([NotNull] T content) where T : AbsContent
        {
            var t = typeof(T);
            var view = FindUIView(content);
            PushStack(content);
            RefreshOrder();
            view.Display(content);
            if (_stack.Last() == content)
            {
                if (_stack.Count > 1)
                    FindUIView(_stack[_stack.Count - 2]).Pause(_stack[_stack.Count - 2]);
            }
            else
            {
                view.Pause(content);
            }

            return _uiDic[t];
        }

        /// <summary>
        /// 关闭一个窗口，上下文可为null，此时将依据<see cref="onlyTop"/>禁用
        /// </summary>
        /// <param name="content">上下文，关闭这个上下文的窗口，如果下一个仍是不同上下问的同一个窗口，将会直接调用<see cref="UIView.ForcedDisable"/>函数，并直接调用下面的上下文的窗口的<see cref="UIView.Resume"/>操作</param>
        /// <param name="onlyTop">当<see cref="content"/>为null时，是否进关闭最上面的上下文的窗口，如果有多个上下文的同一窗口，则会保留其他的上下文之后进行<see cref="UIView.Resume"/>操作</param>
        /// <typeparam name="T">上下文类型</typeparam>
        private void Disable<T>([CanBeNull] T content = null, bool onlyTop = true) where T : AbsContent
        {
            var t = typeof(T);
            var view = FindUIView(content);
            if (content != null)
            {
                DisableSelf(content);
            }
            else
            {
                var lastCon = _stack.Last();
                for (int i = _stack.Count - 1; i >= 0; i--)
                {
                    if (_stack[i].GetType() == t)
                    {
                        if (onlyTop)
                        {
                            DisableSelf(_stack[i]);
                            RefreshOrder();
                            return;
                        }

                        DisableSelf(_stack[i], false);
                    }
                }

                if (lastCon != _stack.Last() && _stack.Count > 0)
                    FindUIView(_stack.Last()).Resume(_stack.Last());
            }

            RefreshOrder();
        }
        /// <summary>
        /// 关闭一个窗口，强烈依赖上下文，不可为null
        /// </summary>
        /// <param name="content">要关闭的窗口的上下文</param>
        /// <param name="needResume"></param>
        /// <typeparam name="T"></typeparam>
        private void DisableSelf<T>([NotNull] T content, bool needResume = true) where T : AbsContent
        {
            var t = typeof(T);
            var view = FindUIView(content);
            bool top = _stack.Last() == content;
            //如果在栈顶
            if (top)
            {
                if (_stack.Count > 1)
                {
                    if (_stack[_stack.Count - 2].GetType() == t)
                        view.ForcedDisable(content);
                    else
                        view.Disable(content);
                    //
                    PopStack(content);
                    if (needResume)
                        FindUIView(_stack.Last()).Resume(_stack.Last());
                    return;
                }
            }

            view.Disable(content);
            PopStack(content);
            return;
        }

        /// <summary>
        /// 禁用所有窗口
        /// </summary>
        /// <param name="ignores">忽略部分Content</param>
        private void DisableAll(params Type[] ignores)
        {
            if (_stack.Count <= 0)
                return;
            var lastCon = _stack[_stack.Count - 1];
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (ignores != null && ignores.Contains(_stack[i].GetType()))
                    continue;
                DisableSelf(_stack[i], false);
            }

            if (_stack.Count > 0 && lastCon != _stack.Last() )
                FindUIView(_stack.Last()).Resume(_stack.Last());
            RefreshOrder();
        }
        /// <summary>
        /// 获取UICamera
        /// </summary>
        /// <returns></returns>
        public Camera GetUICamera()
        {
            if (_uiCamera == null)
            {
                var camera = GenerateUICamera();
                //
                camera.transform.SetParent(_UIRoot);
                camera.transform.SetSiblingIndex(0);
                _uiCamera = camera.GetComponent<Camera>();
            }

            return _uiCamera;
        }

        public int GetUILayer()
        {
            return Config.UILayer;
        }

        public int GetOutUILayer()
        {
            return Config.OutUILayer;
        }

        public int GetUIParticleLayer()
        {
            return Config.UIParticleLayer;
        }

        private GameObject GenerateUICamera()
        {
            var camera = new GameObject("UICamera", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.cullingMask = GetUILayer();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.depth = 1;
            //
            var particleCamera = new GameObject("ParticleCamera",typeof(Camera)).GetComponent<Camera>();
            particleCamera.clearFlags = CameraClearFlags.Depth;
            particleCamera.cullingMask = GetUIParticleLayer();
            particleCamera.orthographic = false;
            particleCamera.fieldOfView = 60;
            particleCamera.depth = 1;
            particleCamera.transform.SetParent(camera.transform);
            return camera.gameObject;
        }
    }
}