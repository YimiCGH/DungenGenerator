using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YM.Dungen {
    [CreateAssetMenu(menuName = "DungenGenerator/GeneratorData",fileName = "DungenGeneratorData")]
    public class DungenGeneratorData : ScriptableObject
    {
        public List<RoomSet> m_roomSets;
        
        public int GetSpawnRoomCount(int _setIndex) {
            return m_roomSets[_setIndex].m_spawnRooms.Count;
        }

        public Room GetSpawnRoom(int _setIndex,int _roomIndex) {
            return m_roomSets[_setIndex].m_spawnRooms[_roomIndex];
        }
        public Door GetDoor(int _setIndex, int _doorIndex)
        {
            return m_roomSets[_setIndex].m_doors[_doorIndex];
        }

        public List<Room> GetRoomTemplates(int _setIndex) {
            return m_roomSets[_setIndex].m_roomTemplates;
        }
    }
}

