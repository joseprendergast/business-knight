using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightPlayer : MonoBehaviour
    {
        public static BusinessNightPlayer Instance { get; private set; }

        [SerializeField] float walkSpeed = 3.4f;
        [SerializeField] float minX = -4.2f;
        [SerializeField] float maxX = 3.45f;
        [SerializeField] float groundY = -1.03f;

        Vector3 target;
        SpriteRenderer spriteRenderer;

        void Awake()
        {
            Instance = this;
            target = transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            Vector3 current = transform.position;
            if (Mathf.Abs(current.x - target.x) < 0.015f)
                return;

            float nextX = Mathf.MoveTowards(current.x, target.x, walkSpeed * Time.deltaTime);
            transform.position = new Vector3(nextX, groundY, current.z);
        }

        public void WalkTo(float worldX)
        {
            float clamped = Mathf.Clamp(worldX, minX, maxX);
            target = new Vector3(clamped, groundY, transform.position.z);

            if (spriteRenderer != null && Mathf.Abs(clamped - transform.position.x) > 0.03f)
                spriteRenderer.flipX = clamped < transform.position.x;
        }
    }
}
