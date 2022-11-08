using UnityEngine;

namespace Assets.Entities
{
    internal class CameraFollow : MonoBehaviour
    {
        public GameObject Target;

        private void LateUpdate()
        {
            if (Target == null) return;
            var pos = Target.transform.position;
            pos.z = transform.position.z;
            transform.position = pos;
        }
    }
}
