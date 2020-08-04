using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LuviKunG
{
    public class ScriptingDefineSymbolsEditorWindow : EditorWindow
    {
        private static readonly GUIContent CONTENT_LIST_HEADER = new GUIContent("Scripting Define Symbols");
        private static readonly GUIContent CONTENT_BUTTON_REVERT = new GUIContent("Revert", "Revert all Scripting Define Symbols changes.");
        private static readonly GUIContent CONTENT_BUTTON_APPLY = new GUIContent("Apply", "Apply all Scripting Define Symbols changes. This will made unity recompile if your current build target group is active.");

        [MenuItem("Window/LuviKunG/Scripting Define Symbols Editor", false)]
        public static ScriptingDefineSymbolsEditorWindow OpenWindow()
        {
            var window = GetWindow<ScriptingDefineSymbolsEditorWindow>(false, "Scripting Define Symbols", true);
            window.Show();
            return window;
        }

        private BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown;
        private BuildTargetGroup inspectorBuildTargetGroup = BuildTargetGroup.Unknown;
        private List<string> sdsList;
        private ReorderableList listMain;
        private bool isDirty = false;

        private void OnEnable()
        {
            UpdateActiveBuildTargetGroup();
            UpdateScriptDefineSymbolsParameters();
        }

        private void OnGUI()
        {
            using (var horizontalScope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(CONTENT_BUTTON_REVERT, EditorStyles.toolbarButton))
                {
                    UpdateScriptDefineSymbolsParameters();
                }
                using (var disableScope = new EditorGUI.DisabledGroupScope(!isDirty))
                {
                    if (GUILayout.Button(CONTENT_BUTTON_APPLY, EditorStyles.toolbarButton))
                    {
                        ApplyChangesScriptingDefineSymbols();
                    }
                }
                GUILayout.FlexibleSpace();
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    inspectorBuildTargetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup(GUIContent.none, buildTargetGroup, EditorStyles.toolbarDropDown, GUILayout.Width(100.0f));
                    if (changeScope.changed && buildTargetGroup != inspectorBuildTargetGroup)
                    {
                        if (isDirty)
                        {
                            if (EditorUtility.DisplayDialog("Warning", "All changes on current active build target will be revert. Do you want to apply change?", "Yes", "No"))
                            {
                                ApplyChangesScriptingDefineSymbols();
                            }
                        }
                        buildTargetGroup = inspectorBuildTargetGroup;
                        UpdateScriptDefineSymbolsParameters();
                    }
                }
            }
            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                EditorGUILayout.HelpBox("Scripting Define Symbols is disabled on this build target group.", MessageType.Info, true);
            }
            else
            {
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    listMain.DoLayoutList();
                    if (changeScope.changed)
                    {
                        if (!isDirty)
                            isDirty = true;
                    }
                }
            }
        }

        private List<string> GetListScriptDefineSymbolsParameters(string sds) => new List<string>(sds.Split(';'));

        private void UpdateActiveBuildTargetGroup()
        {
            buildTargetGroup = inspectorBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        }

        private void UpdateScriptDefineSymbolsParameters()
        {
            var sds = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (string.IsNullOrWhiteSpace(sds))
                sdsList = new List<string>();
            else
                sdsList = GetListScriptDefineSymbolsParameters(sds);
            isDirty = false;
            listMain = new ReorderableList(sdsList, typeof(string), true, true, true, true);
            listMain.drawHeaderCallback = DrawHeader;
            listMain.drawElementCallback = DrawElement;
            listMain.elementHeightCallback = ElementHeight;
            listMain.onAddCallback = OnAdd;
            listMain.onRemoveCallback = OnRemove;
            listMain.onReorderCallback = OnReorder;
            //local function.
            void DrawHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, CONTENT_LIST_HEADER);
            }
            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                sdsList[index] = EditorGUI.TextField(rect, GUIContent.none, sdsList[index]);
            }
            float ElementHeight(int index)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            void OnAdd(ReorderableList list)
            {
                sdsList.Add("");
                isDirty = true;
            }
            void OnRemove(ReorderableList list)
            {
                if (list.index < 0)
                    return;
                sdsList.RemoveAt(list.index);
                isDirty = true;
            }
            void OnReorder(ReorderableList list)
            {
                isDirty = true;
            }
        }

        private void ApplyChangesScriptingDefineSymbols()
        {
            if (sdsList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sdsList[0]);
                for (int i = 1; i < sdsList.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(sdsList[i]))
                    {
                        sb.Append(';');
                        sb.Append(sdsList[i]);
                    }
                    else
                    {
                        sdsList.RemoveAt(i--);
                    }
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sb.ToString());
            }
            else
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "");
            }
            isDirty = false;
        }
    }
}