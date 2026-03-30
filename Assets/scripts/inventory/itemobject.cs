using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemobject : MonoBehaviour
{
    [Header("物品设置")]
    public itemdata item;
    public CircleCollider2D cc;

    [Header("收集品设置")]
    [SerializeField] private bool isCollectible = false; 
    [SerializeField][Range(0, 9)] private int collectibleIndex = 0; 

    [Header("视觉效果(可选)")]
    [SerializeField] private GameObject collectEffect; // 收集特效
    [SerializeField] private AudioClip collectSound; // 收集音效

    void Start()
    {
        cc = GetComponent<CircleCollider2D>();

        if (isCollectible && IsAlreadyCollected())
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (item != null)
        {
            inventory.instance.AddItem(item);
        }

        if (isCollectible)
        {
            MarkAsCollected();
        }

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        Destroy(gameObject);
    }

    private bool IsAlreadyCollected()
    {
        if (BoolManager.Instance == null)
        {
            Debug.LogWarning("[itemobject] BoolManager.Instance 为空，无法检查收集状态");
            return false;
        }

        return BoolManager.Instance.GetBool(collectibleIndex);
    }

    private void MarkAsCollected()
    {
        if (BoolManager.Instance == null)
        {
            Debug.LogWarning("[itemobject] BoolManager.Instance 为空，无法保存收集状态");
            return;
        }

        BoolManager.Instance.SetBool(collectibleIndex, true);
        Debug.Log($"[itemobject] 收集品 {collectibleIndex} 已收集!");
    }

    private void OnValidate()
    {
        // 限制索引范围
        collectibleIndex = Mathf.Clamp(collectibleIndex, 0, 9);
    }

    private void OnDrawGizmosSelected()
    {
        if (isCollectible)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"收集品 #{collectibleIndex}"
            );
#endif
        }
    }
}