using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an enum of the various possible weapon types.
/// It also includes a "shield" type to allow a shield power-up.
/// Items marked [NI] below are Not Implemented in the IGDPD book.
/// </summary>
public enum WeaponType {
	none, // The default / no weapon
	blaster, // A simple blaster
	spread, // Two shots simultaneously
	phaser, // [NI] Shots that move in waves
	missile, // [NI] Homing missiles
	laser, // [NI]Damage over time
	shield // Raise shieldLevel
}

/// <summary>
/// The WeaponDefinition class allows you to set the properties
/// of a specific weapon in the Inspector. The Main class has
/// an array of WeaponDefinitions that makes this possible.
/// </summary>
[System.Serializable]
// a
public class WeaponDefinition{ //b
	public WeaponType type = WeaponType.none;
	public string letter; // Letter to showon the power-up
	public Color color = Color.white; // Color ofCollar & power-up
	public GameObject projectilePrefab; // Prefab forprojectiles
	public Color projectileColor = Color.white;
	public float damageOnHit = 0; // Amount ofdamage caused
	public float continuousDamage = 0; // Damage persecond (Laser)
	public float delayBetweenShots = 0;
	public float velocity = 20; // Speed ofprojectiles
}

public class Weapon : MonoBehaviour {
	
	static public Transform PROJECTILE_ANCHOR;
	[Header("Set Dynamically")] [SerializeField]
	private WeaponType _type = WeaponType.none;
	public WeaponDefinition def;
	public GameObject collar;
	public float lastShotTime; // Time last shot wasfired
	private Renderer collarRend;
	void Start() {
		collar = transform.Find("Collar").gameObject;
		collarRend = collar.GetComponent<Renderer>();
		// Call SetType() for the default _type of WeaponType.none
		SetType( _type); // a
		// Dynamically create an anchor for all Projectiles
		if (PROJECTILE_ANCHOR== null)
		{ // b
			GameObject go = new GameObject("_ProjectileAnchor");
			PROJECTILE_ANCHOR = go.transform;
		}
		// Find the fireDelegate of the root GameObject
		GameObject rootGO = transform.root.gameObject; // c
		if ( rootGO.GetComponent<Hero>() != null )
		{ // d
			rootGO.GetComponent<Hero>().fireDelegate += Fire;
		}
	}
	public WeaponType type {
		get { return( _type ); }
		set { SetType( value ); }
	}
	public void SetType( WeaponType wt ) {
		_type = wt;
		if (type == WeaponType.none)
		{ // e
			this.gameObject.SetActive(false);
			return;
		} else {
			this.gameObject.SetActive(true);
		}
		def = Main.GetWeaponDefiniton(_type); //f
		collarRend.material.color = def.color;
		lastShotTime = 0; // You can fire immediately after _typeis set. // g
	}
	public void Fire() {
		// If this.gameObject is inactive, return
		if (!gameObject.activeInHierarchy) return;
		// h
		// If it hasn't been enough time between shots, return
		if (Time.time - lastShotTime < def.delayBetweenShots)
		{ // i
			return;
		}
		Projectile p;
		Vector3 vel = Vector3.up *
			def.velocity; // j
		if (transform.up.y < 0) {
			vel.y = -vel.y;
		}
		switch (type)
		{ // k
		case WeaponType.blaster:
			p = MakeProjectile();
			p.rigid.velocity = vel;
			break;
			case
			WeaponType.spread: // l
			p = MakeProjectile(); // Make middleProjectile
			p.rigid.velocity = vel;
			p = MakeProjectile(); // Make right Projectile
			p.transform.rotation = Quaternion.AngleAxis( 10,Vector3.back );
			p.rigid.velocity = p.transform.rotation * vel;
			p = MakeProjectile(); // Make left Projectile
			p.transform.rotation = Quaternion.AngleAxis(-10,Vector3.back );
			p.rigid.velocity = p.transform.rotation * vel;
			break;
		}
	}
	public Projectile MakeProjectile()
	{ // m
		GameObject go = Instantiate<GameObject>(
			def.projectilePrefab );
		if ( transform.parent.gameObject.tag == "Hero" )
		{ // n
			go.tag = "ProjectileHero";
			go.layer = LayerMask.NameToLayer("ProjectileHero");
		} else {
			go.tag = "ProjectileEnemy";
			go.layer = LayerMask.NameToLayer("ProjectileEnemy");
		}
		go.transform.position = collar.transform.position;
		go.transform.SetParent( PROJECTILE_ANCHOR, true
		); // o
		Projectile p = go.GetComponent<Projectile>();
		p.type = type;
		lastShotTime =	Time.time; // p
		return( p );
	}

}
