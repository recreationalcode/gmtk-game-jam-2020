using UnityEngine;

public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;

  public Rigidbody2D rigidBody;

  Vector2 movement;

  void Update()
  {
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");
  }

  void FixedUpdate()
  {
    rigidBody.MovePosition(rigidBody.position + (movement * moveSpeed * Time.fixedDeltaTime));
  }
}