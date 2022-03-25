using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
	public EnemyHealth bossHealth;
	public Slider slider;

	void Start()
	{
		slider.maxValue = bossHealth.GetMaxHealth();
	}

	// Update is called once per frame
	void Update()
	{
		slider.value = bossHealth.GetCurrentHealth();
		if (bossHealth.GetCurrentHealth() <= 0)
        {
			Destroy(gameObject);
        }
	}
}
