using UnityEngine;
using UnityEditor;
using YM.Dungen;

[CustomEditor(typeof(Volume))]
public class VolumeEditor : Editor
{
    Volume m_volume;
    private void OnEnable()
    {
        m_volume = target as Volume;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        

        if (GUILayout.Button("生成体素"))
        {
            m_volume.GenerateVoxelGrid();
        }
        if (GUILayout.Button("计算边界"))
        {
            m_volume.CalculateBound();
        }
        if (GUILayout.Button("显示Gizmos"))
        {
            m_volume.ToggleGizmoToDraw();
        }
    }
}