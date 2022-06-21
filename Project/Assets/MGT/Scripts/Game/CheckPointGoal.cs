using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointGoal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
        CharacterAgent agent = other.gameObject.GetComponent<CharacterAgent>();
        if (agent != null)
		{
            agent.AddReward(1.0f);
            Destroy(transform.parent.gameObject);

		}
	}


}
