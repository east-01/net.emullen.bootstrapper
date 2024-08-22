using UnityEditor;
using UnityEngine;

namespace EMullen.Bootstrapper.Editor 
{
    [CustomEditor(typeof(Bootstrapper))]
    public class BootstrapperEditor : UnityEditor.Editor 
    {

        /* Editor properties */
        private Vector2 scrollPos;

        private SerializedProperty sp_isBootstrapScene;
        private SerializedProperty sp_onlyBootstrapOnce;
        private SerializedProperty sp_sequence;
        private SerializedProperty sp_cacheIBCEveryUpdate;
        private SerializedProperty sp_blacklistedGameObjects;

        private void OnEnable() 
        {
            sp_isBootstrapScene = serializedObject.FindProperty("isBootstrapScene");
            sp_onlyBootstrapOnce = serializedObject.FindProperty("onlyBootstrapOnce");
            sp_sequence = serializedObject.FindProperty("sequence");
            sp_cacheIBCEveryUpdate = serializedObject.FindProperty("cacheIBCEveryUpdate");
            sp_blacklistedGameObjects = serializedObject.FindProperty("blacklistedGameObjects");
        }

        public override void OnInspectorGUI() 
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            sp_isBootstrapScene.boolValue = EditorGUILayout.Toggle("Is bootstrap scene", sp_isBootstrapScene.boolValue);

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(sp_sequence, new GUIContent("Bootstrap Sequence"), true, new GUILayoutOption[] {GUILayout.Width(400f)});
            CreateNote("This sequence is only used if there isn't a bootstrap sequence already running.");

            GUILayout.Space(10);

            if(sp_isBootstrapScene.boolValue)
                DrawStandardGUI();
            else
                DrawNonBootstrapperSceneGUI();

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();

        }

        private void DrawStandardGUI() 
        {
            sp_onlyBootstrapOnce.boolValue = EditorGUILayout.Toggle("Only bootstrap once", sp_onlyBootstrapOnce.boolValue);
            CreateNote("This means that this bootstrap scene will only appear once for the entire procress length. Only check if you promise all objects in here will persist for the rest of the process.");
            CreateNote("This also means that there must be proper handling for multiple bootstrap appearances of a scene if the option is unchecked.");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Cache IBootstrapComponents every update");
            sp_cacheIBCEveryUpdate.boolValue = EditorGUILayout.Toggle(sp_cacheIBCEveryUpdate.boolValue);
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.PropertyField(sp_blacklistedGameObjects, new GUILayoutOption[] { GUILayout.Width(400f) });
        }

        private void DrawNonBootstrapperSceneGUI() 
        {
            
        }

        private void CreateBigHeader(string text) 
        {
            GUILayout.Label($"<b><color=white>{text}</color></b>", BigHeaderStyle);
        }

        private void CreateHeader(string text) 
        {
            GUILayout.Label($"<b><color=white>{text}</color></b>", HeaderStyle);
        }

        private void CreateNote(string text) 
        {
            GUILayout.Label($"<i><color=#a7abb0>{text}</color></i>", NoteStyle, new GUILayoutOption[] {GUILayout.Width(400f)});
        }

        public GUIStyle BigHeaderStyle { get {
            return new() {
                richText = true,
                margin = new RectOffset(3, 10, 0, 10),
                fontSize = 15
            };
        } }

        public GUIStyle HeaderStyle { get {
            return new() {
                richText = true,
                margin = new RectOffset(3, 0, 0, 0)
            };
        } }

        public GUIStyle NoteStyle { get {
            return new() {
                richText = true,
                margin = new RectOffset(5, 0, 0, 0),
                wordWrap = true
            };
        } }
        
    }
}