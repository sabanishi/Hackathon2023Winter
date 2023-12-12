using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;

namespace Hackathon2023Winter
{
    public class RoomList:IEnumerable<RoomInfo>
    {
        private Dictionary<string, RoomInfo> _dictionary;

        public void Setup()
        {
            _dictionary = new();
        }
        
        public void Cleanup()
        {
            _dictionary.Clear();
        }
        
        public void Update(List<RoomInfo> changedRoomList)
        {
            foreach (var roomInfo in changedRoomList)
            {
                if (roomInfo.RemovedFromList)
                {
                    _dictionary.Remove(roomInfo.Name);
                }
                else
                {
                    _dictionary[roomInfo.Name] = roomInfo;
                }
            }
        }
        
        public bool TryGetRoomInfo(string roomName, out RoomInfo roomInfo)
        {
            return _dictionary.TryGetValue(roomName, out roomInfo);
        }
        
        public IEnumerator<RoomInfo> GetEnumerator()
        {
            foreach(var pair in _dictionary)
            {
                yield return pair.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}