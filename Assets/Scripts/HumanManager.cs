using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class HumanManager : MonoBehaviour
{
  public int startMinHumans = 5;
  public int startMaxHumans = 20;
  public int levelMaxHumans = 40;
  public float timeBetweenNormalHumanSpawns = 5f;
  public float humanMaxMoveRadius = 15f;

  public float difficultyCurve = 0.1f;

  public Vector3 spawnAreaBottomLeft = new Vector3(-20f, -20f);
  public Vector3 spawnAreaTopRight = new Vector3(20f, 20f);
  public Tilemap obstacleTilemap;
  public AudioManager audioManager;
  public new Camera camera;

  private int minHumans;
  private int maxHumans;
  private bool shouldSpawn = false;

  public GameObject humanPrefab;
  private PlayerController playerController;

  void Start()
  {
    playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    obstacleTilemap = GameObject.Find("ObstacleTilemap").GetComponent<Tilemap>();

    minHumans = startMinHumans;
    maxHumans = startMaxHumans;

    StartCoroutine(SpawnHumans());
  }

  public void StartSpawning()
  {
    shouldSpawn = true;
  }

  public void StopSpawning()
  {
    shouldSpawn = false;

    foreach (Transform child in transform)
    {
      GameObject.Destroy(child.gameObject);
    }
  }

  void Update()
  {
    minHumans = Mathf.Min(levelMaxHumans, startMinHumans + Mathf.RoundToInt(playerController.GetBodyCount() * difficultyCurve));
    maxHumans = Mathf.Min(levelMaxHumans, startMaxHumans + Mathf.RoundToInt(playerController.GetBodyCount() * difficultyCurve));

    if (shouldSpawn && transform.childCount < minHumans)
    {
      SpawnHuman();
    }
  }

  Vector3 GetRandom2DPositionInArea(Vector3 bottomLeft, Vector3 topRight)
  {
    return new Vector3(
      Random.Range(bottomLeft.x, topRight.x),
      Random.Range(bottomLeft.y, topRight.y),
      0f);
  }

  bool IsSpawnPositionVisible(Vector3 spawnPosition)
  {
    Vector3 viewportPosition = camera.WorldToViewportPoint(spawnPosition);
    return viewportPosition.x >= 0 && viewportPosition.x < 1 && viewportPosition.y >= 0 && viewportPosition.y < 1;
  }


  bool IsSpawnPositionBlocked(Vector3 spawnPosition)
  {
    if (!obstacleTilemap.HasTile(obstacleTilemap.WorldToCell(spawnPosition)))
    {
      CircleCollider2D circleCollider = humanPrefab.GetComponent<CircleCollider2D>();

      Collider2D collider = Physics2D.OverlapCircle(
        spawnPosition,
        circleCollider.radius,
        LayerMask.GetMask("Obstacles"));

      return collider == null;
    };

    return true;
  }

  bool SpawnHuman()
  {
    Vector3 spawnPosition = GetRandom2DPositionInArea(spawnAreaBottomLeft, spawnAreaTopRight);

    if (IsSpawnPositionBlocked(spawnPosition) || IsSpawnPositionVisible(spawnPosition)) return false;

    Vector3 targetPosition = GetRandom2DPositionInArea(
      new Vector3(Mathf.Max(spawnPosition.x - humanMaxMoveRadius, spawnAreaBottomLeft.x), Mathf.Max(spawnPosition.y - humanMaxMoveRadius, spawnAreaBottomLeft.y)),
      new Vector3(Mathf.Min(spawnPosition.x + humanMaxMoveRadius, spawnAreaTopRight.x), Mathf.Max(spawnPosition.y + humanMaxMoveRadius, spawnAreaTopRight.y))
    );

    CircleCollider2D circleCollider = humanPrefab.GetComponent<CircleCollider2D>();

    Vector3 directionTowardsTarget = targetPosition - spawnPosition;

    RaycastHit2D hit = Physics2D.BoxCast(
      spawnPosition,
      new Vector2(circleCollider.radius * 2, circleCollider.radius * 2),
      Mathf.Atan2(directionTowardsTarget.y, directionTowardsTarget.x) * Mathf.Rad2Deg,
      directionTowardsTarget.normalized);

    if (hit.collider != null && hit.transform.gameObject.layer != LayerMask.NameToLayer("Obstacles"))
    {
      GameObject human = Instantiate(humanPrefab, spawnPosition, Quaternion.identity);

      human.transform.parent = gameObject.transform;

      HumanController humanController = human.GetComponent<HumanController>();

      humanController.SetTarget(targetPosition);
      humanController.SetAudioManager(audioManager);
      humanController.timeBetweenProjectiles = humanController.timeBetweenProjectiles - Mathf.RoundToInt(playerController.GetBodyCount() * difficultyCurve) / 100;

      return true;
    }
    else
    {
      return false;
    }
  }

  IEnumerator SpawnHumans()
  {
    while (true)
    {
      if (shouldSpawn && transform.childCount < maxHumans)
      {
        while (!SpawnHuman()) { };
      }

      yield return new WaitForSeconds(timeBetweenNormalHumanSpawns);
    }
  }
}