using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
  public float moveSpeed = 1f;
  public Rigidbody2D rigidBody;
  public Vector3 target;
  public float zombieDetectionDistance = 5f;
  public float projectileForce = 1f;
  public float timeBetweenProjectiles = 1.0f;
  public GameObject projectilePrefab;


  private bool shouldMove = true;
  private bool isMoving = false;
  private Vector3 currentTarget;
  private Vector3 futureTarget;
  private Coroutine moveCoroutine;
  private Coroutine zombieDetectionCoroutine;

  void Start()
  {
    currentTarget = target;
    futureTarget = transform.position;
  }

  void Update()
  {
    if (shouldMove && !isMoving)
    {
      isMoving = true;
      moveCoroutine = StartCoroutine(MoveToTarget());
    }

    if (zombieDetectionCoroutine == null)
    {
      RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, zombieDetectionDistance, LayerMask.GetMask("Default"));

      if (hit.collider != null && hit.collider.gameObject.tag.Equals("Player"))
      {
        Debug.Log("ZOMBIE!!! Ahhhhhh!!!");

        shouldMove = false;

        if (moveCoroutine != null)
        {
          StopCoroutine(moveCoroutine);
        }

        zombieDetectionCoroutine = StartCoroutine(HandleZombieDetection(hit.transform));
      }
    }
  }

  public Vector3 GetPosition()
  {
    return transform.position;
  }

  IEnumerator MoveToTarget()
  {
    transform.up = currentTarget - transform.position;

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

  IEnumerator HandleZombieDetection(Transform zombie)
  {
    while (true)
    {
      transform.up = zombie.position - transform.position;

      GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.up, Quaternion.identity);
      Rigidbody2D projectileRigidbody = projectile.GetComponent<Rigidbody2D>();
      projectileRigidbody.AddForce(transform.up * projectileForce, ForceMode2D.Impulse);

      yield return new WaitForSeconds(timeBetweenProjectiles);
      yield return new WaitForFixedUpdate();
    }
  }
}