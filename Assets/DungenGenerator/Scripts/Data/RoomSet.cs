using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YM.Dungen {
    [CreateAssetMenu(menuName = "DungenGenerator/RoomSet", fileName = "New RoomSet")]
    public class RoomSet : ScriptableObject
    {

        public string m_setName = "";

        public List<Room> m_spawnRooms = new List<Room>();
        public List<Room> m_bossRoom = new List<Room>();
        public List<Room> m_roomTemplates = new List<Room>();
        public List<Door> m_doors = new List<Door>();

    }

}
