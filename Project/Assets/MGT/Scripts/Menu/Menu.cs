using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update

	public string MenuName { get { return this.gameObject.name; } }
	public bool IsOpen { get { return this.gameObject.activeSelf; } }

    public virtual void Open()
	{
		this.gameObject.SetActive(true);
	}

	public virtual void Close()
	{
		this.gameObject.SetActive(false);
	}

}
