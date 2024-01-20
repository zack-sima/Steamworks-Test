using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour {
	[SyncVar] public int money;

	private void Start() {
		if (isServer) {
			money = 100;
		}
	}
}
