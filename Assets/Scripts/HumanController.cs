using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
  public float moveSpeed = 20f;
  public float timeBetweenMovements = 1f;
  public float timeBetweenTurns = 5f;
  public Rigidbody2D rigidBody;
  public float projectileForce = 1f;
  public float timeBetweenProjectiles = 1.0f;
  public GameObject projectilePrefab;
  public AudioManager audioManager;
  public string[] attackingSounds;


  private bool shouldMove = true;
  private float timeSinceLastTurn = 0.0f;
  private Coroutine waitToMoveCoroutine;
  private Coroutine zombieDetectionCoroutine;
  private Transform zombie;

  void Start()
  {
    transform.up = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
  }

  void Update()
  {
    if (zombie != null && !CanSeeZombie(zombie))
    {
      HandleZombieUndetection();
    }

    if (timeSinceLastTurn >= timeBetweenTurns && !this.zombie)
    {
      timeSinceLastTurn = 0.0f;
      transform.up = Quaternion.Euler(0, 0, Random.Range(90f, 270f)) * transform.up;
    }
    else
    {
      timeSinceLastTurn += Time.deltaTime;
    }
  }

  void FixedUpdate()
  {
    if (shouldMove == true)
    {
      rigidBody.MovePosition(Vector3.Lerp(transform.position, transform.position + transform.up, Time.fixedDeltaTime * moveSpeed));
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

      if (waitToMoveCoroutine != null)
      {
        StopCoroutine(waitToMoveCoroutine);
      }

      shouldMove = false;

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

      shouldMove = true;
      zombie = null;
    }
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

  void OnCollisionEnter2D(Collision2D other)
  {
    shouldMove = false;

    timeSinceLastTurn = 0.0f;
    transform.up = Quaternion.Euler(0, 0, Random.Range(90f, 270f)) * transform.up;

    waitToMoveCoroutine = StartCoroutine(WaitToMove());
  }

  IEnumerator WaitToMove()
  {
    yield return new WaitForSeconds(timeBetweenMovements);

    shouldMove = true;
  }

  IEnumerator ShootZombie(Transform zombie)
  {
    transform.up = zombie.position - transform.position;

    while (true)
    {

      yield return new WaitForSeconds(timeBetweenProjectiles);
      yield return new WaitForFixedUpdate();

      transform.up = zombie.position - transform.position;

      GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.up, transform.rotation);
      Rigidbody2D projectileRigidbody = projectile.GetComponent<Rigidbody2D>();
      projectileRigidbody.AddForce(transform.up * projectileForce, ForceMode2D.Impulse);
    }
  }
}