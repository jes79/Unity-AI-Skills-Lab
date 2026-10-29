using UnityEngine;

// 0501 점수를 얻을 때 동전이 통 튀어 오르는 연출 (A·B 결과 코드가 같이 쓰는 부품)
public class ScorePop : MonoBehaviour
{
    [SerializeField] float jumpHeight = 0.8f;   // 튀어 오르는 높이
    [SerializeField] float duration = 0.35f;    // 한 번 튀는 시간
    [SerializeField] float spinSpeed = 120f;    // 평소 도는 빠르기 (도/초)

    Vector3 startPosition;
    float timer;

    void Awake()
    {
        startPosition = transform.position;
    }

    public void Play()
    {
        timer = duration;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        if (timer <= 0f) return;
        timer = Mathf.Max(0f, timer - Time.deltaTime);
        float t = 1f - timer / duration;
        transform.position = startPosition + Vector3.up * Mathf.Sin(t * Mathf.PI) * jumpHeight;
    }
}
