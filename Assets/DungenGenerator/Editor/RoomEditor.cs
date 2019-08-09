using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YM.Dungen;

[CustomEditor(typeof(Room))]
public class RoomEditor :Editor
{
    Room m_room;
    private void OnEnable()
    {
        m_room = target as Room;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("绑定接口与房间")) {
            m_room.BingConnector();
        }
    }
}
