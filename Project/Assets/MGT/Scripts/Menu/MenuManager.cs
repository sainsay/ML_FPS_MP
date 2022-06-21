using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	private Menu[] menus;

	private void Awake()
	{
		menus = GetComponentsInChildren<Menu>(true);
	}

	private void Start()
	{
		DontDestroyOnLoad(this);
	}

	public Menu OpenMenu(string menuName)
	{
		foreach (var item in menus)
		{
			if (menuName == item.gameObject.name)
			{
				OpenMenu(item);
				return item;
			}
		}
		Debug.LogError("no menu called: " + menuName);
		return null;
	}

	public void OpenMenu(Menu menu)
	{
		foreach (var item in menus)
		{
			if (item.IsOpen)
			{
				item.Close();
			}
		}
		menu.Open();
	}

	public void CloseMenu(string menuName)
	{
		foreach (var item in menus)
		{
			if (menuName == item.gameObject.name)
			{
				CloseMenu(item);
			}
		}
	}

	public void CloseMenu(Menu menu)
	{
		menu.Close();
	}

}
