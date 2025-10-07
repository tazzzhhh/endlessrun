using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    public int value = 1;
    public float moveSpeed = 5f;

    private void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.AddDiamond(value); // Tăng diamond
            GameManager.Instance.RemoveDiamond(this.gameObject); // Gỡ khỏi danh sách (nếu có)
            Destroy(gameObject); // Xóa viên kim cương sau khi ăn
        }
    }
}
