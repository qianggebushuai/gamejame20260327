using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class icepiece : MonoBehaviour
{
    public float breaktime = 5f;
    public float Rotatespeed = 60f;
    private SpriteRenderer sr;
    private float initialBreaktime;

    void Start()
    {
        transform.localScale = Random.Range(1, 3) / 3f * new Vector3(1, 1, 1);
        sr = GetComponent<SpriteRenderer>();
        initialBreaktime = breaktime;
    }

    void Update()
    {
        breaktime -= Time.deltaTime;

        transform.Rotate(0, 0, Rotatespeed * Time.deltaTime);

        float alpha = Mathf.Clamp01(breaktime / initialBreaktime);
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

        if (breaktime < 0)
        {
            Destroy(gameObject);
        }
    }
}
