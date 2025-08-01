using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 게임의 모든 영구 저장 데이터를 관리하는 싱글톤 매니저 클래스입니다.
/// JSON 직렬화/역직렬화로 데이터를 파일에 저장·로드하며,
/// runtime 데이터가 인스펙터에 표시되어 편리하게 모니터링할 수 있습니다.
/// </summary>
public class SaveManager : MonoBehaviour
{
    /// <summary>
    /// 어디서든 접근 가능한 싱글톤 인스턴스입니다.
    /// </summary>
    public static SaveManager Instance { get; private set; }

    [Header("---- Runtime 저장 데이터 (▶ Play 모드에서 확인) ----")]
    [Tooltip("JSON으로 직렬화되는 저장 데이터 전체를 인스펙터에서 표시합니다.")]
    [SerializeField] private SaveData data;              // 인스펙터에 노출되는 저장 데이터

    [HideInInspector] private string savePath;           // 파일 경로 (인스펙터 표시 불필요)

    /// <summary>
    /// 저장할 모든 데이터를 담는 구조체.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Tooltip("언락된 씬 키 목록")]
        public List<string> unlockedScenes = new List<string>();

        [Tooltip("인벤토리에 획득된 아이템 ID 목록")]
        public List<string> inventoryItems = new List<string>();

        [Tooltip("갤러리 엔트리 ID 목록")]
        public List<string> galleryEntries = new List<string>();

        [Tooltip("메시지 엔트리 ID 목록")]
        public List<string> messageEntries = new List<string>();

        [Tooltip("현재 오디오 모드 (true = 진동, false = 소리)")]
        public bool isVibrateMode = false;
    }

    public string lastSceneBeforeGameOver;

    /// <summary>
    /// Awake 시점에 싱글톤 설정, 파일 경로 초기화 및 저장 데이터 로드 수행.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 경로 지정 (예: Application.persistentDataPath 하위)
            savePath = Path.Combine(Application.persistentDataPath, "saveData.json");

            LoadGame(); // 누락되어 있다면 이 줄도 호출 필요
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 현재 데이터를 JSON으로 직렬화하여 파일에 기록합니다.
    /// </summary>
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(savePath, json);
    }

    /// <summary>
    /// 저장된 JSON 파일을 읽어 메모리에 로드합니다.
    /// 파일이 없으면 새로운 SaveData를 생성합니다.
    /// </summary>
    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);

            // null 체크
            if (data.unlockedScenes == null) data.unlockedScenes = new List<string>();
            if (data.inventoryItems == null) data.inventoryItems = new List<string>();
            if (data.galleryEntries == null) data.galleryEntries = new List<string>();
            if (data.messageEntries == null) data.messageEntries = new List<string>();
        }
        else
        {
            data = new SaveData();
        }
    }

    /// <summary>
    /// 현재 저장된 오디오 모드를 반환합니다.
    /// </summary>
    /// <returns>true면 진동 모드, false면 소리 모드</returns>
    public bool IsVibrateMode()
    {
        return data.isVibrateMode;
    }

    /// <summary>
    /// 오디오 모드를 저장 데이터에 기록하고 즉시 파일에 저장합니다.
    /// </summary>
    /// <param name="vibrate">진동 모드 여부 (true: 진동, false: 소리)</param>
    public void SetVibrateMode(bool vibrate)
    {
        data.isVibrateMode = vibrate;
        SaveGame();
    }

    /// <summary>
    /// 지정한 키(씬)가 이미 언락되었는지 확인합니다.
    /// </summary>
    public bool IsSceneUnlocked(string sceneKey)
    {
        return data.unlockedScenes.Contains(sceneKey);
    }

    /// <summary>
    /// 씬 언락 목록에 추가하고 저장합니다.
    /// </summary>
    public void UnlockScene(string sceneKey)
    {
        if (!data.unlockedScenes.Contains(sceneKey))
        {
            data.unlockedScenes.Add(sceneKey);
            SaveGame();
        }
    }

    /// <summary>
    /// 인벤토리 아이템 ID를 저장 목록에 추가하고 저장합니다.
    /// </summary>
    public void AddInventoryItem(string itemId)
    {
        if (!data.inventoryItems.Contains(itemId))
        {
            data.inventoryItems.Add(itemId);
            SaveGame();
        }
    }

    /// <summary>
    /// 저장된 인벤토리 아이템 목록을 복사하여 반환합니다.
    /// </summary>
    public List<string> GetInventoryItems()
    {
        return new List<string>(data.inventoryItems);
    }

    // (galleryEntries, messageEntries 관련 메서드도 기존 패턴과 동일하게 추가 구현 가능)
}
