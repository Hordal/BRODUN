// CameraFollow.cs — A 캐릭터 추적 카메라 (슬라이스).
using UnityEngine;

namespace BroDungeon.Slice
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 1.5f, -10f);
        public float smooth = 8f;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 goal = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, goal, smooth * Time.deltaTime);
        }
    }
}
