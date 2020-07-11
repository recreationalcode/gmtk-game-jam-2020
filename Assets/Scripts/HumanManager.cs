using UnityEngine;
using System.Collections;

public class HumanManager : MonoBehaviour
{
  public int startMinHumans = 5;
  public int startMaxHumans = 20;
  public float timeBetweenNormalHumanSpawns = 5f;
  public float humanMaxMoveRadius = 15f;

  public float difficultyCurve = 0.1f;

  public Vector3 spawnAreaBottomLeft = new Vector3(-20f, -20f);
  public Vector3 spawnAreaTopRight = new Vector3(20f, 20f);

  private int minHumans;
  private int maxHumans;

  public GameObject humanPrefab;
  private GameController gameController;
  private PlayerController playerController;

  void Start()
  {
    gameController = GameObject.Find("GameController").GetComponent<GameController>();
    playerController = GameObject.Find("Player").GetComponent<PlayerController>();

    minHumans = startMinHumans;
    maxHumans = startMaxHumans;

    StartCoroutine(SpawnHumans());
  }

  void Update()
  {
    if (transform.childCount < minHumans)
    {
      SpawnHuman();
    }

    minHumans = startMinHumans + Mathf.RoundToInt(playerController.GetBodyCount() * difficultyCurve);
    maxHumans = startMaxHumans + Mathf.RoundToInt(playerController.GetBodyCount() * difficultyCurve);
  }

  Vector3 GetRandom2DPositionInArea(Vector3 bottomLeft, Vector3 topRight)
  {
    return new Vector3(
      Random.Range(bottomLeft.x, topRight.x),
      Random.Range(bottomLeft.y, topRight.y),
      0f);
  }

  void SpawnHuman()
  {
    Vector3 spawnPosition = GetRandom2DPositionInArea(spawnAreaBottomLeft, spawnAreaTopRight);

    GameObject human = Instantiate(humanPrefab, spawnPosition, Quaternion.identity);

    human.transform.parent = gameObject.transform;

    HumanController humanController = human.GetComponent<HumanController>();

    humanController.SetTarget(GetRandom2DPositionInArea(
      new Vector3(Mathf.Max(spawnPosition.x - humanMaxMoveRadius, spawnAreaBottomLeft.x), Mathf.Max(spawnPosition.y - humanMaxMoveRadius, spawnAreaBottomLeft.y)),
      new Vector3(Mathf.Min(spawnPosition.x + humanMaxMoveRadius, spawnAreaTopRight.x), Mathf.Max(spawnPosition.y + humanMaxMoveRadius, spawnAreaTopRight.y))
    ));
  }

  IEnumerator SpawnHumans()
  {
    while (true)
    {
      if (transform.childCount < maxHumans)
      {
        SpawnHuman();
      }
      yield return new WaitForSeconds(timeBetweenNormalHumanSpawns);
    }
  }
}