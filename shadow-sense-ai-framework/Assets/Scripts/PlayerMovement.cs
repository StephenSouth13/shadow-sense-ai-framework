using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Đọc nút bấm WASD hoặc phím mũi tên từ bàn phím
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Tính toán hướng đi
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // Di chuyển nhân vật
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }
}