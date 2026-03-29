using UnityEngine;


/// <summary>
/// 游戏进度存储和加载
/// </summary>
public class GameProgress : MonoBehaviour
{
    private const string LAST_LEVEL_KEY = "LastLevel";
    private const string HAS_SAVE_KEY = "HasSave";

    /// <summary>
    /// 保存当前关卡进度
    /// </summary>
    public static void SaveLevel(string levelName)
    {
        PlayerPrefs.SetString(LAST_LEVEL_KEY, levelName);
        PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log($"Progress saved: {levelName}");
    }

    /// <summary>
    /// 获取上次保存的关卡名
    /// </summary>
    public static string GetLastLevel()
    {
        return PlayerPrefs.GetString(LAST_LEVEL_KEY, "Level1");
    }

    /// <summary>
    /// 检查是否有存档
    /// </summary>
    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt(HAS_SAVE_KEY, 0) == 1;
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(LAST_LEVEL_KEY);
        PlayerPrefs.DeleteKey(HAS_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("Save data deleted");
    }

    /// <summary>
    /// 清空所有存档
    /// </summary>
    public static void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All data cleared");
    }
}