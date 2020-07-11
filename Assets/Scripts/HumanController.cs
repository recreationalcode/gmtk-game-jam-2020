using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
  public float moveSpeed = 1f;
  public bool shouldMove = true;
  public Rigidbody2D rigidBody;
  public Vector3 target;

  private bool isMoving = false;

  private Vector3 currentTarget;
  private Vector3 futureTarget;

  void Start()
  {
    currentTarget = target;
    futureTarget = transform.position;
  }

  void Update()
  {
    if (!isMoving)
    {
      isMoving = true;
      StartCoroutine(MoveToTarget());
    }
  }

  public Vector3 GetPosition()
  {
    return transform.position;
  }

  IEnumerator MoveToTarget()
  {
    float t = 0.0f;
    float timeToTarget = Vector3.Distance(transform.position, currentTarget) / moveSpeed;

    while (t < timeToTarget)
    {
      t += Time.deltaTime;
      rigidBody.MovePosition(Vector3.Lerp(futureTarget, currentTarget, t / timeToTarget));
      yield return new WaitForFixedUpdate();
    }

    Vector3 target = currentTarget;
    currentTarget = futureTarget;
    futureTarget = target;

    isMoving = false;
  }
}