using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MLAgents;
public class GameController : MonoBehaviour {

	// enum AppMode
	// {
	// 	Traning, Game
	// }
	// //////////////////////////////////////////////////////////////////////////
	// [Header("GENERAL")]
	// public AppMode appMode;
	Brain mlAgentsBrain;

	//////////////////////////////////////////////////////////////////////////
	[Header("GAME UI")]
	public GameObject titlePanel;
	public GameObject backButton;
	public GameObject stickTitleScreen;


	// public ThrowBone throwController;
	Throw throwController;


	//////////////////////////////////////////////////////////////////////////
	[Header("CAMERAS")]

	public CinemachineVirtualCamera cameraTraining;
	public CinemachineVirtualCamera cameraTitle;
	public CinemachineVirtualCamera cameraGame;
	public CinemachineBrain cmBrain;

	//////////////////////////////////////////////////////////////////////////
	[Header("Music & Sound Effects")]
	// public AudioSource audioSourceBackgroundMusic;
	public AudioSource audioSourceSFX;
	// public bool playBackgroundMusic = true;
	// public AudioClip backgroundMusic;
	public AudioClip buttonClickStartSFX;
	public AudioClip buttonClickEndSFX;

	////////////////////////////////////////////////////////////////////////////


	// void SetupAudio()
	// {
	// 	audioSourceBackgroundMusic = gameObject.AddComponent<AudioSource>();
	// 	audioSourceSFX = gameObject.AddComponent<AudioSource>();
	// 	audioSourceBackgroundMusic.loop = true;
	// 	audioSourceBackgroundMusic.volume = .5f;
	// }


	// public void PlayGameBackgroundAudio()
	// {
	// 	audioSourceBackgroundMusic.clip = backgroundMusic;
	// 	audioSourceBackgroundMusic.Play();
	// }

	void Awake () {
		audioSourceSFX = gameObject.AddComponent<AudioSource>();

		mlAgentsBrain = FindObjectOfType<Brain>();
		throwController = GetComponent<Throw>();
		cmBrain = FindObjectOfType<CinemachineBrain>();
		if(mlAgentsBrain.brainType == BrainType.External)//we are training
		{
			TrainingMode();
		}
		// else if(mlAgentsBrain.brainType == BrainType.Internal)//we are doing inference
		// {
		// 	playBackgroundMusic = true;
		// }
		
		// SetupAudio();
		// if (playBackgroundMusic)
		// {
		// 	PlayGameBackgroundAudio();
		// }
		
	}

	void TrainingMode()
	{
		// playBackgroundMusic = false;
		throwController.item.gameObject.SetActive(true);
		stickTitleScreen.SetActive(false);
		titlePanel.SetActive(false);
		backButton.SetActive(false);
		cameraTitle.Priority = 1;
		cameraGame.Priority = 1;
		cameraTraining.Priority = 2;
		throwController.enabled = false;
	}
	
	public void StartGame()
	{
		audioSourceSFX.PlayOneShot(buttonClickStartSFX, 1);

		titlePanel.SetActive(false);
		backButton.SetActive(true);
		cameraTitle.Priority = 1;
		cameraGame.Priority = 2;
		cameraTraining.Priority = 1;
		throwController.enabled = true;
		stickTitleScreen.SetActive(false);
		throwController.item.gameObject.SetActive(true);
	}

	public void EndGame()
	{
		audioSourceSFX.PlayOneShot(buttonClickEndSFX, 1);
		titlePanel.SetActive(true);
		backButton.SetActive(false);
		cameraTitle.Priority = 2;
		cameraGame.Priority = 1;
		cameraTraining.Priority = 1;
		throwController.item.gameObject.SetActive(false);
		throwController.dogAgent.target = throwController.returnPoint;
		throwController.enabled = false;
		stickTitleScreen.SetActive(true);

	}
}
