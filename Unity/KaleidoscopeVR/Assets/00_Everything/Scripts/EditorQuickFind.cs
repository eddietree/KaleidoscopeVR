using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace FunktronicLabs
{
    public class EditorQuickFind
        : EditorWindow
    {
        private string key = "";
        private List<string> items = new List<string>();
        private List<string> filtered = new List<string>();
        private int selectedIndex = 0;
        private Vector2 scrollPosition = Vector2.zero;

        private enum SearchType
        {
            Project,
            Hierarchy,
            MAX,
        }

        static private SearchType searchType = SearchType.Project;
        static private string[] searchToolbarStrings = new string[] { "Project", "Scene", };

        private bool iconsLoaded = false;
        private Texture2D iconScene;
        private Texture2D iconScript;
        private Texture2D iconTexture;
        private Texture2D iconFolder;
        private Texture2D iconGameObject;
        private Texture2D iconMaterial;
        private Texture2D iconPrefab;

        private static bool alreadyOpened = false;

        [MenuItem("Tools/Quick Find &Q")]
        public static void QuickOpen()
        {
            /*if (alreadyOpened)
            {
                alreadyOpened = false;
                GetWindow<EditorQuickFind>().Close();
                return;
            }*/

            alreadyOpened = true;

            var window = GetWindow<EditorQuickFind>(true);

            window.titleContent = new GUIContent("Quick Find");
            window.ShowUtility();
            window.Focus();

            window.items.Clear();
            window.filtered.Clear();

            // center only if first time openning
            if (!alreadyOpened)
            {
                CenterWindow(window);
            }
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void UpdateIconTextures()
        {
            if (iconsLoaded)
                return;

            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            var textureList = new List<Texture2D>(textures);

            iconScene = iconScene ?? textureList.Find((x) => x.name == "SceneAsset Icon");
            iconScript = iconScript ?? textureList.Find((x) => x.name == "cs Script Icon");
            iconTexture = iconTexture ?? textureList.Find((x) => x.name == "RenderTexture Icon");
            iconFolder = iconFolder ?? textureList.Find((x) => x.name == "Folder Icon");
            iconGameObject = iconGameObject ?? textureList.Find((x) => x.name == "GameObject Icon");
            iconMaterial = iconMaterial ?? textureList.Find((x) => x.name == "Material Icon");
            iconPrefab = iconPrefab ?? textureList.Find((x) => x.name == "Prefab Icon");

            iconsLoaded = true;
        }

        public void OnGUI()
        {
            UpdateIconTextures();

            GetWindow<EditorQuickFind>().Focus();

            var event_ = Event.current;
            var type_ = event_.type;

            if (type_ == EventType.KeyDown)
            {
                if (event_.keyCode == KeyCode.Tab)
                {
                    searchType += 1;
                    if (searchType >= SearchType.MAX)
                        searchType = SearchType.Project;

                    selectedIndex = 0;
                    scrollPosition = Vector2.zero;

                    // need to force repaint
                    GetWindow<EditorQuickFind>().Repaint();
                    return;
                }
            }

            DoGui();
        }

        private void DoGui()
        {
            var event_ = Event.current;
            var type_ = event_.type;
            var isShiftDown = (event_.modifiers & EventModifiers.Shift) != 0;
            var newSelectedIndex = selectedIndex;

            if (type_ == EventType.KeyDown)
            {
                if (event_.keyCode == KeyCode.Escape)
                {
                    GetWindow<EditorQuickFind>().Close();
                    return;
                }

                if (isShiftDown)
                {
                    if (event_.keyCode == KeyCode.Alpha1) { OpenIndex(0); return; }
                    if (event_.keyCode == KeyCode.Alpha2) { OpenIndex(1); return; }
                    if (event_.keyCode == KeyCode.Alpha3) { OpenIndex(2); return; }
                    if (event_.keyCode == KeyCode.Alpha4) { OpenIndex(3); return; }
                    if (event_.keyCode == KeyCode.Alpha5) { OpenIndex(4); return; }
                    if (event_.keyCode == KeyCode.Alpha6) { OpenIndex(5); return; }
                    if (event_.keyCode == KeyCode.Alpha7) { OpenIndex(6); return; }
                    if (event_.keyCode == KeyCode.Alpha8) { OpenIndex(7); return; }
                    if (event_.keyCode == KeyCode.Alpha9) { OpenIndex(8); return; }
                    if (event_.keyCode == KeyCode.Alpha0) { OpenIndex(9); return; }
                }

                if (event_.keyCode == KeyCode.DownArrow) { newSelectedIndex = newSelectedIndex + 1; }
                if (event_.keyCode == KeyCode.UpArrow) { newSelectedIndex = newSelectedIndex - 1; }
                if (event_.keyCode == KeyCode.Return) { OpenIndex(selectedIndex); return; }
            }

            searchType = (SearchType)GUILayout.Toolbar((int)searchType, searchToolbarStrings);

            if (event_.keyCode == KeyCode.UpArrow || event_.keyCode == KeyCode.DownArrow)
            {
                event_.Use();
            }

            GUI.SetNextControlName("QueryKey");
            key = GUILayout.TextField(key);
            GUI.FocusControl("QueryKey");

            switch (searchType)
            {
                case SearchType.Project:
                    FilterFiles(key);
                    break;

                case SearchType.Hierarchy:
                    FilterObjects(key);
                    break;
            }

            var fontStyleDefault = 11;

            var textStyle = new GUIStyle(GUI.skin.label) { richText = true, };
            textStyle.margin = new RectOffset(5, 5, 0, 0);
            textStyle.padding = new RectOffset(0, 0, 1, 1);
            textStyle.fontSize = fontStyleDefault;
            textStyle.hover.background = Texture2D.whiteTexture;

            var textHightlightStyle = new GUIStyle(GUI.skin.box) { richText = true, };
            textHightlightStyle.margin = new RectOffset(5, 5, 0, 0);
            textHightlightStyle.padding = new RectOffset(0, 0, 0, 0);
            textHightlightStyle.fontSize = fontStyleDefault;
            textHightlightStyle.hover.background = Texture2D.whiteTexture;

            var iconStyle = new GUIStyle() { stretchWidth = false, stretchHeight = false, };

            newSelectedIndex = Mathf.Clamp(newSelectedIndex, 0, filtered.Count - 1);

            // selected a new one!
            if (newSelectedIndex != selectedIndex)
            {
                selectedIndex = newSelectedIndex;

                // ping!
                if (searchType == SearchType.Hierarchy)
                {
                    if (newSelectedIndex >= 0 && newSelectedIndex < filtered.Count)
                        PingObjectInHierachy(filtered[newSelectedIndex]);
                }
            }

            GUILayout.Space(5);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, null);

            var itemCount = 0;
            foreach (var raw_item in filtered)
            {
                var displayIndex = string.Format("<b>{0}.</b>", itemCount + 1);
                var highlightText = (string)raw_item.Clone();

                // valid key
                if (!string.IsNullOrEmpty(key))
                {
                    var fragments = key.Split(' ');

                    foreach (var fragment in fragments)
                    {
                        if (string.IsNullOrEmpty(fragment))
                            continue;

                        var cursor = 0;
                        var fragmentTrimmed = fragment.Trim();

                        while (cursor < highlightText.Length)
                        {
                            var index = highlightText.IndexOf(fragmentTrimmed, cursor, System.StringComparison.InvariantCultureIgnoreCase);
                            if (index == -1)
                                break;

                            var search_str = highlightText.Substring(index, fragmentTrimmed.Length);
                            var highlight_str = string.Format("<b>{0}</b>", search_str);

                            highlightText = highlightText.Remove(index, fragmentTrimmed.Length);
                            highlightText = highlightText.Insert(index, highlight_str);

                            cursor = index + highlight_str.Length + 1;
                        }
                    }
                }

                // if selected
                var isSelected = selectedIndex == itemCount;
                if (isSelected)
                {
                    highlightText = string.Format("<color=green>{0}</color>", highlightText);
                }
                if (isShiftDown && itemCount < 9)
                {
                    displayIndex = string.Format("<color=red>{0}</color>", displayIndex);
                }

                GUILayout.BeginHorizontal(isSelected ? textHightlightStyle : textStyle);

                // number
                textStyle.fixedWidth = 30;
                textStyle.alignment = TextAnchor.MiddleRight;
                textStyle.margin = new RectOffset(0, 0, 0, 0);
                GUILayout.Label(displayIndex, textStyle);

                // icon
                var iconTexture = Texture2D.whiteTexture;
                switch (searchType)
                {
                    case SearchType.Project:
                        iconTexture = AssetDatabase.GetCachedIcon(raw_item) as Texture2D;
                        break;

                    case SearchType.Hierarchy:
                        var obj = GameObject.Find(raw_item);
                        iconTexture = PrefabUtility.GetPrefabParent(obj) != null ? iconPrefab : iconGameObject;
                        break;
                }

                iconStyle.normal.background = iconTexture;
                iconStyle.margin = new RectOffset(12, 0, 0, 0);
                GUILayout.Label("   ", iconStyle);

                // file name
                textStyle.fixedWidth = 0;
                textStyle.alignment = TextAnchor.MiddleLeft;
                textStyle.margin = new RectOffset(4, 0, 0, 0);
                //GUILayout.Label(highlightText, textStyle);
                if (GUILayout.Button(highlightText, textStyle))
                {
                    switch (searchType)
                    {
                        case SearchType.Project:
                            OpenFile(raw_item);
                            break;

                        case SearchType.Hierarchy:
                            OpenObject(raw_item);
                            break;
                    }
                }

                GUILayout.EndHorizontal();

                itemCount++;
            }

            GUILayout.EndScrollView();
        }

        private void OpenIndex(int index)
        {
            if (index >= filtered.Count)
                return;

            switch (searchType)
            {
                case SearchType.Project:

                    OpenFile(filtered[index]);
                    break;

                case SearchType.Hierarchy:
                    OpenObject(filtered[index]);
                    break;
            }
        }

        private void OpenFile(string path)
        {
            Debug.LogFormat("QuickOpen: File '{0}'", path);

            //var path = filtered[index];
            var ext = Path.GetExtension(path);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            //var type = asset.GetType();

            var is_scene = ext == ".unity";
            var is_script = ext == ".cs";

            Selection.activeObject = asset;

            // doesnt bring project window into focus
            // ping also hides name for too long if you want to rename
            //EditorGUIUtility.PingObject(asset);
            //EditorUtility.FocusProjectWindow();

            // NOTE: not version safe
            var project_browser_type = System.Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
            var project_browser_instance_field = project_browser_type.GetField("s_LastInteractedProjectBrowser");
            if (project_browser_instance_field != null)
            {
                var project_browser_instance = project_browser_instance_field.GetValue(null);
                var project_browser_window = (EditorWindow)project_browser_instance;

                if (project_browser_window)
                    project_browser_window.Focus();
            }

            if (is_scene)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            }
            else if (is_script)
            {
                AssetDatabase.OpenAsset(asset);
            }
            else // ?
            {
            }

            GetWindow<EditorQuickFind>().Close();
        }

        private void PingObjectInHierachy(string name)
        {
            var obj = GameObject.Find(name);

            if (obj != null)
            {
                Selection.objects = new Object[] { obj };
                EditorGUIUtility.PingObject(obj);
            }
        }

        private void OpenObject(string name)
        {
            var obj = GameObject.Find(name);

            if (obj != null)
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
                UnityEditor.SceneView.lastActiveSceneView.FrameSelected();
            }

            GetWindow<EditorQuickFind>().Close();
        }

        private void FilterFiles(string key)
        {
            var assets = AssetDatabase.GetAllAssetPaths();

            items.Clear();
            items.AddRange(assets);

            var fragments = key.Split(' ');
            var max = 10;
            var count = 0;

            filtered.Clear();

            foreach (var item in items)
            {
                var extension = Path.GetExtension(item);

                // no extention
                if (string.IsNullOrEmpty(extension))
                    continue;

                // asset
                if (extension == ".asset")
                    continue;

                var contains_fragment = true;

                foreach (var raw_fragment in fragments)
                {
                    var fragment = raw_fragment.Trim();

                    if (!item.ToLower().Contains(fragment.ToLower()))
                    {
                        contains_fragment = false;
                        break;
                    }
                }

                if (contains_fragment)
                {
                    filtered.Add(item);
                    count++;
                }

                if (count > max)
                    break;
            }
        }

        private void FilterObjects(string key)
        {
            var raw = GameObject.FindObjectsOfType<GameObject>();
            var fragments = key.Split(' ');
            var max = 10;
            var count = 0;

            filtered.Clear();

            foreach (var item in raw)
            {
                var contains_fragment = true;

                foreach (var raw_fragment in fragments)
                {
                    var fragment = raw_fragment.Trim();

                    if (!item.name.ToLower().Contains(fragment.ToLower()))
                    {
                        contains_fragment = false;
                        break;
                    }
                }

                if (contains_fragment)
                {
                    var fullname = item.name;
                    var parent = item.transform.parent;

                    while (parent != null)
                    {
                        fullname = string.Format("{0}/{1}", parent.name, fullname);
                        parent = parent.transform.parent;
                    }

                    filtered.Add(fullname);
                    count++;
                }

                if (count > max)
                    break;
            }
        }

        public static Rect GetEditorMainWindowPos()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsSubclassOf(typeof(ScriptableObject)))
                        continue;

                    if (type.Name != "ContainerWindow")
                        continue;

                    var showModeField = type.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    var positionProperty = type.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

                    var windows = Resources.FindObjectsOfTypeAll(type);
                    foreach (var window in windows)
                    {
                        var showMode = (int)showModeField.GetValue(window);
                        if (showMode == 4)
                        {
                            var rect = (Rect)positionProperty.GetValue(window, null);
                            return rect;
                        }
                    }
                }
            }

            return new Rect(0, 0, 0, 0);
        }

        public static void CenterWindow(EditorWindow window)
        {
            var main = GetEditorMainWindowPos();
            var pos = window.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
        }
    }
}
