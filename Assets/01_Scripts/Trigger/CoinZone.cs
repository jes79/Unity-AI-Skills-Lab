using UnityEngine;

// 0403 동전 영역 (Is Trigger 가 켜진 Box Collider, Rigidbody 없음)
// 무언가 들어오면 OnTriggerEnter 가 불린다 — 단, 둘 중 하나에 Rigidbody 가 있을 때만
public class CoinZone : MonoBehaviour
{
    [SerializeField] TriggerDemo demo;
    [SerializeField] Renderer coinRenderer;
    [SerializeField] Color idleColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] Color hitColor = new Color(0.4f, 1f, 0.5f);
    [SerializeField] float flashTime = 0.4f;     // 불렸을 때 초록으로 빛나는 시간
    [SerializeField] float spinSpeed = 90f;      // 동전이 도는 빠르기 (도/초)

    float flashTimer;
    MaterialPropertyBlock block;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        SetColor(idleColor);
    }

    void OnTriggerEnter(Collider other)
    {
        flashTimer = flashTime;
        demo.ReportTrigger(name);
    }

    void Update()
    {
        coinRenderer.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        if (flashTimer <= 0f) return;
        flashTimer -= Time.deltaTime;
        SetColor(flashTimer > 0f ? hitColor : idleColor);
    }

    void SetColor(Color color)
    {
        coinRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        coinRenderer.SetPropertyBlock(block);
    }
}
