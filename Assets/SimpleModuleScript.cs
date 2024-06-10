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

    //twitch plays
	#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <#> [Presses the button when the last digit of the bomb's timer is '#']";
	#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length > 2)
				yield return "sendtochaterror Too many parameters!";
			else if (parameters.Length == 2)
			{
				int temp = -1;
				if (!int.TryParse(parameters[1], out temp))
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is invalid!";
					yield break;
				}
				if (temp < 0 || temp > 9)
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is out of range 0-9!";
					yield break;
				}
				yield return null;
				while (temp != timeCheck) yield return "trycancel Halted waiting to press the button due to a cancel request!";
				button[0].OnInteract();
			}
			else yield return "sendtochaterror Too little parameters!";
		}
		else yield return "sendtochaterror Parameter not valid!";
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (StagesTotes == 0 || stageCur != StagesTotes) yield return true;
        while (timeCheck != stageGoal) yield return true;
        button[0].OnInteract();
    }
}
