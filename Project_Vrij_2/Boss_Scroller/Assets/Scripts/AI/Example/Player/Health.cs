using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {

	public int startHealth;
	public int healthPerHeart;
	
	private int maxHealth;
	private int currentHealth;
	
	// Spacing:
	public float maxHeartsOnRow;
	private float spacingX;
	private float spacingY;
	
	
	void Start () {
		startHealth = 30;
		AddHearts(startHealth/healthPerHeart);
	}

	void Update(){
		int playerHealth = GameObject.Find ("Player").GetComponent<PlayerMovement> ().health;
		if (currentHealth != playerHealth) {
			modifyHealth (playerHealth - currentHealth);
		}
	}

	public void AddHearts(int n) {
		maxHealth += n * healthPerHeart;
		currentHealth = maxHealth;
	}

	
	public void modifyHealth(int amount) {
		currentHealth += amount;
		currentHealth = Mathf.Clamp(currentHealth,0,maxHealth);
	}
}
