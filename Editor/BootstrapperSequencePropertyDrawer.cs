using System;
using UnityEditor;
using UnityEngine;

namespace EMullen.Bootstrapper.Editor 
{
    [CustomPropertyDrawer(typeof(BootstrapSequence))]
    public class BootstrapSequencePropertyDrawer : PropertyDrawer 
    {
        private int selectedTab = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (property.isExpanded ? 5 + GetListSize(property) : 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Display foldout
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);

            if (property.isExpanded)
            {
                // Tab buttons
                Rect tabRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                selectedTab = GUI.Toolbar(tabRect, selectedTab, new string[] { "Bootstrap scenes", "Target scenes" });

                // Draw selected list
                SerializedProperty sp_buildIndexList = selectedTab == 0 ? property.FindPropertyRelative("bootstrapScenes") : property.FindPropertyRelative("targetScenes");
                SerializedProperty sp_activeSceneProperty = property.FindPropertyRelative("targetSceneToSetActive");
                DrawList(position, sp_activeSceneProperty, sp_buildIndexList); 

                Rect labelRect = new Rect(position.x, position.y + (3 + GetListSize(property)) * EditorGUIUtility.singleLineHeight + 5f, 300f, EditorGUIUtility.singleLineHeight);
                GUI.Label(labelRect, "Use current active scenes as load targets");

                Rect toggleRect = new Rect(position.x + position.width - EditorGUIUtility.singleLineHeight, labelRect.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                SerializedProperty overrideTargetScenesWithOpenScenes = property.FindPropertyRelative("overrideTargetScenesWithOpenScenes");
                overrideTargetScenesWithOpenScenes.boolValue = GUI.Toggle(toggleRect, overrideTargetScenesWithOpenScenes.boolValue, "Use current active scenes as load targets");
            }
        }

        private void DrawList(Rect position, SerializedProperty sp_activeSceneProperty, SerializedProperty sp_buildIndexList)
        {
            Rect listRect = new Rect(position.x, position.y + 5f + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

            if (sp_buildIndexList.arraySize == 0) {
                EditorGUI.LabelField(listRect, "List is empty");
            } else {
                float c1Width = 100f;
                float c2Width = 200f;
                float c3Width = 100f;
                (Rect, Rect, Rect) GetColumnRects() {
                    Rect c1 = new Rect(listRect.x, listRect.y, c1Width, EditorGUIUtility.singleLineHeight);
                    Rect c2 = new Rect(listRect.x+5f+c1Width, listRect.y, c2Width, EditorGUIUtility.singleLineHeight);
                    Rect c3 = new Rect(listRect.x+5f+c1Width+5f+c2Width, listRect.y, c3Width, EditorGUIUtility.singleLineHeight);
                    return (c1, c2, c3);
                }

                listRect.y += EditorGUIUtility.singleLineHeight;

                (Rect, Rect, Rect) initColumns = GetColumnRects();
                GUI.Label(initColumns.Item1, "Build idx");
                GUI.Label(initColumns.Item2, "Name");
                if(selectedTab == 1) 
                    GUI.Label(initColumns.Item3, "Active scene");

                listRect.y += 5f;

                for (int i = 0; i < sp_buildIndexList.arraySize; i++) {
                    listRect.y += EditorGUIUtility.singleLineHeight;
                    
                    (Rect, Rect, Rect) columns = GetColumnRects();
                    SerializedProperty sp_sceneBuildIndex = sp_buildIndexList.GetArrayElementAtIndex(i);
                    sp_sceneBuildIndex.intValue = EditorGUI.IntField(columns.Item1, sp_sceneBuildIndex.intValue);

                    string sceneName = String.Join("/", BootstrapSequenceManager.BuildIndexToName(sp_sceneBuildIndex.intValue)
                    .Replace(".unity", "").Split("/")[^2..]);



                    GUI.Label(columns.Item2, $"{sceneName}");

                    if(selectedTab == 1) {
                        if(GUI.Toggle(columns.Item3, sp_sceneBuildIndex.intValue == sp_activeSceneProperty.intValue, ""))
                            sp_activeSceneProperty.intValue = sp_sceneBuildIndex.intValue;
                    }

                    if(i < sp_buildIndexList.arraySize-1)
                        listRect.y += 3f;
                }
            }

            // Add and Remove buttons
            listRect.y += EditorGUIUtility.singleLineHeight + 5f;
            listRect.x += position.width - 50;
            listRect.width = 25;

            if (GUI.Button(listRect, "-"))
            {
                if (sp_buildIndexList.arraySize > 0)
                    sp_buildIndexList.DeleteArrayElementAtIndex(sp_buildIndexList.arraySize - 1);
            }

            listRect.x += 30;

            if (GUI.Button(listRect, "+"))
            {
                sp_buildIndexList.InsertArrayElementAtIndex(sp_buildIndexList.arraySize);
            }
        }

        private int GetListSize(SerializedProperty property)
        {
            SerializedProperty listProperty = selectedTab == 0 ? property.FindPropertyRelative("bootstrapScenes") : property.FindPropertyRelative("targetScenes");
            return listProperty.arraySize + 2; // Extra 2 lines for buttons
        }
    }
}