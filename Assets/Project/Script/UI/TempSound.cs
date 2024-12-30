using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSound : MonoBehaviour
{
    SoundController soundController;
    // Start is called before the first frame update
    private void Awake()
    {
        soundController = gameObject.GetComponent<SoundController>();
    }
    void Start()
    {
        soundController.PlayAudio("MainBGM", true);
    }
}
