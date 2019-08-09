using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YM.Dungen {
    public class DungenGenerator : MonoBehaviour
    {
        public DungenGeneratorData m_generatorData;
        public int m_activeSetIndex;//使用哪一套来生成
        public int m_targetRoomAmount = 10;
        public float m_voxelSize = 10;//体素的尺寸

        public int m_seed;
        public bool m_useCustomSeed;

        public bool m_debugRoomGenProcess;

        RandomInt m_random;
        bool m_isFinishGen = false;
        List<Room> m_rooms;
        List<Door> m_doors;

        List<Connector> m_openSet;
        HashSet<Vector3> m_globalVoxels;
        HashSet<Vector3> m_connectorsPos;

        Room m_startRoom;

        void Start()
        {
            StartCoroutine( GenDungen());           
        }        

        IEnumerator GenDungen() {
            m_isFinishGen = false;

            m_rooms = new List<Room>();
            m_doors = new List<Door>();
            m_openSet = new List<Connector>();
            m_globalVoxels = new HashSet<Vector3>();
            m_connectorsPos = new HashSet<Vector3>();

            if (m_random == null)
                m_random = new RandomInt();
            if (!m_useCustomSeed) {
                m_seed = Random.Range(0, int.MaxValue);
            }
            Debug.Log("seed = "+m_seed);
            m_random.Init(m_seed);

            int spawnRoomID = m_random.GetValue(0, m_generatorData.GetSpawnRoomCount(m_activeSetIndex));
            m_startRoom = Instantiate(m_generatorData.GetSpawnRoom(m_activeSetIndex, spawnRoomID));
            m_startRoom.transform.SetParent(this.transform);
            m_openSet.Add(m_startRoom.m_connectors[0]);

            var volume = m_startRoom.GetComponent<Volume>();
            volume.CalculateBound();
            AddGlobalVoxels(volume);
            AddConnectorsPos(m_startRoom);

            //创建房间
            while (m_openSet.Count > 0) {
                Debug.LogFormat("生成第{0}个地宫" , m_rooms.Count);
                yield return GenerateNextRoom();
                if (m_debugRoomGenProcess) {
                    yield return new WaitForSeconds(1f);
                }                
            }
            //创建门
            /*
            for (int i = 0; i < m_rooms.Count; i++)
            {
                var room = m_rooms[i];
                for (int j = 0; j < room.m_connectors.Count; j++)
                {
                    var connector = room.m_connectors[j];

                    if (connector.m_isOpen) {
                        if (connector.m_doorType != -1) {// -1 为不需要门
                            Door d = Instantiate(m_generatorData.GetDoor(m_activeSetIndex, connector.m_doorType));
                            m_doors.Add(d);

                            connector.m_door = d;
                            connector.m_shareConnetor.m_door = d;

                            d.gameObject.transform.position = connector.transform.position;
                            d.gameObject.transform.rotation = connector.transform.rotation;

                            d.gameObject.transform.SetParent(this.transform);
                        }
                        //关闭连接器
                        connector.m_isOpen = false;
                        connector.m_shareConnetor.m_isOpen = false;
                    }
                }
            }
            */
            m_isFinishGen = true;
            Debug.Log("完成地宫的生成，房间数："+m_rooms.Count);
        }
        bool IsLastRoom {
            get { return m_rooms.Count >= m_targetRoomAmount - 1; }
        }
        IEnumerator GenerateNextRoom() {
            Connector curOpenConnector = m_openSet[0];

            List<Room> possibleRooms = new List<Room>();
            possibleRooms.AddRange(m_generatorData.GetRoomTemplates(m_activeSetIndex));
            //Debug.Log("房间模板数："+possibleRooms.Count);
            possibleRooms.Shuffle(m_random);//打乱模板顺序

            Room newRoom;
            Connector newRoomConnector;
            bool roomIsGood = false;
            Dictionary<Connector, Connector> connectPairs = null;
            do {
                if (IsLastRoom){
                    //Debug.Log("获取只有一个接口的房间");
                    possibleRooms = GetAllRoomWhithOneDoor(possibleRooms);
                }
                int doorsNum = 0;
                int r = m_random.GetValue(0, possibleRooms.Count);
                                
                Room roomToTry = possibleRooms[r];              
                doorsNum = roomToTry.m_connectors.Count;
                if (doorsNum == 1)
                {
                    if (m_rooms.Count < m_targetRoomAmount) {
                        //Try again
                        int chance = m_targetRoomAmount - m_rooms.Count;
                        while (doorsNum == 1 && chance > 0) {
                            r = m_random.GetValue(0, possibleRooms.Count);
                            roomToTry = possibleRooms[r];
                            doorsNum = roomToTry.m_connectors.Count;
                            chance--;
                        }
                    }
                }

                possibleRooms.RemoveAt(r);//移除尝试过的模板
                newRoom = Instantiate(roomToTry, this.transform);
                if (doorsNum == 1){
                    //可以尝试把房间旋转到接口相连
                    newRoomConnector = ConnectOneConnectorRoom(curOpenConnector, newRoom);
                    Debug.Log("尝试旋转房间后拼接");
                    yield return new WaitForSeconds(1f);
                }
                else {
                    newRoomConnector = ConnectRoom(curOpenConnector, newRoom);
                }

                if (newRoomConnector != null ){
                    //检测是否重叠                
                    bool isOverlap = OverlapCheck(newRoomConnector,out connectPairs);
                    if (!isOverlap){
                        roomIsGood = true;
                    }else
                        connectPairs = null;
                }
                yield return null;
                if (!roomIsGood) {
                    GameObject.DestroyImmediate(newRoom.gameObject);
                }

                Debug.Log("剩余模板数："+possibleRooms.Count);
                
            } while (possibleRooms.Count > 0 && !roomIsGood);

            //结束循环后
            if (!roomIsGood)
            {
                Debug.LogError("失败，找不到合适的房间");
            }
            else {
                //Debug.Log("开始 绑定两个房间的接口");
                TryCloseOtherConnector(connectPairs);//把新房间的接口和其他开放房间的接口继续测试，看起是否可以连接

                //成功添加新房间
                m_rooms.Add(newRoom);
                //添加新房间的体素
                AddGlobalVoxels(newRoom.GetComponent<Volume>());
                //添加接口所在体素位置
                AddConnectorsPos(newRoom);

                if (newRoom.HasOpenConnector()) {
                    var newOpenConnectors = newRoom.GetOpenConnectors();
                    Debug.Log("新增接口："+newOpenConnectors.Count);
                    m_openSet.AddRange(newOpenConnectors);
                }
            }
        }
        void AddConnectorsPos(Room _room) {
            for (int i = 0; i < _room.m_connectors.Count; i++)
                m_connectorsPos.Add(_room.m_connectors[i].m_voxelOwner.transform.position.RoundVector3ToInt());            
        }
        void AddGlobalVoxels(Volume _volume) {
            var voxels = _volume.m_voxels;
            for (int i = 0; i < voxels.Count; i++) {
                var voxel = voxels[i];
                Vector3 pos = voxel.position.RoundVector3ToInt();

                if (!m_globalVoxels.Contains(pos)) {
                    m_globalVoxels.Add(pos);
                }
            }
        }

        #region Util
        //获取只有一个门口的房间
        public List<Room> GetAllRoomWhithOneDoor(List<Room> _list) {
            List<Room> roomsWithOneDoor = new List<Room>();
            for (int i = 0; i < _list.Count; i++) {
                var room = _list[i];
                if (room.m_connectors.Count == 1)
                    roomsWithOneDoor.Add(room);
            }
            return roomsWithOneDoor;
        }       
        Connector ConnectOneConnectorRoom(Connector _curConnector, Room _newRoom)
        {
            Connector newRoomConnector = _newRoom.GetFirstOpenConnector();
            _newRoom.transform.rotation = Quaternion.AngleAxis(_curConnector.transform.eulerAngles.y - newRoomConnector.transform.eulerAngles.y + 180f, Vector3.up);
            Vector3 translate = _curConnector.transform.position - newRoomConnector.transform.position;
            _newRoom.transform.position += translate;

            var volume = _newRoom.GetComponent<Volume>();
            volume.CalculateBound();//重新计算包围体
            return newRoomConnector;
        }
        Connector ConnectRoom(Connector _curConnector, Room _newRoom) {
            //新房间的接口
            List<Connector> newConnectors = _newRoom.GetOpenConnectors();

            bool canConnect = false;
            Connector newRoomConnector = null;

            newConnectors.Shuffle(m_random);//打乱顺序

            for (int i = 0; i < newConnectors.Count; i++){
                newRoomConnector = newConnectors[i];
                if (_curConnector.IsCanConnect(newRoomConnector)){
                    canConnect = true;
                    break;
                }
            }

            if (!canConnect){
                Debug.LogWarning("两个房间没有可以连接的接口");
                return null;
            }

            //对齐房间            
            //_newRoom.transform.rotation = Quaternion.AngleAxis(_curConnector.transform.eulerAngles.y - _newRoom.transform.eulerAngles.y + 180f, Vector3.up);
            Vector3 translate = _curConnector.transform.position - newRoomConnector.transform.position;

            _newRoom.transform.position += translate;
            var volume = _newRoom.GetComponent<Volume>();
            volume.CalculateBound();//重新计算包围体       
            return newRoomConnector;
        }
        bool OverlapCheck(Connector _newRoomConnector,out Dictionary<Connector, Connector> _ConnectPairs)
        {
            Room newRoom = _newRoomConnector.m_room;
            Volume volume = newRoom.GetComponent<Volume>();
            HashSet<Vector3> newRoomVoxelsPos = new HashSet<Vector3>();
            //房间体素重合
            for (int i = 0; i < volume.m_voxels.Count; i++){
                var voxel = volume.m_voxels[i];
                Vector3 pos = voxel.position.RoundVector3ToInt();
                if (m_globalVoxels.Contains(pos)) {
                    _ConnectPairs = null;
                    return true;
                }
                newRoomVoxelsPos.Add(pos);
            }
            //将门口，或连接点所在的体素 的位置沿门口方向 移动一个单元个位置
            //如果和其他房间的体素有重合，说明该门口被堵住了

            //获取是否有接口和新房间的接口相连
            _ConnectPairs = new Dictionary<Connector, Connector>();
            HashSet<Vector3> newRoomConnectorPos = new HashSet<Vector3>();

            List<Connector> openConnectors = new List<Connector>();
            openConnectors.AddRange(m_openSet);

            for (int i = 0; i < newRoom.m_connectors.Count; i++){
                var newConnector = newRoom.m_connectors[i];
                Vector3 newConnectorPos = newConnector.m_voxelOwner.transform.position.RoundVector3ToInt();
                newRoomConnectorPos.Add(newConnectorPos);

                for (int j = 0; j < openConnectors.Count; j++){
                    var openConnector = openConnectors[j];
                    if (openConnector.IsCanConnect(newConnector)) {
                        if (GetConnectorTargetVoxelPos(openConnector, volume.voxelScale) == newConnectorPos) {
                            _ConnectPairs.Add(newConnector, openConnector);
                            openConnectors.RemoveAt(j);
                            break;
                        }
                    }
                }
                //1. 检查是否被堵住
                Vector3 newRoomConnectorLinkPos = GetConnectorTargetVoxelPos(newConnector, volume.voxelScale);

                if (m_globalVoxels.Contains(newRoomConnectorLinkPos)) {
                    //如果该位置不是接口所在位置，则说明堵住
                    if (!m_connectorsPos.Contains( newRoomConnectorLinkPos)) {
                        return true;
                    }
                }
            }

            //2. 检查是否堵住其他开放房间的接口
            for (int i = 1; i < m_openSet.Count; i++){
                var openConnector = m_openSet[i];
                
                Vector3 connectorLinkPos = GetConnectorTargetVoxelPos(openConnector, volume.voxelScale);
                if (newRoomVoxelsPos.Contains(connectorLinkPos)) {
                    //如果该位置不是接口所在位置，则说明堵住
                    if (!newRoomConnectorPos.Contains(connectorLinkPos)) {
                        return true;
                    }
                }               
            }
            return false;
        }      

        void TryCloseOtherConnector(Dictionary<Connector, Connector> _ConnectPairs) {
            if (_ConnectPairs.Count == 0)            
                return;
            
            foreach (var item in _ConnectPairs){
                var newConnector = item.Key;
                var openConnector = item.Value;

                newConnector.DoLink(openConnector);
                m_openSet.Remove(openConnector);

                //创建门
                if (openConnector.m_doorType != -1)
                {// -1 为不需要门
                    Door d = Instantiate(m_generatorData.GetDoor(m_activeSetIndex, openConnector.m_doorType));
                    m_doors.Add(d);


                    d.gameObject.transform.position = openConnector.transform.position;
                    d.gameObject.transform.rotation = openConnector.transform.rotation;

                    d.gameObject.transform.SetParent(this.transform);
                }
            }
        }

        float NormalizeAngle(int _rotation)
        {            
            if (_rotation < 0){
                while (_rotation < 0)                
                    _rotation += 360;                
            }
            else {
                while (_rotation > 360)
                    _rotation -= 360;                
            }
            return _rotation;
        }
        //获取接口所连接的体素的位置
        Vector3 GetConnectorTargetVoxelPos(Connector _connector,float _voxelScale) {
            float rotate = NormalizeAngle(Mathf.RoundToInt(_connector.transform.rotation.eulerAngles.y));
            Vector3 dir = new Vector3();
            if (rotate == 0)
                dir = Vector3.forward;//门的朝向为正前
            else if (rotate == 180)
                dir = Vector3.back;//门的朝向为正后
            else if (rotate == 90)
                dir = Vector3.right;//门的朝向为正右
            else if (rotate == 270)
                dir = Vector3.left;//门的朝向为正左

            if (_connector.m_voxelOwner == null)
                Debug.LogError(_connector.transform.parent.gameObject.name+" 连接所在体素为空");
            return _connector.m_voxelOwner.transform.position + (dir * _voxelScale);
        }
        #endregion
    }
}
