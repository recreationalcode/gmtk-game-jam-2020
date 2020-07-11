using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;
  public float killMoveSpeed = 15f;

  public Rigidbody2D rigidBody;

  private bool shouldMove = true;
  private Vector2 movement;

  void Update()
  {
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");
  }

  void FixedUpdate()
  {
    if (shouldMove)
    {
      rigidBody.MovePosition(rigidBody.position + (movement * moveSpeed * Time.fixedDeltaTime));
    }
  }

  public void Kill(HumanController human)
  {
    shouldMove = false;

    StartCoroutine(MoveToTarget(human.gameObject.transform.position));
  }

  void OnCollisionEnter2D(Collision2D other)
  {
    Destroy(other.gameObject);
  }

  IEnumerator MoveToTarget(Vector3 target)
  {
    Vector3 currentPosition = transform.position;

    float t = 0.0f;
    float timeToTarget = Vector3.Distance(currentPosition, target) / killMoveSpeed;

    while (t < timeToTarget)
    {
      t += Time.deltaTime;

      rigidBody.MovePosition(Vector3.Lerp(currentPosition, target, t / timeToTarget));
      yield return new WaitForFixedUpdate();
    }

    shouldMove = true;
  }
}