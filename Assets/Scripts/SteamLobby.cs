#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif
#if !DISABLESTEAMWORKS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;

public class SteamLobby : MonoBehaviour {
	public static SteamLobby instance;

	protected Callback<LobbyCreated_t> LobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> JoinRequest;
	protected Callback<LobbyEnter_t> LobbyEntered;

	public ulong CurrentLobbyID;
	private const string HostAddressKey = "HostAddress";
	private CustomNetworkManager manager;

	public GameObject HostButton;
	public Text LobbyNameText;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
	}
	private void Start() {
		if (!SteamManager.Initialized) { return; }

		manager = GetComponent<CustomNetworkManager>();

		LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
		LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
	}

	public void HostLobby() {
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,
			manager.maxConnections);

	}

	private void OnLobbyCreated(LobbyCreated_t callback) {
		if (callback.m_eResult != EResult.k_EResultOK) { return; };

		Debug.Log("Lobby created successfully");

		manager.StartHost();

		SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
			HostAddressKey, SteamUser.GetSteamID().ToString());
		SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
			"name", SteamFriends.GetPersonaName().ToString() + "'s lobby");
	}
	private void OnJoinRequest(GameLobbyJoinRequested_t callback) {
		Debug.Log("Request to join lobby");
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}
	private void OnLobbyEntered(LobbyEnter_t callback) {
		//everyone
		HostButton.SetActive(false);
		CurrentLobbyID = callback.m_ulSteamIDLobby;
		LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(
			callback.m_ulSteamIDLobby), "name");

		//clients
		if (NetworkServer.active) { return; }
		manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(
			callback.m_ulSteamIDLobby), HostAddressKey);
		manager.StartClient();
	}
}
#endif