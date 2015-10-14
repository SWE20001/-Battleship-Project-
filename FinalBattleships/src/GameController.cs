
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data;
using System.Diagnostics;
using SwinGameSDK;

/// <summary>
/// Game controller.
/// </summary>
public static class GameController
{
															/*The GameController is responsible for controlling the game,
															  managing user input, and displaying the current state of the game*/
	private static BattleShipsGame _theGame;
	private static Player _human;

	private static AIPlayer _ai;

	private static Stack<GameState> _state = new Stack<GameState>();

	private static AIOption _aiSetting;

	/// <summary>
	/// Gets the state of the current.
	/// </summary>
	/// <value>The state of the current.</value>
	public static GameState CurrentState {
		get { return _state.Peek(); }					 /* shall returns the current state of the game,
														    indicating which screen is currently being used*/
	}

	/// <summary>
	/// Gets the human player.
	/// </summary>
	/// <value>The human player.</value>
	public static Player HumanPlayer {
		get { return _human; }										/* it will return the human player,
																       that has value of human player as well*/
	}

	/// <summary>
	/// Gets the computer player.
	/// </summary>
	/// <value>The computer player.</value>
	public static Player ComputerPlayer {
		get { return _ai; }										/*will return the computer player with their value of computer player*/
	}

	/// <summary>
	/// Initializes the <see cref="GameController"/> class.
	/// </summary>
	static GameController()
	{
																  /*bottom state will be quitting.
																  If player exits main menu then the game is over*/
		_state.Push(GameState.Quitting);

																/* at the start the player is viewing the main menu*/
		_state.Push(GameState.ViewingMainMenu);
	}

	/// <summary>
	/// Starts the game.
	/// </summary>
	public static void StartGame()
	{														/*Creates an AI player based upon the _aiSetting*/
		if (_theGame != null)
			EndGame();

															/* it will start Creating the game from here after passing the condition*/
		_theGame = new BattleShipsGame();

															/* after creating the game will also create the players here*/
		switch (_aiSetting) {
			case AIOption.Medium:
				_ai = new AIMediumPlayer(_theGame);
				break;
			case AIOption.Hard:
			_ai = new AIHardPlayer(_theGame);			   /* checking all different of IA levels*/
				break;
			default:
				_ai = new AIEasyPlayer(_theGame);
				break;
		}

		_human = new Player(_theGame);

																/*AddHandler _human.PlayerGrid.Changed, AddressOf GridChanged*/
		_ai.PlayerGrid.Changed += GridChanged;
		_theGame.AttackCompleted += AttackCompleted;

		AddNewState(GameState.Deploying);
	}

	/// <summary>
	/// Ends the game.
	/// </summary>
	private static void EndGame()
														/*Stops listening to the old game once a new game is started*/
	{
														/*RemoveHandler _human.PlayerGrid.Changed, AddressOf GridChanged*/
		_ai.PlayerGrid.Changed -= GridChanged;
		_theGame.AttackCompleted -= AttackCompleted;
	}

	/// <summary>
	/// Grids the changed.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="args">Arguments.</param>
	private static void GridChanged(object sender, EventArgs args)
														/*Listens to the game grids for any changes and redraws the screen
													      when the grids change*/
	{
		DrawScreen();
		SwinGame.RefreshScreen();
	}

	/// <summary>
	/// Plaies the hit sequence.
	/// </summary>
	/// <param name="row">Row.</param>
	/// <param name="column">Column.</param>
	/// <param name="showAnimation">If set to <c>true</c> show animation.</param>
	private static void PlayHitSequence(int row, int column, bool showAnimation)
	{														/*it will show the animation and play sound effect when it is hitted*/
		if (showAnimation) {
			UtilityFunctions.AddExplosion(row, column);
		}

		Audio.PlaySoundEffect(GameResources.GameSound("Hit"));

		UtilityFunctions.DrawAnimationSequence();
	}

	/// <summary>
	/// Play Miss Sequence, show the animation and play sound effect when it is hitted 
	/// </summary>
	/// <param name="row">Row.</param>
	/// <param name="column">Column.</param>
	/// <param name="showAnimation">If set to <c>true</c> show animation.</param>
	private static void PlayMissSequence(int row, int column, bool showAnimation)
	{
		if (showAnimation) {
			UtilityFunctions.AddSplash(row, column);
		}

		Audio.PlaySoundEffect(GameResources.GameSound("Miss"));

		UtilityFunctions.DrawAnimationSequence();
	}

	/// <summary>
	/// Attacks the completed.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="result">Result.</param>

	private static void AttackCompleted(object sender, AttackResult result)
	{				
		bool isHuman = false;
		isHuman = object.ReferenceEquals(_theGame.Player, HumanPlayer);	      /* Displays a message, plays sound and redraws the screen,
																				also will show to player when attack has been attaked */
																					

		if (isHuman) {
			UtilityFunctions.Message = "You " + result.ToString();
		} else {
			UtilityFunctions.Message = "The AI " + result.ToString();
		}

		switch (result.Value) {
			case ResultOfAttack.Destroyed:
				PlayHitSequence(result.Row, result.Column, isHuman);
			Audio.PlaySoundEffect(GameResources.GameSound("Sink"));					/*if the player attacked ,the soundEffect will be played to let the player know */

				break;
			case ResultOfAttack.GameOver:
				PlayHitSequence(result.Row, result.Column, isHuman);
			Audio.PlaySoundEffect(GameResources.GameSound("Sink"));

			while (Audio.SoundEffectPlaying(GameResources.GameSound("Sink"))) {
					SwinGame.Delay(10);
					SwinGame.RefreshScreen();
				}

				if (HumanPlayer.IsDestroyed) {
				Audio.PlaySoundEffect(GameResources.GameSound("Lose"));
				} else {
				Audio.PlaySoundEffect(GameResources.GameSound("Winner"));
				}

				break;
			case ResultOfAttack.Hit:
				PlayHitSequence(result.Row, result.Column, isHuman);
				break;
			case ResultOfAttack.Miss:
				PlayMissSequence(result.Row, result.Column, isHuman);
				break;
			case ResultOfAttack.ShotAlready:
			Audio.PlaySoundEffect(GameResources.GameSound("Error"));
				break;
		}
	}

	/// <summary>
	/// Completes the deployment phase of the game and
	/// switches to the battle mode (Discovering state)
	/// </summary>
	/// <remarks>
	/// This adds the players to the game before switching
	/// state.
	/// </remarks>
	public static void EndDeployment()
	{
																			/*deploy the players*/
		_theGame.AddDeployedPlayer(_human);
		_theGame.AddDeployedPlayer(_ai);

		SwitchState(GameState.Discovering);
	}

	/// <summary>
	/// Attack the specified row and col.
	/// </summary>
	/// <param name="row">Row.</param>
	/// <param name="col">Col.</param>
	public static void Attack(int row, int col)
	{														/*Gets the player to attack the indicated row and column,
															and then Checks the attack result once the attack is complete*/
		AttackResult result = default(AttackResult);
		result = _theGame.Shoot(row, col);
		CheckAttackResult(result);
	}

	/// <summary>
	/// Gets the AI to attack.
	/// </summary>
	/// <remarks>
	/// Checks the attack result once the attack is complete.
	/// </remarks>
	private static void AIAttack()
	{
		AttackResult result = default(AttackResult);
		result = _theGame.Player.Attack();
		CheckAttackResult(result);
	}

	/// <summary>
	/// Checks the attack result.
	/// </summary>
	/// <param name="result">Result.</param>
	private static void CheckAttackResult(AttackResult result)
	{
		switch (result.Value) {
			case ResultOfAttack.Miss:
			if (object.ReferenceEquals(_theGame.Player, ComputerPlayer))		/*Checks the results of the attack and switches to
	                                                                              Ending the Game if the result was game over */
					AIAttack();
				break;
			case ResultOfAttack.GameOver:
			SwitchState(GameState.EndingGame);							 /*Gets the AI to attack if the result switched
																		    to the AI player*/
				break;
		}
	}

	/// <summary>
	/// Handles the user SwinGame.
	/// </summary>
	/// <remarks>
	/// Reads key and mouse input and converts these into
	/// actions for the game to perform. The actions
	/// performed depend upon the state of the game.
	/// </remarks>
	public static void HandleUserInput()
	{
																			
		SwinGame.ProcessEvents();

		switch (CurrentState) {
			case GameState.ViewingMainMenu:
				MenuController.HandleMainMenuInput();
				break;
			case GameState.ViewingGameMenu:
				MenuController.HandleGameMenuInput();
				break;
			case GameState.AlteringSettings:
			MenuController.HandleSetupMenuInput();					/*Read incoming input events*/
				break;
			case GameState.Deploying:
				DeploymentController.HandleDeploymentInput();
				break;
			case GameState.Discovering:
				DiscoveryController.HandleDiscoveryInput();
				break;
			case GameState.EndingGame:
				EndingGameController.HandleEndOfGameInput();
				break;
			case GameState.ViewingHighScores:
				HighScoreController.HandleHighScoreInput();
				break;
		}

		UtilityFunctions.UpdateAnimations();
	}

	/// <summary>
	/// Draws the current state of the game to the screen.
	/// </summary>
	/// <remarks>
	/// What is drawn depends upon the state of the game.
	/// </remarks>
	public static void DrawScreen()
	{
		UtilityFunctions.DrawBackground();

		switch (CurrentState) {									/*display the state of the game*/
			case GameState.ViewingMainMenu:
				MenuController.DrawMainMenu();
				break;
			case GameState.ViewingGameMenu:
				MenuController.DrawGameMenu();
				break;
			case GameState.AlteringSettings:
				MenuController.DrawSettings();
				break;
			case GameState.Deploying:
				DeploymentController.DrawDeployment();
				break;
			case GameState.Discovering:
				DiscoveryController.DrawDiscovery();
				break;
			case GameState.EndingGame:
				EndingGameController.DrawEndOfGame();
				break;
			case GameState.ViewingHighScores:
				HighScoreController.DrawHighScores();
				break;
		}

		UtilityFunctions.DrawAnimations();

		SwinGame.RefreshScreen();
	}

	/// <summary>
	/// Move the game to a new state. The current state is maintained
	/// so that it can be returned to.
	/// </summary>
	/// <param name="state">the new game state</param>
	public static void AddNewState(GameState state)
	{
		_state.Push(state);											/* Move the game to a new state. The current state is maintained*/
		UtilityFunctions.Message = "";
	}

	/// <summary>
	/// End the current state and add in the new state.
	/// </summary>
	/// <param name="newState">the new state of the game</param>
	public static void SwitchState(GameState newState)
	{
		EndCurrentState();
		AddNewState(newState);
	}

	/// <summary>
	/// Ends the state of the current.
	/// </summary>
	public static void EndCurrentState()
	{
		_state.Pop();											/* returning to the prior state*/
	}

	/// <summary>
	/// Sets the difficulty.
	/// </summary>
	/// <param name="setting">Setting.</param>
	public static void SetDifficulty(AIOption setting)
	{
		_aiSetting = setting;							/* making the next setting for next level of the game*/
	}

	public static AIOption getDifficutly(){
		return _aiSetting;
	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
