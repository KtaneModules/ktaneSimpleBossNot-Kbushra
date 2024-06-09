using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBossModule BossModule;
	public KMBombModule module;
	public KMSelectable[] button;
	public TextMesh screen;
	private string[] ignoredModules;

	public int stageCur;
	public int StagesTotes;
	public int stageRand;
	public int StageRandNum;
	public int stageGoal;
	public int timeCheck;

	public bool _isSolved = false;

	public AudioSource correct;

	static int ModuleIdCounter;
	int ModuleId;

	void Awake()
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in button)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { buttonPress(pressedButton); return false; };
		}
	}

	void Start()
	{

		if (ignoredModules == null) 
		{
			ignoredModules = BossModule.GetIgnoredModules ("Remember Simple", new string[] 
			{
					"14",
					"Cruel Purgatory",
					"Forget Enigma",
					"Forget Everything",
					"Forget It Not",
					"Forget Infinity",
					"Forget Me Later",
					"Forget Me Not",
					"Forget Perspective",
					"Forget Them All",
					"Forget This",
					"Forget Us Not",
					"Organization",
					"Purgatory",
					"Simon's Stages",
					"Souvenir",
					"Tallordered Keys",
					"The Time Keeper",
					"Timing is Everything",
					"The Troll",
					"Turn The Key",
					"Übermodule",
					"Ültimate Custom Night",
					"The Very Annoying Button",
					"Remember Simple",
					"Remembern't Simple"
			});

			module.OnActivate += delegate () 
			{ 
				StagesTotes = info.GetSolvableModuleNames ().Where (a => !ignoredModules.Contains (a)).ToList ().Count;
				if (StagesTotes > 0) 
				{
					Log ("Yes Stages");
				} 
				else 
				{
					Log ("No Stages");
					module.HandlePass ();
				}
			};
		}
		Invoke ("RandomAndStages", 1);
	}

	void  RandomAndStages()
	{
		StagesTotes = info.GetSolvableModuleNames ().Where (a => !ignoredModules.Contains (a)).ToList ().Count;

		stageRand = Random.Range (1, StagesTotes);
		StageRandNum = Random.Range (1, 100);

		Debug.LogFormat ("Random stage will be {0} and total stages will be {1}", stageRand, StagesTotes);
	}

	void FixedUpdate()
	{
		stageCur = info.GetSolvedModuleNames ().Where (a => !ignoredModules.Contains (a)).ToList ().Count;

		stageGoal = (StagesTotes + StageRandNum) % 10;
		timeCheck = (int) info.GetTime () % 10;

		if (stageRand == stageCur) 
		{
			screen.text = StageRandNum.ToString ();
		}
		else 
		{
			screen.text = "";
		}
	}

	public void buttonPress(KMSelectable pressedButton)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);
		int buttonPosition = new int();
		for(int i = 0; i < button.Length; i++)
		{
			if (pressedButton == button[i])
			{
				buttonPosition = i;
				break;
			}
		}

		if (_isSolved == false) 
		{
			switch (buttonPosition) 
			{
			case 0:
				if (stageCur == StagesTotes)
				{
					if (stageGoal == timeCheck)
					{
						Log ("Boss defeated..");
						module.HandlePass ();
					}
					else
					{
						Log ("You weren't supposed to press the button with this time.");
						module.HandleStrike ();
					}
				}
				else 
				{
					Log ("The modules haven't been solved yet...");
					module.HandleStrike ();
				}
				break;
			}
		}
	}



	void Log(string message)
	{
		Debug.LogFormat("[Remembern't Simple #{0}] {1}", ModuleId, message);
	}

	#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} press # [presses button at specified time].";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
        string[] parameters = command.Split(' ');

        if (parameters[0] == "press") 
		{
			if (parameters.Length < 2) { yield return "sendtochaterror Too little parameters!"; yield break; }
			else if (parameters.Length > 2) { yield return "sendtochaterror Too many parameters!"; yield break; }
			else
			{
				int number;

				if (!int.TryParse(parameters[1], out number))
				{ yield return "sendtochaterror 2nd parameter not a number!"; yield break; }
				else
				{
					while (timeCheck != int.Parse(parameters[1])) yield return "trycancel";
					button[0].OnInteract();
					yield break;
				}
			}
		}
		else { yield return "sendtochaterror Not a viable command!"; yield break; }
	}
	IEnumerator TwitchHandleForcedSolve() 
	{
		while (stageGoal != timeCheck || stageCur != StagesTotes) yield return true;
		button[0].OnInteract();
		yield break;
	}
}
