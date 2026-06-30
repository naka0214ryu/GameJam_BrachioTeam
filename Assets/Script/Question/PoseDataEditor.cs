#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(IKPoseData))]
public class PoseDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pose Recorder", EditorStyles.boldLabel);

        if (GUILayout.Button("Save Current Pose (from Selection)"))
        {
            IKPoseData pose = (IKPoseData)target;
            var list = new List<IKPoseData.BoneData>();

            foreach (var root in Selection.transforms)
            {
                // root自身とその全ての子要素を処理
                foreach (var t in root.GetComponentsInChildren<Transform>())
                {
                    list.Add(new IKPoseData.BoneData
                    {
                        boneName = t.name,
                        localPosition = t.localPosition,
                        localRotation = t.localRotation
                    });
                }
            }

            pose.bones = list.ToArray();

            // 変更を保存
            EditorUtility.SetDirty(pose);
            AssetDatabase.SaveAssets();

            Debug.Log($"Pose saved: {pose.bones.Length} bones recorded.");
        }
    }
}
#endif