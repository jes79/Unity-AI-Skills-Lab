using UnityEngine;

// 0402 총알 한 발: 벽에 맞으면 그 자리에 멈추고, 벽을 지나쳐 버리면 '뚫음'으로 센다
[RequireComponent(typeof(Rigidbody))]
public class FastBullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 2f;   // 이 시간이 지나면 사라짐

    FastBulletDemo demo;
    float passX;          // 이 x 를 넘으면 벽을 뚫고 지나간 것
    bool finished;

    public void Fire(FastBulletDemo owner, float speed, CollisionDetectionMode mode, float wallBackX)
    {
        demo = owner;
        passX = wallBackX;
        var body = GetComponent<Rigidbody>();
        body.collisionDetectionMode = mode;
        body.linearVelocity = Vector3.right * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (finished) return;
        finished = true;
        var body = GetComponent<Rigidbody>();
        body.linearVelocity = Vector3.zero;
        body.isKinematic = true;
        demo.ReportHit(collision.GetContact(0).point);
    }

    void FixedUpdate()
    {
        if (finished || transform.position.x < passX) return;
        finished = true;
        demo.ReportPass();
    }
}
