using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;

public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;
  public float minimumMoveSpeed = 1f;
  public float killMoveSpeed = 15f;
  public float timeBetweenQuips = 5f;
  public float timeDisabledAfterKill = 0.5f;
  public float coldBloodPerKill = 20.0f;
  public float coldBloodPerProjectile = 2.0f;

  public string[] idleSounds;
  public string[] movingSounds;
  public string[] attackingSounds;

  public Rigidbody2D rigidBody;
  public Animator playerAnimator;
  public GameObject zombieBloodSpatterEffect;
  public GameObject humanBloodSpatterEffect;
  public TextMeshProUGUI bodyCountLabel;
  public TextMeshProUGUI highestBodyCountLabel;
  public AudioManager audioManager;

  private CircleCollider2D circleCollider;
  private CinemachineImpulseSource cinemachineImpulseSource;
  private ColdBloodManager coldBloodManager;
  private bool shouldMove = true;
  private bool shouldTalk = true;
  private bool isGameStarted = false;
  private Vector2 movement;

  private LinkedList<HumanController> targets;
  private HumanController currentTarget;
  private Coroutine attackCoroutine;

  private int highestBodyCount = 0;
  private int humanBodyCount = 0;
  private int combo = 0;
  private int highestCombo = 0;

  void Start()
  {
    coldBloodManager = GetComponent<ColdBloodManager>();
    circleCollider = GetComponent<CircleCollider2D>();
    cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    targets = new LinkedList<HumanController>();
  }

  void Update()
  {
    bodyCountLabel.SetText(string.Format("body count: {0:0}", humanBodyCount));
    highestBodyCountLabel.SetText(string.Format("highest body count: {0:0}", highestBodyCount));

    if (!isGameStarted) return;

    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    if (shouldMove && targets.Count > 0)
    {
      currentTarget = targets.Last.Value;
      targets.RemoveLast();

      if (currentTarget != null)
      {

        shouldMove = false;
        playerAnimator.SetBool("isAttacking", true);
        playerAnimator.SetBool("isMoving", false);

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

      PlayMovingSound();

      playerAnimator.SetBool("isMoving", true);

      RotateTowards(transform.position + new Vector3(movement.x, movement.y, 0));

      float finalMoveSpeed = Mathf.Max(minimumMoveSpeed, moveSpeed * coldBloodManager.GetColdBloodPercentage());
      rigidBody.MovePosition(rigidBody.position + (movement * finalMoveSpeed * Time.fixedDeltaTime));
    }
    else
    {
      playerAnimator.SetBool("isMoving", false);

      PlayIdleSound();
    }
  }

  public void StartPlaying()
  {
    highestBodyCountLabel.enabled = false;

    humanBodyCount = 0;

    isGameStarted = true;
  }

  public void StopPlaying()
  {
    if (humanBodyCount > highestBodyCount) highestBodyCount = humanBodyCount;

    coldBloodManager.ResetColdBlood();

    if (attackCoroutine != null)
    {
      StopCoroutine(attackCoroutine);
    }
    shouldMove = true;
    isGameStarted = false;

    playerAnimator.SetBool("isMoving", false);
    playerAnimator.SetBool("isAttacking", false);

    highestBodyCountLabel.enabled = true;
  }

  void PlayIdleSound()
  {
    if (!shouldTalk) return;

    foreach (string sound in idleSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    foreach (string sound in movingSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    foreach (string sound in attackingSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    shouldTalk = false;
    audioManager.Play(idleSounds[Random.Range(0, idleSounds.Length)]);

    StartCoroutine(StopTalking());
  }

  void PlayMovingSound()
  {
    if (!shouldTalk) return;

    foreach (string sound in movingSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    foreach (string sound in idleSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    foreach (string sound in attackingSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    shouldTalk = false;
    audioManager.Play(movingSounds[Random.Range(0, movingSounds.Length)]);


    StartCoroutine(StopTalking());
  }

  void PlayAttackingSound()
  {
    foreach (string sound in attackingSounds)
    {
      if (audioManager.IsPlaying(sound))
      {
        return;
      }
    }

    foreach (string sound in idleSounds)
    {
      audioManager.Stop(sound);
    }

    foreach (string sound in movingSounds)
    {
      audioManager.Stop(sound);

    }

    audioManager.Play(attackingSounds[Random.Range(0, attackingSounds.Length)]);
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

      Vector3 direction = (human.gameObject.transform.position - transform.position).normalized;

      Instantiate(humanBloodSpatterEffect, transform.position + (1.5f * direction * circleCollider.radius), Quaternion.FromToRotation(humanBloodSpatterEffect.transform.up, direction));


      humanBodyCount++;

      coldBloodManager.AddColdBlood(coldBloodPerKill);

      if (attackCoroutine != null)
      {
        StopCoroutine(attackCoroutine);
      }

      playerAnimator.SetBool("isAttacking", false);

      shouldMove = true;

      return;
    }

    ProjectileController projectile = other.gameObject.GetComponent<ProjectileController>();

    if (projectile != null)
    {
      coldBloodManager.RemoveColdBlood(coldBloodPerProjectile);

      cinemachineImpulseSource.GenerateImpulse(-transform.forward);

      Vector3 direction = (projectile.gameObject.transform.position - transform.position).normalized;

      Instantiate(zombieBloodSpatterEffect, transform.position + (1.5f * direction * circleCollider.radius), Quaternion.FromToRotation(zombieBloodSpatterEffect.transform.up, direction));
    }
  }

  IEnumerator Attack(Vector3 target)
  {
    Vector3 currentPosition = transform.position;

    Vector3 directionTowardsTarget = target - transform.position;

    RaycastHit2D hit = Physics2D.BoxCast(
      currentPosition,
      new Vector2(circleCollider.radius * 2, circleCollider.radius * 2),
      Mathf.Atan2(directionTowardsTarget.y, directionTowardsTarget.x) * Mathf.Rad2Deg,
      directionTowardsTarget.normalized);

    if (hit.collider != null)
    {

      // Humans or Projectiles
      if (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 9)
      {

        RotateTowards(target);

        float t = 0.0f;
        float timeToTarget = Vector3.Distance(currentPosition, target) / killMoveSpeed;

        PlayAttackingSound();

        while (t < timeToTarget)
        {
          t += Time.deltaTime;

          rigidBody.MovePosition(Vector3.Lerp(currentPosition, target, t / timeToTarget));
          yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(timeDisabledAfterKill);
      }
    }

    playerAnimator.SetBool("isAttacking", false);

    shouldMove = true;
  }

  IEnumerator StopTalking()
  {
    yield return new WaitForSeconds(timeBetweenQuips);

    shouldTalk = true;
  }
}