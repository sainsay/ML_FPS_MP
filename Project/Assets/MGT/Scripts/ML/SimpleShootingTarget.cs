using System;
using UnityEngine;

public class SimpleShootingTarget : MonoBehaviour
{
	public float health = 10.0f;
	public event Action<SimpleShootingTarget> OnDeath;

	// returns true if target is dead.
	public bool DoDamage(float damage)
	{
		health -= damage;

		if (health <= 0.0f)
		{
			OnDeath.Invoke(this);
			return true;
		}
		return false;
	}
}
