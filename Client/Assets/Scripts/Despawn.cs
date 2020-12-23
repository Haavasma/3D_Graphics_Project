using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Despawn : MonoBehaviour
{
    [SerializeField] float secondsToDespawn = 3.0f;

    [SerializeField] float fadeOutDuration = 1.0f;

    private bool fadeOut = false;

    Text text;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DespawnAfterSec(3.0f));
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(fadeOut){
            text.CrossFadeAlpha(0, fadeOutDuration*0.5f, false);
        }
    }

    IEnumerator DespawnAfterSec(float seconds){
        yield return new WaitForSeconds(seconds);
        fadeOut = true;
        yield return new WaitForSeconds(fadeOutDuration*2.0f);
        Destroy(gameObject);
    }
}
