using UnityEngine;

namespace ThinkInvisible.RadialPings {
    public class PingMenuInstanceTracker : MonoBehaviour {
        public GameObject latestMenu;
        public float btnHoldStopwatch = 0f;
        public bool btnHoldActioned = false;
        public bool currentPingIsCustom = false;
    }
}
