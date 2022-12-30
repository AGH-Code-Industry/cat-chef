using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerConteoller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform playerPosition;

    [SerializeField]
    private float zOffset = -10.0f;
    [SerializeField]
    private float yOffset = 5.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerPosition.position.x, playerPosition.position.y + yOffset, zOffset);
    }
}
