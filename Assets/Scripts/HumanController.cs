using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
  public float moveSpeed = 1f;
  public Rigidbody2D rigidBody;
  public Vector3 target;
  public float zombieDetectionDistance = 5f;
  public float zombieDetectionMaxAngle = 5f;
  public float projectileForce = 1f;
  public float timeBetweenProjectiles = 1.0f;
  public GameObject projectilePrefab;
  public AudioManager audioManager;
  public string[] attackingSounds;


  private bool shouldMove = true;
  private bool isMoving = false;
  private Vector3 currentTarget;
  private Vector3 futureTarget;
  private Coroutine moveCoroutine;
  private Coroutine zombieDetectionCoroutine;
  private Transform zombie;

  void Start()
  {
    futureTarget = transform.position;
  }

  void Update()
  {
    if (shouldMove && !isMoving && currentTarget != null)
    {
      if (moveCoroutine != null)
      {
        StopCoroutine(moveCoroutine);
      }

      moveCoroutine = StartCoroutine(MoveToTarget());
    }

    if (zombie != null && !CanSeeZombie(zombie))
    {
      HandleZombieUndetection();
    }
  }

  bool CanSeeZombie(Transform zombie)
  {
    RaycastHit2D hit = Physics2D.Linecast(
        transform.position,
        zombie.position,
        LayerMask.GetMask("Default", "Obstacles", "Zombies"));

    return hit.collider != null && hit.collider.gameObject.tag.Equals("Player");
  }

  public void HandleZombieDetection(Transform zombie)
  {
    if (!this.zombie && CanSeeZombie(zombie))
    {
      this.zombie = zombie;
      shouldMove = false;
      isMoving = false;

      if (moveCoroutine != null)
      {
        StopCoroutine(moveCoroutine);
      }

      PlayAttackingSound();

      zombieDetectionCoroutine = StartCoroutine(ShootZombie(zombie));
    }
  }


  public void HandleZombieUndetection()
  {
    if (zombie)
    {

      if (zombieDetectionCoroutine != null)
      {
        StopCoroutine(zombieDetectionCoroutine);
      }

      PlayUndetectionSound();

      moveCoroutine = StartCoroutine(MoveToTarget());

      shouldMove = true;
      zombie = null;
    }
  }

  public void SetTarget(Vector3 target)
  {
    this.target = target;
    this.currentTarget = target;
  }

  public void SetAudioManager(AudioManager audioManager)
  {
    this.audioManager = audioManager;
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

    audioManager.Play(attackingSounds[Random.Range(0, attackingSounds.Length)]);
  }

  void PlayUndetectionSound()
  {

  }

  void OnDestroy()
  {
    foreach (string sound in attackingSounds)
    {
      audioManager.Stop(sound);
    }
  }

  public Vector3 GetPosition()
  {
    return transform.position;
  }

  IEnumerator MoveToTarget()
  {
    isMoving = true;

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

  IEnumerator ShootZombie(Transform zombie)
  {
    while (true)
    {
      transform.up = zombie.position - transform.position;

      GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.up, transform.rotation);
      Rigidbody2D projectileRigidbody = projectile.GetComponent<Rigidbody2D>();
      projectileRigidbody.AddForce(transform.up * projectileForce, ForceMode2D.Impulse);

      yield return new WaitForSeconds(timeBetweenProjectiles);
      yield return new WaitForFixedUpdate();
    }
  }
}