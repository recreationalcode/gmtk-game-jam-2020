using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;
  public float minimumMoveSpeed = 1f;
  public float killMoveSpeed = 15f;
  public float coldBloodPerKill = 20.0f;

  public Rigidbody2D rigidBody;

  private ColdBloodManager coldBloodManager;
  private bool shouldMove = true;
  private Vector2 movement;

  private LinkedList<HumanController> targets;
  private HumanController currentTarget;

  private

  void Start()
  {
    coldBloodManager = GetComponent<ColdBloodManager>();
    targets = new LinkedList<HumanController>();
  }

  void Update()
  {
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    if (shouldMove && targets.Count > 0)
    {
      Debug.Log("Targets: " + targets.Count);
      shouldMove = false;

      currentTarget = targets.Last.Value;
      targets.RemoveLast();

      StartCoroutine(MoveToTarget(currentTarget.GetPosition()));
    }
  }

  void FixedUpdate()
  {
    if (shouldMove)
    {
      float finalMoveSpeed = Mathf.Max(minimumMoveSpeed, moveSpeed * coldBloodManager.GetColdBloodPercentage());
      rigidBody.MovePosition(rigidBody.position + (movement * finalMoveSpeed * Time.fixedDeltaTime));
    }
  }

  public void Kill(HumanController human)
  {
    if (currentTarget != human && !targets.Contains(human))
    {
      targets.AddLast(human);
    }
  }

  void OnCollisionEnter2D(Collision2D other)
  {
    HumanController human = other.gameObject.GetComponent<HumanController>();

    if (human != null)
    {
      if (currentTarget == human)
      {
        currentTarget = null;
      }

      targets.Remove(human);

      Destroy(human.gameObject);

      coldBloodManager.AddColdBlood(coldBloodPerKill);
    }
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