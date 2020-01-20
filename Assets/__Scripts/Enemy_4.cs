using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Part {
	// These three fields need to be defined in the Inspector pane
	public string name; // The name of this part
	public float health; // The amount of health thispart has
	public string[] protectedBy; // The other parts thatprotect this
	// These two fields are set automatically in Start().
	// Caching like this makes it faster and easier to find theselater
	[HideInInspector] // Makes field on the next line not appearin the Inspector
	public GameObject go; // The GameObject of thispart
	[HideInInspector]
	public Material mat; // The Material to showdamage
}

/// <summary>
/// Enemy_4 will start offscreen and then pick a random point oscreen to
/// move to. Once it has arrived, it will pick another randompoint and
/// continue until the player has shot it down.
/// </summary>
public class Enemy_4 : Enemy {
	[Header("Set in Inspector:Enemy_4")] // a
	public Part [] parts;
	private Vector3 p0, p1; // The two points tointerpolate
	private float timeStart; // Birth time for thisEnemy_4
	private float duration = 4; // Duration of movement
	void Start () {
		// There is already an initial position chosen byMain.SpawnEnemy()
		// so add it to points as the initial p0 & p1
		p0 = p1 =	pos; 

		InitMovement();
		Transform t;
		foreach (Part prt in parts) {
			t = transform.Find(prt.name);
			if (t != null) {
				prt.go = t.gameObject;
				prt.mat = prt.go.GetComponent<Renderer>().material;
			}
		}
	}
	void InitMovement() {
		// b
		p0 = p1; // Set p0 to the old p1
		// Assign a new on-screen location to p1
		float widMinRad = bndCheck.camWidth - bndCheck.radius;
		float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
		p1.x = Random.Range( -widMinRad, widMinRad );
		p1.y = Random.Range( -hgtMinRad, hgtMinRad );
		// Reset the time
		timeStart = Time.time;
	}
	public override void Move () {
		// c
		// This completely overrides Enemy.Move() with a linearinterpolation
		float u = (Time.time-timeStart)/duration;
		if (u>=1) {
			InitMovement();
			u=0;
		}
		u = 1 - Mathf.Pow( 1-u, 2 ); // Apply Ease Out easingto u // d
		pos = (1-u)*p0 + u*p1; // Simple linearinterpolation // e
	}

	// These two functions find a Part in parts based on name orGameObject
	Part FindPart(string n)
	{ // a
		foreach( Part prt in parts ) {
			if (prt.name == n) {
				return( prt );
			}
		}
		return( null );
	}
	Part FindPart(GameObject go)
	{ // b
		foreach( Part prt in parts ) {
			if (prt.go == go) {
				return( prt );
			}
		}
		return( null );
	}
	// These functions return true if the Part has been destroyed
	bool Destroyed(GameObject go)
	{ // c
		return( Destroyed( FindPart(go) ) );
	}
	bool Destroyed(string n) {
		return( Destroyed( FindPart(n) ) );
	}
	bool Destroyed(Part prt) {
		if (prt == null) { // If no real ph was passed in
			return(true); // Return true (meaning yes, it wasdestroyed)
		}
		// Returns the result of the comparison: prt.health <= 0
		// If prt.health is 0 or less, returns true (yes, it wasdestroyed)
		return (prt.health <= 0);
	}
	// This changes the color of just one Part to red instead othe whole ship.
	void ShowLocalizedDamage(Material m)
	{ // d
		m.color = Color.red;
		damageDoneTime = Time.time + showDamageDuration;
		showingDamage = true;
	}
	// This will override the OnCollisionEnter that is part ofEnemy.cs.
	void OnCollisionEnter( Collision coll ) {
		// e
		GameObject other = coll.gameObject;
		switch (other.tag) {
		case "ProjectileHero":
			Projectile p = other.GetComponent<Projectile>();
			// If this Enemy is off screen, don't damage it.
			if ( !bndCheck.isOnScreen ) {
				Destroy( other );
				break;
			}
			// Hurt this Enemy
			GameObject goHit = coll.contacts[0].thisCollider.gameObject; // f
			Part prtHit = FindPart(goHit);
			if (prtHit == null) { // If prtHit wasn'tfound… // g
				goHit = coll.contacts[0].otherCollider.gameObject;
				prtHit = FindPart(goHit);
			}
			// Check whether this part is still protected
			if (prtHit.protectedBy != null)
			{ // h
				foreach( string s in prtHit.protectedBy ) {
					// If one of the protecting parts hasn'tbeen destroyed...
					if (!Destroyed(s)) {
						// ...then don't damage this part yet
						Destroy(other); // Destroy theProjectileHero
						return; // return beforedamaging Enemy_4
					}
				}
			}
			// It's not protected, so make it take damage
			// Get the damage amount from the Projectile.type	and Main.W_DEFS $$$$$$$$$$$$$$$$
			prtHit.health -= Main.GetWeaponDefiniton( p.type).damageOnHit;
			// Show damage on the part
			ShowLocalizedDamage(prtHit.mat);
			if (prtHit.health <= 0)
			{ // i
				// Instead of destroying this enemy, disablethe damaged part
				prtHit.go.SetActive(false);
			}
			// Check to see if the whole ship is destroyed
			bool allDestroyed = true; // Assume it is	destroyed
			foreach( Part prt in parts ) {
				if (!Destroyed(prt)) { // If a part stillexists...
					allDestroyed = false; // ...changeallDestroyed to false
					break; // & break out ofthe foreach loop
					}
			}
			if (allDestroyed) { // If it IS completelydestroyed... // j
				// ...tell the Main singleton that this shipwas destroyed
				Main.S.shipDestroyed( this );
				// Destroy this Enemy
				Destroy(this.gameObject);
			}
			Destroy(other); // Destroy the ProjectileHero
			break;
		}
	}

}