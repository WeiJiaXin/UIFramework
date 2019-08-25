using System;
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

        [MenuItem("Window/UIFramework/Create View %u")]
        public static void CreateView()
        {
            //创建窗口
            CreateViewWindow window = GetWindow();

            window.Show();
        }

        public static CreateViewWindow GetWindow()
        {
            Rect wr = new Rect(0, 0, 500, 350);
            return (CreateViewWindow) EditorWindow.GetWindowWithRect(typeof(CreateViewWindow), wr, true,
                "Create View Window");
        }

        private void OnEnable()
        {
            _content = GetContent();
        }

        private void OnDisable()
        {
            SaveContent(_content);
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

            _content.cs_name = EditorGUILayout.TextField("CS name", _content.cs_name).Trim();
            _content.content_type = (UIContentType) EditorGUILayout.EnumPopup("Content Type", _content.content_type);
            _content.uiView_base =
                EditorGUILayout.Popup("UIView Base Class", _content.uiView_base, WindowContent.UIView_base_Names);
            _content.screenSize = EditorGUILayout.Vector2Field("Screen Size：", _content.screenSize);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Cs Path", GUILayout.MaxWidth(150)))
            {
                string path = SelectPath();
                if (!string.IsNullOrEmpty(path) && path.Contains(Application.dataPath+ "/"))
                {
                    _content.cs_path = path.Replace(Application.dataPath + "/", "");
                }
                else
                {
                    EditorUtility.DisplayDialog("UI Framework", $"'{path}' Path invalid, path need has '{Application.dataPath}/'", "ok");
                }
            }

            GUILayout.Label("CS Path：" + _content.cs_path);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Resources Path", GUILayout.MaxWidth(150)))
            {
                string path = SelectPath();
                if (!string.IsNullOrEmpty(path) && path.Contains(Application.dataPath+ "/"))
                {
                    _content.prefab_res_path = path.Replace(Application.dataPath + "/", "");
                }
                else
                {
                    EditorUtility.DisplayDialog("UI Framework", $"'{path}' Path invalid, path need has '{Application.dataPath}/'", "ok");
                }
            }

            GUILayout.Label("CS Path：" + _content.prefab_res_path);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Prefabs", GUILayout.Height(40)))
            {
                EditorUtility.DisplayProgressBar("UI Framework", "Loading Script ...", 0.4f);
                SaveContent(_content);
                if (CanCreate(_content))
                {
                    CreateMonoUIView(_content);
                    _createing = true;
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "文件已存在", "ok");
                    _createing = false;
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.LabelField("如果修改视图类型：");
            EditorGUILayout.LabelField("1.代码'重命名'脚本名，后缀为所需的上下文类型");
            EditorGUILayout.LabelField("2.下面的上下文中修改类型为对应的枚举");
            EditorGUILayout.LabelField("3.重命名文件，保持与脚本对应");
            EditorGUILayout.LabelField("4.重命名预制物，与脚本对应");
            EditorGUILayout.LabelField("5.移动预制物到相应的文件夹中");
        }

        private float i = 0;
        private bool CanCreate(WindowContent content)
        {
            if (File.Exists($"{Application.dataPath}/{content.cs_path}/{content.cs_name}{content.content_type}.cs"))
                return false;
            if (File.Exists(
                $"{Application.dataPath}/{content.prefab_res_path}/{content.content_type}/{content.cs_name}{content.content_type}.prefab")
            )
                return false;
            return true;
        }

        private string SelectPath()
        {
            return EditorUtility.SaveFolderPanel("Select cs save path", Application.dataPath, "Assets");
        }

        private WindowContent GetContent()
        {
            if (PlayerPrefs.HasKey("UI_FRAMEWORK_WINDOW_CONTENT_KEY"))
                return JsonUtility.FromJson<WindowContent>(PlayerPrefs.GetString("UI_FRAMEWORK_WINDOW_CONTENT_KEY"));
            return new WindowContent();
        }

        private void SaveContent(WindowContent content)
        {
            PlayerPrefs.SetString("UI_FRAMEWORK_WINDOW_CONTENT_KEY", JsonUtility.ToJson(content));
        }

        private void CreateMonoUIView(WindowContent content)
        {
            string path = $"{Application.dataPath}/{content.cs_path}";
            CreatePath(path);
            string oldText = "";
            if (File.Exists($"{path}/{content.cs_name}{content.content_type}.cs"))
                oldText = File.ReadAllText($"{path}/{content.cs_name}{content.content_type}.cs");
            string text = File.ReadAllText($"{Application.dataPath}/UIFramework/Editor/UIView.cs.txt");
            text = text.Replace("$FILE_NAME$", content.cs_name);
            text = text.Replace("$CONTENT_TYPE$", content.content_type.ToString());
            text = text.Replace("$UI_VIEW$", WindowContent.UIView_base_Names[content.uiView_base]);
            //
            if (text == oldText)
            {
                _createing = false;
                return;
            }

            //
            File.WriteAllText($"{path}/{content.cs_name}{content.content_type}.cs",
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
            rect.gameObject.layer = LayerMask.NameToLayer("UI");
            //
            CreatePath($"{Application.dataPath}/{content.prefab_res_path}/{content.content_type}");
            //
            PrefabUtility.SaveAsPrefabAsset(g,
                $"Assets/{content.prefab_res_path}/{content.content_type}/{name}.prefab");
            DestroyImmediate(g);
            EditorUtility.ClearProgressBar();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void AllScriptsReloaded()
        {
            var window = GetWindow();
            if (!window._createing)
            {
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
            g.layer = LayerMask.NameToLayer("UI");
        }
    }
}