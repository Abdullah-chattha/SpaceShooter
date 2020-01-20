using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	private BoundsCheck bndCheck;
	private Renderer rend;

	[Header("Set Dynamically")]
	public Rigidbody rigid;
	[SerializeField]

	private WeaponType _type; //

	public WeaponType type {

		get {
			return( _type );
		}
		set {
			SetType ( value ); // c
		}
	}

	void Awake () {
		bndCheck = GetComponent<BoundsCheck>();
		rend = GetComponent<Renderer>(); // d
		rigid = GetComponent<Rigidbody>();
	}

	
	// Update is called once per frame
	void Update () {
		if (bndCheck.offUp) { // a
			Destroy( gameObject );
		}
		
	}

	public void SetType( WeaponType eType ) { 

		_type = eType;
		WeaponDefinition def = Main.GetWeaponDefiniton ( _type );
		rend.material.color = def.projectileColor;
	}
}