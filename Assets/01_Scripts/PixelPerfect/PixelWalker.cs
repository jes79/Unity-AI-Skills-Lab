using UnityEngine;

// 0303 도트 캐릭터가 좌우로 천천히 걷고, 카메라가 따라간다
// 위치는 일부러 픽셀 단위로 맞추지 않는다 (소수점 위치 = 서브픽셀) → 맞추는 일은 Pixel Perfect Camera 가 한다
public class PixelWalker : MonoBehaviour
{
    [SerializeField] SpriteRenderer body;
    [SerializeField] Sprite[] frames = new Sprite[2];  // 걷기 2장 (PixelPerfectDemo 가 A/B 그림으로 바꿔 넣음)
    [SerializeField] Transform followCamera;
    [SerializeField] float speed = 0.45f;              // 초당 이동 (월드 단위)
    [SerializeField] float range = 3.2f;               // 가운데에서 좌우로 갈 수 있는 거리
    [SerializeField] float frameTime = 0.25f;          // 걷기 그림 한 장 시간
    [SerializeField] float cameraLag = 4f;             // 카메라가 따라오는 빠르기
    [SerializeField] float cameraOffsetX = 1.6f;       // 캐릭터를 화면 왼쪽-가운데에 두기 (오른쪽은 강의 카드 자리)

    float direction = 1f;
    float frameTimer;
    int frameIndex;

    public void SetFrames(Sprite first, Sprite second)
    {
        frames[0] = first;
        frames[1] = second;
        body.sprite = frames[frameIndex];
    }

    void Update()
    {
        var position = transform.position;
        position.x += direction * speed * Time.deltaTime;
        if (Mathf.Abs(position.x) > range)
        {
            position.x = Mathf.Sign(position.x) * range;
            direction = -direction;
        }
        transform.position = position;
        body.flipX = direction < 0f;

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            frameIndex = 1 - frameIndex;
            body.sprite = frames[frameIndex];
        }
    }

    void LateUpdate()
    {
        // 부드럽게 따라가는 카메라: 위치가 소수점이 되어 도트가 화면 픽셀 사이에 걸친다
        var cameraPosition = followCamera.position;
        float targetX = transform.position.x + cameraOffsetX;
        cameraPosition.x = Mathf.Lerp(cameraPosition.x, targetX, 1f - Mathf.Exp(-cameraLag * Time.deltaTime));
        followCamera.position = cameraPosition;
    }
}
