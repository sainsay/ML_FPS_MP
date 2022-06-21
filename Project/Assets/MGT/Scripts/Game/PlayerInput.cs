using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public CharacterController character;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (character != null)
		{
            character.LookHorizontal(Input.GetAxis("Mouse X"));
            character.LookVertical(Input.GetAxis("Mouse Y"));
            character.Move(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized * Time.deltaTime);
            if (Input.GetMouseButtonDown(0))
			{
                character.Shoot(false);
			}
        
        }
    }
}
