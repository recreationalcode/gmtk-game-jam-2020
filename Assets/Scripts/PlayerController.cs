using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;
  public float minimumMoveSpeed = 1f;
  public float killMoveSpeed = 15f;
  public float timeDisabledAfterKill = 0.5f;
  public float coldBloodPerKill = 20.0f;
  public float coldBloodPerProjectile = 2.0f;

  public Rigidbody2D rigidBody;
  public Text bodyCountLabel;

  private ColdBloodManager coldBloodManager;
  private bool shouldMove = true;
  private Vector2 movement;

  private LinkedList<HumanController> targets;
  private HumanController currentTarget;
  private Coroutine attackCoroutine;

  private int humanBodyCount = 0;

  void Start()
  {
    coldBloodManager = GetComponent<ColdBloodManager>();
    targets = new LinkedList<HumanController>();
  }

  void Update()
  {
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    bodyCountLabel.text = string.Format("{0:0} body count", humanBodyCount);

    if (shouldMove && targets.Count > 0)
    {
      shouldMove = false;

      currentTarget = targets.Last.Value;
      targets.RemoveLast();

      if (currentTarget != null)
      {
        attackCoroutine = StartCoroutine(Attack(currentTarget.GetPosition()));
      }
      else
      {
        shouldMove = true;
      }
    }
  }

  void FixedUpdate()
  {
    if (shouldMove && (movement.x != 0.0f || movement.y != 0.0f))
    {
      RotateTowards(transform.position + new Vector3(movement.x, movement.y, 0));

      float finalMoveSpeed = Mathf.Max(minimumMoveSpeed, moveSpeed * coldBloodManager.GetColdBloodPercentage());
      rigidBody.MovePosition(rigidBody.position + (movement * finalMoveSpeed * Time.fixedDeltaTime));
    }
  }

  void RotateTowards(Vector3 target)
  {
    transform.up = target - transform.position;
  }

  public void Kill(HumanController human)
  {
    if (currentTarget != human && !targets.Contains(human))
    {
      targets.AddLast(human);
    }
  }

  public int GetBodyCount()
  {
    return humanBodyCount;
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

      humanBodyCount++;

      coldBloodManager.AddColdBlood(coldBloodPerKill);

      if (attackCoroutine != null)
      {
        StopCoroutine(attackCoroutine);
      }

      shouldMove = true;

      return;
    }

    ProjectileController projectile = other.gameObject.GetComponent<ProjectileController>();

    if (projectile != null)
    {
      coldBloodManager.RemoveColdBlood(coldBloodPerProjectile);
    }
  }

  IEnumerator Attack(Vector3 target)
  {
    Vector3 currentPosition = transform.position;

    RotateTowards(target);

    float t = 0.0f;
    float timeToTarget = Vector3.Distance(currentPosition, target) / killMoveSpeed;

    while (t < timeToTarget)
    {
      t += Time.deltaTime;

      rigidBody.MovePosition(Vector3.Lerp(currentPosition, target, t / timeToTarget));
      yield return new WaitForFixedUpdate();
    }

    yield return new WaitForSeconds(timeDisabledAfterKill);

    shouldMove = true;
  }
}