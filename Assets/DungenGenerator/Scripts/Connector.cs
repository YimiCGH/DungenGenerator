using UnityEngine;
using System.Collections;
namespace YM.Dungen {
    public class Connector : MonoBehaviour
    {
        public bool m_isOpen = true;

        public int m_doorType;

        public GameObject m_voxelOwner;

        public Connector m_shareConnetor;
        public Door m_door;

        public Room m_room;

        public bool IsCanConnect(Connector _TargetConnector) {
            int angle = Mathf.RoundToInt( Mathf.Abs(transform.eulerAngles.y - _TargetConnector.transform.eulerAngles.y));
            //float dot = Vector3.Dot(transform.forward, _TargetConnector.transform.forward);
            //Debug.LogFormat("room1 angle = {0}, room2 angle = {1}", transform.eulerAngles.y, _TargetConnector.transform.eulerAngles.y);
            return angle == 180;
        }

        public void DoLink(Connector _TargetConnector) {
            m_shareConnetor = _TargetConnector;
            m_shareConnetor.m_shareConnetor = this;

            m_isOpen = false;
            _TargetConnector.m_isOpen = false;
        }
    }
}
