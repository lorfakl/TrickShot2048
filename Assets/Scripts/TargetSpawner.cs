using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab; // spawn from 0 to 36 Y
    public GameObject initalTarget;
    //public GameObject 

    public TMP_Text text;

    // Start is called before the first frame update

    private float totalScore = 0;
    private IEnumerator coroutine;

    private void Awake()
    {
    }

    void Start()
    {
        initalTarget.GetComponent<TargetScript>().TargetDestroyed.AddListener(ProcessTargetDestruction);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ProcessTargetDestruction(float score, GameObject target)
    {
        print("Processing Hit!");
        coroutine = CountDownToDestruction(1.5f, target);
        totalScore += score;
        text.text = "Score: " + totalScore;
        CreateNewTarget();

        StartCoroutine(coroutine);
    }

    void CreateNewTarget()
    {
        print("How many of these are making?");

        GameObject t = Instantiate(targetPrefab, new Vector3(Random.Range(-13, 15), Random.Range(0, 37)), Quaternion.identity);
        t.GetComponent<TargetScript>().TargetDestroyed.AddListener(ProcessTargetDestruction);
        t.transform.localScale = new Vector3(Random.Range(1, 7), Random.Range(1, 9), 1);
    }

    IEnumerator CountDownToDestruction(float s, GameObject target)
    {
        target.GetComponent<TargetScript>().TargetDestroyed.RemoveListener(ProcessTargetDestruction);

        yield return new WaitForSeconds(s);
        Destroy(target);
        

    }
}
