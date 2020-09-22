
using UnityEngine;

namespace kumaS.Tracker
{
    public abstract class SendDataBase
    {
        public abstract void Send();
        public abstract void SetOffsets(Vector3[] offsets);
    }
}
