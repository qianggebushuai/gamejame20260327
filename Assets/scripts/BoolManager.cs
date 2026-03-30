using UnityEngine;

/// <summary>
/// 跨场景单例管理器 - 存储10个布尔值
/// 使用方法: BoolManager.Instance.SetBool(0, true); bool value = BoolManager.Instance.GetBool(0);
/// </summary>
public class BoolManager : MonoBehaviour
{
    // 单例实例
    public static BoolManager Instance { get; private set; }

    // 存储10个布尔值的数组
    [SerializeField] private bool[] boolValues = new bool[10];

    // 可选：在Inspector中显示的名称（方便调试）
    [SerializeField] private string[] boolNames = new string[10];

    // 事件：当任意布尔值改变时触发
    public delegate void BoolValueChangedHandler(int index, bool newValue);
    public event BoolValueChangedHandler OnBoolValueChanged;

    private void Awake()
    {
        // 单例模式实现
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景保留

        // 初始化默认名称
        if (boolNames[0] == null)
        {
            for (int i = 0; i < 10; i++)
            {
                boolNames[i] = $"Bool_{i}";
            }
        }
    }

    #region 公共接口

    /// <summary>
    /// 获取指定索引的布尔值
    /// </summary>
    /// <param name="index">索引 0-9</param>
    /// <returns>布尔值</returns>
    public bool GetBool(int index)
    {
        ValidateIndex(index);
        return boolValues[index];
    }

    /// <summary>
    /// 设置指定索引的布尔值
    /// </summary>
    /// <param name="index">索引 0-9</param>
    /// <param name="value">要设置的值</param>
    public void SetBool(int index, bool value)
    {
        ValidateIndex(index);

        if (boolValues[index] != value)
        {
            boolValues[index] = value;
            OnBoolValueChanged?.Invoke(index, value); // 触发事件
            Debug.Log($"[BoolManager] {boolNames[index]} 改变为: {value}");
        }
    }

    /// <summary>
    /// 切换指定索引的布尔值（true变false，false变true）
    /// </summary>
    /// <param name="index">索引 0-9</param>
    public void ToggleBool(int index)
    {
        ValidateIndex(index);
        SetBool(index, !boolValues[index]);
    }

    /// <summary>
    /// 将所有布尔值重置为false
    /// </summary>
    public void ResetAll()
    {
        for (int i = 0; i < 10; i++)
        {
            SetBool(i, false);
        }
        Debug.Log("[BoolManager] 所有布尔值已重置");
    }

    /// <summary>
    /// 设置指定索引的布尔值名称（用于调试）
    /// </summary>
    public void SetBoolName(int index, string name)
    {
        ValidateIndex(index);
        boolNames[index] = name;
    }

    /// <summary>
    /// 获取指定索引的布尔值名称
    /// </summary>
    public string GetBoolName(int index)
    {
        ValidateIndex(index);
        return boolNames[index];
    }

    /// <summary>
    /// 获取所有布尔值的当前状态（用于保存系统）
    /// </summary>
    public bool[] GetAllValues()
    {
        return (bool[])boolValues.Clone();
    }

    /// <summary>
    /// 批量设置所有布尔值（用于加载系统）
    /// </summary>
    public void SetAllValues(bool[] values)
    {
        if (values == null || values.Length != 10)
        {
            Debug.LogError("[BoolManager] 数组长度必须为10");
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            SetBool(i, values[i]);
        }
    }

    #endregion

    #region 辅助方法

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= 10)
        {
            throw new System.ArgumentOutOfRangeException(nameof(index), "索引必须在 0-9 范围内");
        }
    }

    #endregion

    // 在Inspector中显示当前状态
    private void OnValidate()
    {
        if (boolValues == null || boolValues.Length != 10)
            boolValues = new bool[10];
        if (boolNames == null || boolNames.Length != 10)
            boolNames = new string[10];
    }
}