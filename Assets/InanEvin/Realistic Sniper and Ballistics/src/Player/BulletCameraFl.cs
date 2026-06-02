using UnityEngine;

namespace IE.RSB 
{
    public class BulletCameraFl : MonoBehaviour
    {
        [SerializeField] private Transform bullet;
        [SerializeField] private Vector3 offset = new Vector3(0, 0.1f, -1f);
        [SerializeField] private float smoothSpeed = 10f;

        public void SetTarget(Transform bulletTarget)
        {
            bullet = bulletTarget;
        }

        private void LateUpdate()
        {
            if (bullet == null) return;

            Vector3 targetPosition = bullet.position
                                    + bullet.forward * offset.z
                                    + bullet.up * offset.y
                                    + bullet.right * offset.x;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
            transform.rotation = Quaternion.LookRotation(bullet.forward);
        }
    }
}
