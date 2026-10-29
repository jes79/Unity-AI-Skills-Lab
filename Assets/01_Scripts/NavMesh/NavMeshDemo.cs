using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// 0404 길찾기 설정 (/initialize-ai-navigation)
// 바닥을 클릭하면 그곳으로 에이전트(NavMeshAgent)를 보낸다
// - A: 옛 방식 — 바닥·장애물에 정적(Static) 표시만 하고 'Navigation 창에서 Bake' 안내만 따름
//      → Unity 6 방식인 NavMeshSurface 로 굽지 않아 NavMesh 가 없음 → 에이전트가 경고를 남기고 멈춤
//      (A 에서는 씬의 NavMeshSurface 를 꺼서 '구운 NavMesh 가 없는 상태'를 재현한다)
// - B: NavMeshSurface 컴포넌트로 구운 NavMesh (씬을 만들 때 구워 둠) → 장애물을 돌아서 감
public class NavMeshDemo : AbCompareScene
{
    // NavMesh 가 하나도 없을 때 에이전트를 켜면 Unity 가 콘솔에 남기는 경고 (6000.6.7f1 플레이어 로그에서 확인한 문장)
    const string AgentWarning = "Failed to create agent because there is no valid NavMesh";

    [SerializeField] NavMeshSurface surface;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform targetMarker;
    [SerializeField] Camera viewCamera;
    [SerializeField] MeshFilter navMeshView;          // 구운 NavMesh 를 바닥 위에 반투명하게 보여 줄 곳
    [SerializeField] LineRenderer pathLine;           // 에이전트가 갈 길
    [SerializeField] Vector3 demoTarget = new Vector3(6f, 0f, 5f);  // B 로 바꾸면 먼저 가 보는 곳 (벽 뒤)

    Label infoLabel, warningLabel;
    Vector3 startPosition;

    protected override void BuildSceneUi()
    {
        startPosition = agent.transform.position;
        var panel = AddPanel(Root, 150f, 820f, 1050f);
        infoLabel = AddText(panel, "", 20f, new Color(0.85f, 0.9f, 0.95f));
        warningLabel = AddText(panel, "", 22f, new Color(1f, 0.78f, 0.35f));
        warningLabel.style.marginTop = 6f;
    }

    protected override void ShowSide(bool useSkill)
    {
        // 에이전트를 껐다가 NavMesh 상태를 바꾼 다음 다시 켠다 (켜질 때 가까운 NavMesh 를 찾음)
        agent.enabled = false;
        agent.transform.position = startPosition;
        surface.enabled = useSkill;      // NavMeshSurface 가 꺼지면 구운 NavMesh 도 빠진다
        agent.enabled = true;            // A 에서는 여기서 Unity 가 위 경고를 콘솔에 남긴다

        navMeshView.gameObject.SetActive(useSkill);
        if (useSkill) ShowNavMesh();
        pathLine.positionCount = 0;

        infoLabel.text = useSkill
            ? "NavMeshSurface 컴포넌트(AI Navigation 패키지)로 바닥·장애물을 구웠다 (하늘색 = NavMesh)\n바닥을 클릭하면 장애물을 돌아서 간다"
            : "옛 튜토리얼 안내만 따름: 바닥·장애물에 Static 체크 → 'Navigation 창에서 Bake'\nUnity 6 방식인 NavMeshSurface 가 씬에 없어(꺼져 있어) NavMesh 가 없다";
        warningLabel.text = agent.isOnNavMesh ? "" : $"[경고] Unity 콘솔에 나오는 경고와 같은 문장을 옮겨 적음: \"{AgentWarning}\"";

        if (useSkill) MoveTo(demoTarget);
    }

    protected override void Update()
    {
        base.Update();
        UpdatePathLine();

        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
        // 강의 화면의 말풍선·버튼을 누른 클릭은 바닥 클릭으로 치지 않음
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var ray = viewCamera.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 200f))
            MoveTo(hit.point);
    }

    void MoveTo(Vector3 point)
    {
        if (!agent.isOnNavMesh)
        {
            warningLabel.text = $"[경고] 에이전트가 NavMesh 위에 없어 SetDestination 을 할 수 없음 (isOnNavMesh = false)\nUnity 콘솔에 나오는 경고와 같은 문장을 옮겨 적음: \"{AgentWarning}\"";
            return;
        }
        agent.SetDestination(point);
    }

    void UpdatePathLine()
    {
        if (!agent.isOnNavMesh || !agent.hasPath)
        {
            pathLine.positionCount = 0;
            return;
        }
        var corners = agent.path.corners;
        pathLine.positionCount = corners.Length;
        for (int i = 0; i < corners.Length; i++)
            pathLine.SetPosition(i, corners[i] + Vector3.up * 0.1f);
    }

    // 구운 NavMesh 의 삼각형을 그대로 메시로 만들어 바닥 위에 보여 준다
    void ShowNavMesh()
    {
        var triangulation = NavMesh.CalculateTriangulation();
        var mesh = new Mesh { name = "NavMeshView" };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = triangulation.vertices;
        mesh.triangles = triangulation.indices;
        mesh.RecalculateNormals();
        if (navMeshView.sharedMesh != null) Destroy(navMeshView.sharedMesh);
        navMeshView.sharedMesh = mesh;
    }
}
