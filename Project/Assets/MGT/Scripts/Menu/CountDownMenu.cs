using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDownMenu : Menu
{
	public TMP_Text CountDownTextField;
	public string PreValueText;
	public string PostValueText;

	public delegate float getTimeValue();
	public event getTimeValue getTimeEvent;

	private void Update()
	{
		if (IsOpen)
		{
			float value = getTimeEvent.Invoke();
			CountDownTextField.SetText(PreValueText + " " + value + " " + PostValueText);

		}
	}

	public override void Close()
	{
		base.Close();
		getTimeEvent = null;
	}
}
