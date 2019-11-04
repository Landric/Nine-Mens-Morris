using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDolly : MonoBehaviour
{
    GameStateManager state;
    float pivotVelocity;

    // Use this for initialization
    void Start()
    {
        state = FindObjectOfType<GameStateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = this.transform.rotation.eulerAngles.y;
        if (angle > 180)
            angle -= 360f;

        angle = Mathf.SmoothDamp(
            angle,
            (state.currentPlayerID == 0 ? 0 : 180),
            ref pivotVelocity,
            0.25f);

        this.transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }
}