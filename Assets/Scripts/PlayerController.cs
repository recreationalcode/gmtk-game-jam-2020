using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;

public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;
  public float killMoveSpeed = 15f;
  public float timeBetweenQuips = 5f;
  public float timeUntilComboStops = 0.5f;
  public float timeDisabledAfterKill = 0.2f;
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
  public TextMeshProUGUI comboLabel;
  public TextMeshProUGUI highestComboLabel;
  public AudioManager audioManager;

  private CircleCollider2D circleCollider;
  private CinemachineImpulseSource cinemachineImpulseSource;
  private ColdBloodManager coldBloodManager;
  private bool shouldMove = true;
  private bool shouldTalk = true;
  private bool isGameStarted = false;
  private Vector2 movement;

  private HumanController currentTarget;
  private Coroutine attackCoroutine;
  private Coroutine comboTimerCoroutine;

  private int highestBodyCount = 0;
  private int humanBodyCount = 0;
  private int combo = 0;
  private int highestCombo = 0;

  void Start()
  {
    coldBloodManager = GetComponent<ColdBloodManager>();
    circleCollider = GetComponent<CircleCollider2D>();
    cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
  }

  void Update()
  {
    bodyCountLabel.SetText(string.Format("body count: {0:0}", humanBodyCount));
    comboLabel.SetText(string.Format("combo: {0:0}", combo));
    highestBodyCountLabel.SetText(string.Format("highest body count: {0:0}", highestBodyCount));
    highestComboLabel.SetText(string.Format("highest combo: {0:0}", highestCombo));

    if (!isGameStarted) return;

    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    if (shouldMove && currentTarget != null)
    {
      shouldMove = false;
      playerAnimator.SetBool("isMoving", false);

      attackCoroutine = StartCoroutine(Attack(currentTarget.GetPosition()));
    }
  }

  void FixedUpdate()
  {
    if (shouldMove && (movement.x != 0.0f || movement.y != 0.0f))
    {
      PlayMovingSound();

      playerAnimator.SetBool("isMoving", true);

      RotateTowards(transform.position + new Vector3(movement.x, movement.y, 0));

      rigidBody.MovePosition(rigidBody.position + (movement * moveSpeed * Time.fixedDeltaTime));
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
    highestComboLabel.enabled = false;

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
    highestComboLabel.enabled = true;
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
    if (currentTarget == null)
    {
      currentTarget = human;
    }
  }

  public void Forget(HumanController human)
  {
    if (currentTarget == human)
    {
      currentTarget = null;
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

      Destroy(human.gameObject);

      humanBodyCount++;

      if (comboTimerCoroutine != null)
      {
        StopCoroutine(comboTimerCoroutine);
      }

      comboTimerCoroutine = StartCoroutine(ComboTimer());

      combo++;

      if (combo > highestCombo)
      {
        highestCombo = combo;
      }

      Vector3 direction = (human.gameObject.transform.position - transform.position).normalized;

      Instantiate(humanBloodSpatterEffect, transform.position + (1.5f * direction * circleCollider.radius), Quaternion.FromToRotation(humanBloodSpatterEffect.transform.up, direction));

      coldBloodManager.AddColdBlood(coldBloodPerKill);

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

  bool CanAttack(Vector3 currentPosition, Vector3 target)
  {
    RaycastHit2D hit = Physics2D.Linecast(currentPosition, target, LayerMask.GetMask("Obstacles"));

    if (hit.collider == null)
    {

      Vector3 directionTowardsTarget = target - currentPosition;

      hit = Physics2D.CircleCast(
        currentPosition,
        circleCollider.radius,
        directionTowardsTarget.normalized,
        directionTowardsTarget.magnitude,
        LayerMask.GetMask("Obstacles"));

      return hit.collider == null;
    }

    return false;
  }

  IEnumerator Attack(Vector3 target)
  {
    Vector3 currentPosition = transform.position;

    if (CanAttack(currentPosition, target))
    {
      playerAnimator.SetBool("isAttacking", true);

      RotateTowards(target);

      float t = 0.0f;
      float timeToTarget = Vector3.Distance(currentPosition, target) / killMoveSpeed;

      PlayAttackingSound();

      while (t < timeToTarget && currentTarget != null)
      {
        t += Time.deltaTime;

        rigidBody.MovePosition(Vector3.Lerp(currentPosition, target, t / timeToTarget));
        yield return new WaitForFixedUpdate();
      }
      playerAnimator.SetBool("isAttacking", false);

      yield return new WaitForSeconds(timeDisabledAfterKill);
    }


    shouldMove = true;
  }

  IEnumerator StopTalking()
  {
    yield return new WaitForSeconds(timeBetweenQuips);

    shouldTalk = true;
  }

  IEnumerator ComboTimer()
  {
    yield return new WaitForSeconds(timeUntilComboStops);

    combo = 0;
  }
}