﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Lowy.UIFramework
{
    public class WindowContent
    {
        public static string[] UIView_base_Names = new[] {"UIView", "AnimUIView"};
        public string cs_name = "NewUIView";
        public UIContentType content_type = UIContentType.UIView;
        public int uiView_base = 0;
        public Vector2 screenSize = new Vector2(1080, 1920);
        public string cs_path = "Scripts/View";
        public string prefab_res_path = "Resource";
    }

    public class CreateViewWindow : EditorWindow
    {
        private WindowContent _content;
        private bool _createing;

        public CreateViewWindow()
        {
            _content = new WindowContent();
        }

        [MenuItem("Window/UIFramework/Create View %u")]
        public static void CreateView()
        {
            //创建窗口
            CreateViewWindow window = GetWindow();
                
            window.Show();
        }

        public static CreateViewWindow GetWindow()
        {
            Rect wr = new Rect(0, 0, 500, 300);
            return (CreateViewWindow) EditorWindow.GetWindowWithRect(typeof(CreateViewWindow), wr, true,
                "Create View Window");
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            if (_createing)
            {
                EditorGUILayout.LabelField("Create UIViewMono OK");
                EditorGUILayout.LabelField("Loading Script ...");
                EditorGUILayout.LabelField("Create Prefabs wait");
                return;
            }
            _content.cs_name = EditorGUILayout.DelayedTextField("CS name", _content.cs_name).Trim();
            _content.content_type = (UIContentType) EditorGUILayout.EnumPopup("Content Type", _content.content_type);
            _content.uiView_base =
                EditorGUILayout.Popup("UIView Base Class", _content.uiView_base, WindowContent.UIView_base_Names);
            _content.screenSize = EditorGUILayout.Vector2Field("Screen Size：", _content.screenSize);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Cs Path", GUILayout.MaxWidth(150)))
            {
            }

            GUILayout.Label("CS Path：" + _content.cs_path);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Resources Path", GUILayout.MaxWidth(150)))
            {
            }

            GUILayout.Label("CS Path：" + _content.prefab_res_path);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Prefabs", GUILayout.Height(40)))
            {
                SaveContent(_content);
                CreateMonoUIView(_content);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _createing = true;
            }
        }

        private void SaveContent(WindowContent content)
        {
            //
        }

        private void CreateMonoUIView(WindowContent content)
        {
            CreatePath($"{Application.dataPath}/{content.cs_path}");
            string text = File.ReadAllText($"{Application.dataPath}/UIFramework/Editor/UIView.cs.txt");
            text = text.Replace("$FILE_NAME$", content.cs_name);
            text = text.Replace("$CONTENT_TYPE$", content.content_type.ToString());
            text = text.Replace("$UI_VIEW$", WindowContent.UIView_base_Names[content.uiView_base]);
            //
            File.WriteAllText($"{Application.dataPath}/{content.cs_path}/{content.cs_name}{content.content_type}.cs",
                text);
        }

        private void CreatePrefabs(WindowContent content)
        {
            string name = $"{content.cs_name}{content.content_type}";
            var g = new GameObject(name);
            CreateCanvas(g, _content);
            RectTransform rect = new GameObject("Root", typeof(Image)).transform as RectTransform;
            rect.transform.SetParent(g.transform);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
            //
            CreatePath($"{Application.dataPath}/{content.prefab_res_path}/{content.content_type}");
            //
            PrefabUtility.SaveAsPrefabAsset(g,
                $"Assets/{content.prefab_res_path}/{content.content_type}/{name}.prefab");
            DestroyImmediate(g);
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        static void AllScriptsReloaded()
        {
            var window = GetWindow();
            if(!window._createing){
                window.Close();
                return;
            }
            window.CreatePrefabs(window._content);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            window._createing = false;
        }

        private void CreatePath(string s)
        {
            if (!Directory.Exists(s))
                Directory.CreateDirectory(s);
        }

        private void CreateCanvas(GameObject g, WindowContent conotent)
        {
            g.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            var scaler = g.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = conotent.screenSize;
            g.AddComponent<GraphicRaycaster>();
            var t = Assembly.Load("Assembly-CSharp").GetType(conotent.cs_name + conotent.content_type);
            g.AddComponent(t);
        }
    }
}