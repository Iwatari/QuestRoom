using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class test : MonoBehaviour
{
    public GameObject EnemyPrefab;

    private void Start()
    {
        StartCoroutine(routine: CoroutineSample());
    }

    private IEnumerator CoroutineSample()
    {
        GameObject enemy = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);

        yield return StartCoroutine(routine: Utils.MoveToTarget(enemy.transform, new Vector3(x: -2, y: 1, z: 0)));

        yield return new WaitUntil(predicate: () => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(routine: Utils.MoveToTarget(enemy.transform, new Vector3(x: 5, y: 3, z: 0)));

        yield return new WaitForSeconds(1);

        enemy.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
}
