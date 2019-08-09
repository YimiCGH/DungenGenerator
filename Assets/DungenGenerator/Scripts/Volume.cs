using UnityEngine;
using System.Collections.Generic;

namespace YM.Dungen {
    public class Volume : MonoBehaviour
    {
        public static bool drawVolume = false;


        public Vector3Int m_roomSzie = new Vector3Int(1,1,1);

        public List<Transform> m_voxels = new List<Transform>();

        public float voxelScale = 10f;
        public Bounds m_bounds;
        [HideInInspector]
        public GameObject _voxelsContainer;

#if UNITY_EDITOR
        public void GenerateVoxelGrid()
        {
            if (m_voxels.Count != 0)
            {
                for (int i = 0; i < m_voxels.Count; i++)
                {
                    DestroyImmediate(m_voxels[i].gameObject);
                }
            }

            if (_voxelsContainer == null)
            {
                _voxelsContainer = new GameObject("Voxels");
                _voxelsContainer.transform.parent = this.transform;
            }

            m_voxels = new List<Transform>();
            for (int i = 0; i < m_roomSzie.x; i++)
            {
                for (int j = 0; j < m_roomSzie.y; j++)
                {
                    for (int k = 0; k < m_roomSzie.z; k++)
                    {
                        CreateVoxel(i * (int)voxelScale,
                                    j * (int)voxelScale,
                                    k * (int)voxelScale);
                    }
                }
            }
        }
        void CreateVoxel(int i, int j, int k)
        {
            GameObject voxel = new GameObject(string.Format("Voxel - ({0}, {1}, {2})", i, j, k));
            voxel.transform.position = new Vector3(i, j, k);
            voxel.transform.parent = _voxelsContainer.transform;
            m_voxels.Add(voxel.transform);
        }

        public void CalculateBound() {
            Transform firstChild = m_voxels[0];

            Vector3 min, max;
            min = max = firstChild.position ;

            for (int i = 0; i < m_voxels.Count; i++)
            {
                Transform voxel = m_voxels[i];
                Vector3 pos = voxel.position;

                //左下角
                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.z < min.z) min.z = pos.z;
                //右上角
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
                if (pos.z > max.z) max.z = pos.z;
            }

            Vector3 size = new Vector3(0.5f * voxelScale,
                0.5f * voxelScale,
                0.5f * voxelScale);

            m_bounds = new Bounds((min + max) * 0.5f,
                (max +size) - (min - size));

        }
        GUIStyle guisTyle = new GUIStyle();
        public void OnDrawGizmos()
        {
            if (!drawVolume)
            {
                if (m_bounds == null) return;
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
            }
            else
            {
                if (_voxelsContainer == null) return;
                Gizmos.color = new Color(0, 1, 0, 0.3f);

                UnityEditor.Handles.BeginGUI();
                guisTyle.richText = true;

                for (int i = 0; i < _voxelsContainer.transform.childCount; i++)
                {
                    Transform tsf = _voxelsContainer.transform.GetChild(i).transform;
                    Gizmos.DrawCube(tsf.position , Vector3.one * (voxelScale + 0.1f));


                    UnityEditor.Handles.Label(tsf.position,string.Format("<color=#0000FF>{0}</color>", tsf.name), guisTyle);
                }
                UnityEditor.Handles.EndGUI();
            }
        }

        public void ToggleGizmoToDraw()
        {
            Volume.drawVolume = !Volume.drawVolume;
        }

#endif

    }

}
