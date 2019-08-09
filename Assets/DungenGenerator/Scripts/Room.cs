using System.Collections.Generic;
using UnityEngine;

namespace YM.Dungen {
    [RequireComponent(typeof( Volume))]
    public class Room : MonoBehaviour
    {

        public List<Connector> m_connectors;
        void Start()
        {

        }

        public bool HasOpenConnector() {
            for (int i = 0; i < m_connectors.Count; i++)
            {
                var connector = m_connectors[i];
                if (connector.m_isOpen)
                    return true;
            }
            return false;
        }
        public Connector GetFirstOpenConnector() {
            for (int i = 0; i < m_connectors.Count; i++)
            {
                var connector = m_connectors[i];
                if (connector.m_isOpen)
                    return connector;
            }
            return null;
        }
        public List<Connector> GetOpenConnectors() {
            List<Connector> connectors = new List<Connector>();
            for (int i = 0; i < m_connectors.Count; i++)
            {
                var connector = m_connectors[i];
                if (connector.m_isOpen)                
                    connectors.Add(connector);                
            }
            return connectors;
        }

#if UNITY_EDITOR
        public void BingConnector() {
            for (int i = 0; i < m_connectors.Count; i++)
            {
                m_connectors[i].m_room = this;
            }
        }
        private void OnDrawGizmos()
        {
            if (m_connectors == null || m_connectors.Count == 0)
                return;
            for (int i = 0; i < m_connectors.Count; i++)
            {
                var connector_tsf = m_connectors[i].transform;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(connector_tsf.position, 0.1f);

                Gizmos.DrawRay(new Ray(connector_tsf.position, connector_tsf.transform.right));

                Gizmos.color = Color.green;
                Gizmos.DrawRay(new Ray(connector_tsf.position, connector_tsf.transform.up));

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(new Ray(connector_tsf.position, connector_tsf.forward));

            }
        }
#endif
    }
}
