using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tree : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D bc;
    [SerializeField] private float timer = 5f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float targetAngle = 90f;
    [SerializeField] private Animator anim;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        bc.enabled = false;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0 && !isFalling)
        {
            FallDown();
        }
    }

    public void FallDown()
    {
        isFalling = true;
        anim.SetBool("Fall", isFalling);
        StartCoroutine(FallDownCoroutine());
    }

    private IEnumerator FallDownCoroutine()
    {
        float rotated = 0f;

        while (rotated < targetAngle)
        {
            float rotateAmount = rotateSpeed * Time.deltaTime;
            rotateAmount = Mathf.Min(rotateAmount, targetAngle - rotated);

            transform.Rotate(0, 0, -rotateAmount);
            rotated += rotateAmount;

            yield return null;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        bc.enabled = true;
        Debug.Log("Ê÷µ¹ÏÂÁË£¡");
    }
}