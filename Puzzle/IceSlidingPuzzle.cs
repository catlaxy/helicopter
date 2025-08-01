using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


/// <summary>
/// PuzzleBase를 상속받아 순차 퍼즐 진행 + 진행도 업데이트 + 퍼즐 종료 후 오브젝트 비활성화를 수행하는 스크립트입니다.
/// </summary>
public class IceSlidingPuzzle : PuzzleBase
{
    [Header("씬 종료 대사 처리용")]
    public DialogStorage dialogStorage;
    public int endIndex = 0;  // 이 값을 인스펙터에서 0 또는 1 등으로 설정하세요

    [Header("퍼즐 패널 리스트 (순서대로)")]
    public List<GameObject> puzzlePanels;

    [Header("퍼즐 종료 시 비활성화할 오브젝트들")]
    public List<GameObject> objectsToDeactivate;

    [Header("진행도 UI")]
    public Slider progressBar;

    [Header("충돌 레이어 설정")]
    public LayerMask wallLayerMask;

    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("충돌 감지 설정")]
    [Tooltip("벽에 얼마나 가까이 가면 멈출지 결정하는 거리")]
    public float wallDetectDistance = 1.0f;

    [Tooltip("벽에 닿았을 때 얼마나 튕겨나올지 결정하는 거리")]
    public float bounceBackDistance = 0.1f;


    [Header("디버그")]
    public bool isMoving = false;

    private int currentIndex = 0;

    private GameObject player;
    private Rigidbody2D playerRb;
    private Collider2D playerCol;
    private List<Collider2D> wallColliders = new();
    private Collider2D goalCollider;
    private Vector2 moveDirection;

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        currentIndex = 0;

        if (progressBar != null)
        {
            progressBar.maxValue = puzzlePanels.Count;
            progressBar.value = 0;
        }

        ActivatePanel(currentIndex);
    }

    private void Update()
    {
        if (isMoving || player == null) return;

        if (Input.GetKeyDown(KeyCode.RightArrow)) moveDirection = Vector2.right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) moveDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) moveDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) moveDirection = Vector2.down;
        else return;

        StartMovement();
    }
    private bool IsNearWall()
    {
        RaycastHit2D[] results = new RaycastHit2D[1];
        int hitCount = playerCol.Cast(moveDirection, results, wallDetectDistance);

        return hitCount > 0 && ((1 << results[0].collider.gameObject.layer) & wallLayerMask) != 0;
    }

    private void FixedUpdate()
    {
        if (!isMoving || playerCol == null) return;

        foreach (var wall in wallColliders)
        {
            if (IsNearWall())
            {
                StopMovement();
                return;
            }

        }

        if (goalCollider != null && playerCol.IsTouching(goalCollider))
        {
            StopMovement();
            AdvanceToNextPanel();
        }
    }

    private void StartMovement()
    {
        isMoving = true;
        playerRb.linearVelocity = moveDirection * moveSpeed;
    }

    private void StopMovement()
    {
        isMoving = false;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.position -= moveDirection * bounceBackDistance;
    }

    private void AdvanceToNextPanel()
    {
        Debug.Log($"패널 {currentIndex + 1} 클리어");

        if (progressBar != null)
            progressBar.value += 1;

        puzzlePanels[currentIndex].SetActive(false);
        currentIndex++;

        if (currentIndex < puzzlePanels.Count)
        {
            ActivatePanel(currentIndex);
        }
        else
        {
            Debug.Log("모든 퍼즐 완료!");

            DeactivateObjects();

            if (dialogStorage != null)
            {
                dialogStorage.PlayEndAndLoadNext(endIndex); // 🎯 씬 종료 대사 + 페이드 + 씬전환 호출
            }

            EndPuzzle(true); // PuzzleBase 이벤트 발동
        }
    }

    private void ActivatePanel(int index)
    {
        for (int i = 0; i < puzzlePanels.Count; i++)
            puzzlePanels[i].SetActive(i == index);

        GameObject panel = puzzlePanels[index];

        player = panel.transform.Find("Player")?.gameObject;
        playerRb = player?.GetComponent<Rigidbody2D>();
        playerCol = player?.GetComponent<Collider2D>();

        wallColliders.Clear();
        Transform wallRoot = panel.transform.Find("Walls");
        if (wallRoot != null)
        {
            foreach (Transform wall in wallRoot)
            {
                Collider2D col = wall.GetComponent<Collider2D>();
                if (col != null)
                    wallColliders.Add(col);
            }
        }

        goalCollider = panel.transform.Find("Goal")?.GetComponent<Collider2D>();
        ResetPlayerPosition();
    }

    private void ResetPlayerPosition()
    {
        Transform start = puzzlePanels[currentIndex].transform.Find("StartPosition");
        if (start != null && player != null)
            player.transform.position = start.position;

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        isMoving = false;
    }

    /// <summary>
    /// 퍼즐 완료 후 지정된 오브젝트들을 비활성화
    /// </summary>
    private void DeactivateObjects()
    {
        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
