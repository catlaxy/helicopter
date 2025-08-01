using UnityEngine;
using System.Collections.Generic;

public class FuseBoxPuzzle : PuzzleBase
{
    public FuseBlock[] blocks;
    public Vector2Int startPosition;
    public Vector2Int endPosition;

    private int startBlockIndex;
    private int endBlockIndex;

    private Dictionary<Vector2Int, FuseBlock> gridMap;

    public override void StartPuzzle()
    {
        blocks = GetComponentsInChildren<FuseBlock>();
        gridMap = new Dictionary<Vector2Int, FuseBlock>();

        for (int i = 0; i < blocks.Length; i++)
        {
            gridMap[blocks[i].gridPosition] = blocks[i];
        }

        startBlockIndex = GetBlockIndexAt(startPosition);
        endBlockIndex = GetBlockIndexAt(endPosition);

        Debug.Log($"[FuseBoxPuzzle] 시작 인덱스: {startBlockIndex}, 도착 인덱스: {endBlockIndex}");
    }

    public void CheckCurrentFlow()
    {
        Debug.Log("[FuseBoxPuzzle] 흐름 검사 시작");

        bool[] visited = new bool[blocks.Length];
        List<Vector2Int> path = new List<Vector2Int>();

        SimulateCurrentFlow(blocks[startBlockIndex], -1, visited, path);

        if (visited[endBlockIndex])
        {
            Debug.Log("[FuseBoxPuzzle] 도착점까지 전류 도달 → 실패");
            Debug.Log("[FuseBoxPuzzle] 흐른 경로: " + string.Join(" -> ", path));
        }
        else
        {
            Debug.Log("[FuseBoxPuzzle] 전류 차단됨 → 성공");
            Debug.Log("[FuseBoxPuzzle] 흐른 경로: " + string.Join(" -> ", path));
            EndPuzzle(true);
        }
    }

    private void SimulateCurrentFlow(FuseBlock current, int cameFromDir, bool[] visited, List<Vector2Int> path)
    {
        int index = GetBlockIndexAt(current.gridPosition);
        if (index < 0 || index >= blocks.Length) return;
        if (visited[index]) return;

        visited[index] = true;
        path.Add(current.gridPosition);

        for (int dir = 0; dir < 4; dir++)
        {
            if (cameFromDir != -1 && dir == (cameFromDir + 2) % 4)
                continue;

            if (!current.connections[dir]) continue;

            Vector2Int nextPos = current.gridPosition + DirectionToVector(dir);

            if (!gridMap.TryGetValue(nextPos, out var nextBlock))
                continue;

            int oppositeDir = (dir + 2) % 4;
            if (!nextBlock.connections[oppositeDir]) continue;

            SimulateCurrentFlow(nextBlock, dir, visited, path);
        }
    }

    private int GetBlockIndexAt(Vector2Int pos)
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].gridPosition == pos) return i;
        }
        return -1;
    }

    private Vector2Int DirectionToVector(int dir)
    {
        return dir switch
        {
            0 => new Vector2Int(0, -1),  // 상: 위쪽이 Y감소
            1 => new Vector2Int(1, 0),   // 우
            2 => new Vector2Int(0, 1),   // 하: 아래쪽이 Y증가
            3 => new Vector2Int(-1, 0),  // 좌
            _ => Vector2Int.zero,
        };
    }
}
