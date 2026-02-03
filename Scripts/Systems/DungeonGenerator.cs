using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation; // Cần cài package AI Navigation để Bake NavMesh tự động

public class DungeonGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int totalSteps = 100;    // Số bước đi (Map càng to số càng lớn)
    public int walkerCount = 10;    // Số lượng 'con bọ' đi cùng lúc (tạo nhiều nhánh)
    public float gridUnit = 2f;     // Kích thước 1 ô (Ví dụ tường của bạn rộng 2m thì điền 2)

    [Header("Prefabs References")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject wallWithPillarPrefab; // Tường có cột
    public GameObject torchPrefab;
    public GameObject cratePrefab;
    public GameObject pillarPrefab; // Cột đứng riêng (nếu cần)

    [Header("Decoration Chance (0 - 1)")]
    [Range(0, 1)] public float wallPillarChance = 0.2f; // 20% tường sẽ có cột
    [Range(0, 1)] public float torchChance = 0.3f;      // 30% tường có đuốc
    [Range(0, 1)] public float crateChance = 0.1f;      // 10% sàn có thùng

    // Lưu trữ vị trí các ô sàn
    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    private GameObject mapParent; // Để gom tất cả vào 1 object cho gọn

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        // 1. Dọn dẹp map cũ (nếu có)
        if (mapParent != null) Destroy(mapParent);
        mapParent = new GameObject("Generated Dungeon");
        floorPositions.Clear();

        // 2. Tạo vị trí sàn (Logic Random Walker)
        RunRandomWalkers();

        // 3. Spawn sàn và đồ trang trí trên sàn
        foreach (var pos in floorPositions)
        {
            SpawnFloor(pos);
        }

        // 4. Spawn tường bao quanh và trang trí trên tường
        CreateWalls();

        // 5. Quan trọng: Build lại NavMesh cho quái di chuyển
        // Yêu cầu: Cài NavMeshSurface component vào MapGenerator hoặc một object cha
        if (GetComponent<NavMeshSurface>())
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    void RunRandomWalkers()
    {
        Vector2Int currentPos = Vector2Int.zero;
        floorPositions.Add(currentPos); // Vị trí bắt đầu luôn là sàn

        for (int i = 0; i < walkerCount; i++)
        {
            Vector2Int walkerPos = currentPos;
            for (int step = 0; step < totalSteps; step++)
            {
                // Random hướng đi: 0=Lên, 1=Phải, 2=Xuống, 3=Trái
                int direction = Random.Range(0, 4);
                switch (direction)
                {
                    case 0: walkerPos += Vector2Int.up; break;
                    case 1: walkerPos += Vector2Int.right; break;
                    case 2: walkerPos += Vector2Int.down; break;
                    case 3: walkerPos += Vector2Int.left; break;
                }
                floorPositions.Add(walkerPos); // HashSet tự loại bỏ trùng lặp
            }
        }
    }

    void SpawnFloor(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x * gridUnit, 0, pos.y * gridUnit);
        GameObject floor = Instantiate(floorPrefab, worldPos, Quaternion.identity, mapParent.transform);

        // --- Logic đặt thùng gỗ (Crate) ---
        // Chỉ đặt thùng nếu random trúng VÀ không phải vị trí (0,0) (để Player spawn)
        if (Random.value < crateChance && pos != Vector2Int.zero)
        {
            // Random xoay thùng cho tự nhiên
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
            Instantiate(cratePrefab, worldPos, randomRot, mapParent.transform);
        }
    }

    void CreateWalls()
    {
        // Duyệt qua từng ô sàn, kiểm tra 4 hướng xung quanh nó
        foreach (var floorPos in floorPositions)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = floorPos + dir;

                // Nếu hướng này KHÔNG CÓ sàn -> Nghĩa là cần xây tường ngăn cách
                if (!floorPositions.Contains(neighborPos))
                {
                    SpawnWall(neighborPos, dir);
                }
            }
        }
    }

    void SpawnWall(Vector2Int wallPos, Vector2Int directionToFloor)
    {
        // Tính vị trí tường
        Vector3 worldPos = new Vector3(wallPos.x * gridUnit, 0, wallPos.y * gridUnit);
        
        // Tính góc xoay tường (để mặt tường quay vào trong sàn)
        // Lưu ý: Tùy vào Pivot của model tường bạn mà chỉnh LookRotation
        // Ở đây giả định model tường mặt trước hướng về phía Z+
        Quaternion rotation = Quaternion.LookRotation(new Vector3(-directionToFloor.x, 0, -directionToFloor.y));

        // --- Logic chọn loại tường (Có cột hay không) ---
        GameObject prefabToSpawn = wallPrefab;
        if (Random.value < wallPillarChance && wallWithPillarPrefab != null)
        {
            prefabToSpawn = wallWithPillarPrefab;
        }

        GameObject wall = Instantiate(prefabToSpawn, worldPos, rotation, mapParent.transform);

        // --- Logic gắn đuốc (Torch) ---
        if (Random.value < torchChance && torchPrefab != null)
        {
            // Gắn đuốc làm con của tường
            GameObject torch = Instantiate(torchPrefab, wall.transform);
            
            // Chỉnh vị trí đuốc (Ví dụ: Cao lên 1.5m, nhô ra trước 0.3m)
            // Bạn cần tự chỉnh số này khớp với model của bạn
            torch.transform.localPosition = new Vector3(0, 1.5f, 0.3f); 
            torch.transform.localRotation = Quaternion.identity;
        }
    }
}