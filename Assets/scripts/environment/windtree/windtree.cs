using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windtree : MonoBehaviour
{
    [Header("生长设置")]
    [SerializeField] private float growtime = 5f;
    public Sprite seed;
    public Sprite tree;

    [Header("视觉过渡设置")]
    [Tooltip("过渡动画持续的时间")]
    public float transitionDuration = 1.5f;

    private SpriteRenderer sr;
    private Collider2D triggerCollider;
    public bool hasGrown = false;
    public LayerMask whatisground;
    public float groundCheckDistance;
    public Rigidbody2D rb;
    public virtual bool isgrounddetected() => Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatisground);
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        triggerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr.sprite = seed;
        sr.color = Color.white;
    }

    void Update()
    {
        if (!isgrounddetected())
        {
            rb.velocity= new Vector2(0, -1);
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }
        if (hasGrown) return;

        if (ScreenCoverTransition2D.instance != null)
        {
            if (ScreenCoverTransition2D.instance.currentState == ScreenCoverTransition2D.State.Idle)
            {
                growtime -= Time.deltaTime;
            }
        }

        if (growtime <= 0&&isgrounddetected())
        {
            GrowUp();
        }
    }

    private void GrowUp()
    {
        hasGrown = true;     
        sr.sprite = tree;      
        StartCoroutine(ColorTransitionEffect());

        ChangeSurroundingWindArea();
    }

    private IEnumerator ColorTransitionEffect()
    {
        float timer = 0f;


        Color startColor = new Color(2f, 2f, 2f, 0f); 
        Color endColor = Color.white;               

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            sr.color = Color.Lerp(startColor, endColor, progress);

            yield return null; 
        }

        sr.color = endColor;
    }

    private void ChangeSurroundingWindArea()
    {
        if (triggerCollider == null) return;

        List<Collider2D> results = new List<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        int count = triggerCollider.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            windarea wd = results[i].GetComponent<windarea>();
            if (wd != null)
            {
                wd.direction = windarea.winddir.up;
            }
        }
    }
}