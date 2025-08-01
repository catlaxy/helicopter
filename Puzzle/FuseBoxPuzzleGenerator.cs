using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// FuseBox 퍼즐의 블록을 자동 배치하고,
/// 퍼즐 본체(FuseBoxPuzzle)를 자동으로 찾아 시작까지 수행하는 제너레이터입니다.
/// </summary>
public class FuseBoxPuzzleGenerator : MonoBehaviour
{
    [Header("블록 프리팹 리스트")]
    [Tooltip("배치할 블록 프리팹들을 순서대로 넣으세요.")]
    public List<GameObject> blockPrefabs;

    [Header("블록 배치 부모")]
    [Tooltip("생성된 블록들을 자식으로 넣을 부모 Transform")]
    public Transform parentTransform;

    [Header("블록 간격")]
    [Tooltip("블록 간의 좌표 간격")]
    public float spacing = 1.1f;

    [Header("블록 배치 데이터")]
    [Tooltip("블록 위치 및 회전 설정")]
    public List<BlockPlacement> blockPlacements;

    /// <summary>
    /// 퍼즐 본체 (자동 연결됨)
    /// </summary>
    private FuseBoxPuzzle fuseBoxPuzzle;

    /// <summary>
    /// 씬 시작 시 퍼즐 블록을 생성하고 퍼즐을 자동으로 시작합니다.
    /// </summary>
    private void Start()
    {
        // 퍼즐 자동 연결 시도
        fuseBoxPuzzle = FindFirstObjectByType<FuseBoxPuzzle>();
        if (fuseBoxPuzzle == null)
        {
            Debug.LogError("[FuseBoxPuzzleGenerator] FuseBoxPuzzle을 씬에서 찾을 수 없습니다.");
            return;
        }

        GeneratePuzzle();
        fuseBoxPuzzle.StartPuzzle();
        Debug.Log("[FuseBoxPuzzleGenerator] 퍼즐 자동 시작 완료");
    }

    /// <summary>
    /// 퍼즐 블록을 자동 생성합니다. 퍼즐 시작은 호출자 책임입니다.
    /// </summary>
    public void GeneratePuzzle()
    {
        foreach (var placement in blockPlacements)
        {
            GameObject prefab = blockPrefabs[placement.prefabIndex];
            GameObject block = Instantiate(prefab, parentTransform);
            block.transform.localPosition = new Vector3(
                placement.position.x * spacing,
                -placement.position.y * spacing,
                0
            );
            // 회전 제거됨
            // block.transform.localRotation = Quaternion.Euler(0f, 0f, placement.rotation);

            FuseBlock fuseBlock = block.GetComponent<FuseBlock>();
            if (fuseBlock != null)
            {
                fuseBlock.gridPosition = placement.position;
            }

            block.name = $"Block_{placement.prefabIndex}_({placement.position.x},{placement.position.y})";
        }

        Debug.Log("[FuseBoxPuzzleGenerator] 퍼즐 블록 자동 생성 완료");
    }

    [System.Serializable]
    public struct BlockPlacement
    {
        public Vector2Int position;
        public int prefabIndex;
        public float rotation;
    }
}
