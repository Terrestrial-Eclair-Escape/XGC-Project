using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageGoal : MonoBehaviour
{
    bool inRangePlayer;
    bool inRangeObject;
    bool StageCleared;
    Omnipotent Omni;

    // Start is called before the first frame update
    void Start()
    {
        Omni = GameObject.Find(Constants.OmnipotentName).GetComponent<Omnipotent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inRangePlayer && inRangeObject && !StageCleared)
        {
            StartCoroutine(OnStageCleared());
        }
    }
    public IEnumerator OnStageCleared()
    {
        Omni?.SetVictoryState();
        StageCleared = true;
        yield return new WaitForSecondsRealtime(2);
        Omni?.LoadNextScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(Constants.Tags.Player.ToString()))
        {
            inRangePlayer = true;

            if (other.TryGetComponent<BaseCharacterMovement>(out BaseCharacterMovement bcm))
            {
                if (bcm.pickedUpObject != null && bcm.pickedUpObject.CompareTag(Constants.Tags.MainObjective.ToString()))
                {
                    inRangeObject = true;
                }
            }
        }

        if (other.tag.Equals(Constants.Tags.MainObjective.ToString()))
        {
            inRangeObject = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(Constants.Tags.Player.ToString()))
        {
            if (other.TryGetComponent<BaseCharacterMovement>(out BaseCharacterMovement bcm))
            {
                if (bcm.pickedUpObject != null && bcm.pickedUpObject.CompareTag(Constants.Tags.MainObjective.ToString()))
                {
                    inRangeObject = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.Tags.Player.ToString()))
        {
            inRangePlayer = false;
        }

        if (other.CompareTag(Constants.Tags.MainObjective.ToString()))
        {
            inRangeObject = false;
        }
    }
}
