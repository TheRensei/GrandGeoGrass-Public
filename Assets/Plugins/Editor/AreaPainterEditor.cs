#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GrandGeoGrass
{
    [CustomEditor(typeof(GGG_AreaPainter))]
    [CanEditMultipleObjects]
    public class AreaPainterEditor : Editor
    {
        GGG_AreaPainter painter;

        SerializedProperty area;
        SerializedProperty amount;
        SerializedProperty randomSeed;
        SerializedProperty useSeed;
        SerializedProperty rayStartHeight;
        SerializedProperty grassHeightRange;
        SerializedProperty grassWidthRange;

        SerializedProperty useDensityMap;
        SerializedProperty densityMap;
        SerializedProperty tiling;
        SerializedProperty densityMultiplier;
        SerializedProperty densityThreshold;
        SerializedProperty worldSpace;

        SerializedProperty material;
        SerializedProperty meshFilter;

        bool inScene = true;
        void OnEnable()
        {
            painter = target as GGG_AreaPainter;

            area = serializedObject.FindProperty("area");
            amount = serializedObject.FindProperty("amount");

            randomSeed = serializedObject.FindProperty("randomSeed");
            useSeed = serializedObject.FindProperty("useSeed");
            rayStartHeight = serializedObject.FindProperty("rayStartHeight");
            grassHeightRange = serializedObject.FindProperty("grassHeightRange");
            grassWidthRange = serializedObject.FindProperty("grassWidthRange");

            useDensityMap = serializedObject.FindProperty("useDensityMap");
            densityMap = serializedObject.FindProperty("densityMap");
            tiling = serializedObject.FindProperty("tiling");
            densityMultiplier = serializedObject.FindProperty("densityMultiplier");
            densityThreshold = serializedObject.FindProperty("densityThreshold");
            worldSpace = serializedObject.FindProperty("worldSpace");

            material = serializedObject.FindProperty("material");
            meshFilter = serializedObject.FindProperty("meshFilter");
        }

        public override void OnInspectorGUI()
        {
            inScene = true;
            if (!painter.gameObject.scene.IsValid())
                inScene = false;

            serializedObject.Update();

            var headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            headerStyle.fontStyle = FontStyle.Bold;

            //DrawBackground();
            //Color mainColor = new Color(0, 1f, 0, 0.5f);
            Color mainColor = Color.green;


            GUI.color = mainColor;
            using (var verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.color = Color.white;
                EditorGUILayout.LabelField("Settings", headerStyle, GUILayout.ExpandWidth(true));
                float defaultLabelWidth = 0f;
                if (!EditorGUIUtility.wideMode)
                {
                    EditorGUIUtility.wideMode = true;
                    defaultLabelWidth = EditorGUIUtility.currentViewWidth - 212;

                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                }
                EditorGUILayout.PropertyField(area);

                if (useDensityMap.boolValue && painter.densityMap != null)
                {
                    GUI.enabled = false;
                }

                EditorGUILayout.IntSlider(amount, 1, 60000);
                GUI.enabled = true;

                EditorGUILayout.PropertyField(rayStartHeight);

                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(randomSeed);
                    EditorGUIUtility.labelWidth = 60f;
                    EditorGUILayout.PropertyField(useSeed);
                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                }

                painter.uniform = EditorGUILayout.Toggle("Uniform Range", painter.uniform);
                if (painter.uniform)
                {
                    EditorGUILayout.PropertyField(grassHeightRange, new GUIContent("Size Range"));
                    grassWidthRange.vector2Value = grassHeightRange.vector2Value;
                    GUI.enabled = false;
                }
                EditorGUILayout.PropertyField(grassHeightRange);
                EditorGUILayout.PropertyField(grassWidthRange);
                GUI.enabled = true;

                EditorGUILayout.PropertyField(useDensityMap);

                if (useDensityMap.boolValue)
                {
                    GUI.color = mainColor;
                    using (var vS = new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField("Density Map", headerStyle, GUILayout.ExpandWidth(true));

                        using (var horizontalScope = new GUILayout.HorizontalScope())
                        {
                            Rect pos = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
                            densityMap.objectReferenceValue = (Texture2D)EditorGUI.ObjectField(pos, densityMap.objectReferenceValue, typeof(Texture2D), false);

                            EditorGUILayout.Space();
                            GUILayout.BeginVertical();
                            //EditorGUILayout.PropertyField(tiling, GUIContent.none, GUILayout.MaxWidth(400));
                            EditorGUIUtility.labelWidth = 0.01f;
                            EditorGUILayout.PropertyField(tiling);
                            EditorGUIUtility.labelWidth = defaultLabelWidth;

                            EditorGUIUtility.labelWidth = 80f;
                            EditorGUILayout.PropertyField(worldSpace);
                            EditorGUIUtility.labelWidth = defaultLabelWidth;
                            GUILayout.EndVertical();
                        }

                        EditorGUILayout.Slider(densityMultiplier, 0f, 1f);
                        EditorGUILayout.Slider(densityThreshold, 0f, 1f);
                    }
                }

            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUI.color = mainColor;
            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.color = Color.white;
                EditorGUILayout.PropertyField(material);
                EditorGUILayout.PropertyField(meshFilter);
            }


            //If object is a prefab
            if (!inScene)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            ///------------------------------------------------------------------------------
            /// Buttons
            ///------------------------------------------------------------------------------
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUI.color = mainColor;
            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.color = Color.white;
                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Generate"))
                    {
                        painter.Generate();
                    }
                    if (GUILayout.Button("Clear"))
                    {
                        painter.Clear();
                    }
                }

                GUILayout.Space(5);

                if (GUILayout.Button("Save Mesh"))
                {
                    painter.saveFolder = EditorUtility.SaveFilePanelInProject(
                        "Save Mesh",
                        "GrassMesh.asset",
                        "asset",
                        "Please enter a file name");

                    if (!string.IsNullOrEmpty(painter.saveFolder))
                    {
                        Mesh meshToSave = Instantiate(painter.meshFilter.sharedMesh) as Mesh;
                        Mesh existingOne = AssetDatabase.LoadAssetAtPath(painter.saveFolder, typeof(Mesh)) as Mesh;

                        MeshUtility.Optimize(meshToSave);

                        if (existingOne != null)
                        {
                            existingOne.Clear();
                            EditorUtility.CopySerialized(meshToSave, existingOne);
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(meshToSave, painter.saveFolder);
                        }

                        AssetDatabase.SaveAssets();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (painter != null)
            {
                Handles.color = Color.green;
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(painter.transform.position, painter.transform.rotation, painter.transform.lossyScale);
                Handles.matrix = rotationMatrix;
                Handles.DrawWireCube(Vector3.zero, new Vector3(area.vector2Value.x, 1, area.vector2Value.y));
            }
        }

        void DrawBackground()
        {
            Rect logoPos = EditorGUILayout.GetControlRect();
            if (useDensityMap.boolValue == true)
            {
                logoPos.height = 453;
            }
            else
            {
                logoPos.height = 340;
            }

            if(!inScene)
            {
                logoPos.height -= 73;
            }
            logoPos.x -= 16;
            logoPos.width += 18;
            
            if (painter.logo == null)
            {
                painter.logo = EditorGUIUtility.Load("Assets/Plugins/GrandGeoGrass/Editor/Logo/GGG_GRADIENT.png") as Texture2D;
            }
            if (painter.logo != null)
            {
                GUI.DrawTexture(logoPos, painter.logo, ScaleMode.ScaleAndCrop, true, 0, Color.white * 0.9f, 0, 0);
            }
        }
    }
}
#endif