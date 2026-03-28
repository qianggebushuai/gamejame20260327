using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [Header("触发设置")]
    [SerializeField] private Collider2D triggerZone; // 指定的触发器
    [SerializeField] private LayerMask triggerLayers; // 可触发的层级

    [Header("Sprite列表")]
    [SerializeField] private List<SpriteRenderer> spriteList = new List<SpriteRenderer>();

    [Header("碰撞箱")]
    [SerializeField] private BoxCollider2D boxCollider;

    private bool hasTriggered = false;

    void Start()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        // 初始隐藏所有sprite
        SetSpritesActive(false);
    }

    void Update()
    {
        if (ScreenCoverTransition2D.instance == null) return;

        var currentState = ScreenCoverTransition2D.instance.currentState;

        // 当状态为Covered时，碰撞箱启用
        if (currentState == ScreenCoverTransition2D.State.Covered)
        {
            boxCollider.enabled = true;
        }
        else if (currentState != ScreenCoverTransition2D.State.Idle)
        {
            boxCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否为Idle状态
        if (ScreenCoverTransition2D.instance == null ||
            ScreenCoverTransition2D.instance.currentState != ScreenCoverTransition2D.State.Idle)
        {
            return;
        }

        // 检查是否为指定层级的物体
        if (((1 << other.gameObject.layer) & triggerLayers) == 0)
        {
            return;
        }

        // 防止重复触发
        if (hasTriggered) return;

        hasTriggered = true;
        ActivateSprites();
    }

    private void ActivateSprites()
    {
        SetSpritesActive(true);

        // 同时启用碰撞箱
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    private void SetSpritesActive(bool active)
    {
        foreach (var sprite in spriteList)
        {
            if (sprite != null)
            {
                sprite.enabled = active;
            }
        }
    }

    // 重置状态（可选，用于重新开始）
    public void ResetWind()
    {
        hasTriggered = false;
        SetSpritesActive(false);

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }
}