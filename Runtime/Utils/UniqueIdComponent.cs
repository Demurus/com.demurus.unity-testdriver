using UnityEngine;

namespace UnityTestDriver.Runtime.Utils
{
    public class UniqueIdComponent : MonoBehaviour
    {
        [Tooltip("Unique ID to distinguish between copies")]
        [SerializeField]
        private string _uniqueId;

        public string UniqueId => _uniqueId;
    }
}
