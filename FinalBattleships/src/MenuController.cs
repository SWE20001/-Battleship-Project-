
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data;
using System.Diagnostics;
using SwinGameSDK;

/// <summary>
/// Menu controller.
/// </summary>
static class MenuController
																		/* handles the drawing and user interactions
																		   from the menus . also include the main menu, game
																		   menu and the settings.*/
{

	/// <summary>
	/// The menu structure.
	/// </summary>

	private static readonly string[][] _menuStructure = {
		new string[] {
			"PLAY",
			"SETUP",
			"SCORES",
			"QUIT"
		},
		new string[] {	 												/* These are the text captions for the menu items. */
																			
			"RETURN",
			"SURRENDER",
			"QUIT"
		},
		new string[] {
			"EASY",
			"MEDIUM",
			"HARD"
		}

	};
	private const int MENU_TOP = 575;
	private const int MENU_LEFT = 30;
	private const int MENU_GAP = 0;
	private const int BUTTON_WIDTH = 75;
	private const int BUTTON_HEIGHT = 15;
	private const int BUTTON_SEP = BUTTON_WIDTH + MENU_GAP;

	private const int TEXT_OFFSET = 0;
	private const int MAIN_MENU = 0;
	private const int GAME_MENU = 1;

	private const int SETUP_MENU = 2;
	private const int MAIN_MENU_PLAY_BUTTON = 0;
	private const int MAIN_MENU_SETUP_BUTTON = 1;
	private const int MAIN_MENU_TOP_SCORES_BUTTON = 2;

	private const int MAIN_MENU_QUIT_BUTTON = 3;
	private const int SETUP_MENU_EASY_BUTTON = 0;
	private const int SETUP_MENU_MEDIUM_BUTTON = 1;
	private const int SETUP_MENU_HARD_BUTTON = 2;

	private const int SETUP_MENU_EXIT_BUTTON = 3;
	private const int GAME_MENU_RETURN_BUTTON = 0;
	private const int GAME_MENU_SURRENDER_BUTTON = 1;

	private const int GAME_MENU_QUIT_BUTTON = 2;
	private static readonly Color MENU_COLOR = SwinGame.RGBAColor(2, 167, 252, 255);

	private static readonly Color HIGHLIGHT_COLOR = SwinGame.RGBAColor(1, 57, 86, 255);

	/// <summary>
	/// Handles the main menu input.
	/// </summary>
	public static void HandleMainMenuInput()
																				/*Handles the processing of user input when the main menu is showing*/
	{
		HandleMenuInput(MAIN_MENU, 0, 0);
	}

	/// <summary>
	/// Handles the setup menu input.
	/// </summary>
	public static void HandleSetupMenuInput()
	{
		bool handled = false;
		handled = HandleMenuInput(SETUP_MENU, 1, 1);								/*setting for menuinput */

		if (!handled) {
			HandleMenuInput(MAIN_MENU, 0, 0);
		}
	}

	/// <summary>
	/// Handles the game menu input.
	/// </summary>
	public static void HandleGameMenuInput()
																					/* Player can return to the game, surrender, or quit entirely */
	{																			
		HandleMenuInput(GAME_MENU, 0, 0);
	}

	/// <summary>
	/// Handles input for the specified menu.
	/// </summary>
	/// <param name="menu">the identifier of the menu being processed</param>
	/// <param name="level">the vertical level of the menu</param>
	/// <param name="xOffset">the xoffset of the menu</param>
	/// <returns>false if a clicked missed the buttons. This can be used to check prior menus.</returns>
	private static bool HandleMenuInput(int menu, int level, int xOffset)
	{
		if (SwinGame.KeyTyped(KeyCode.vk_ESCAPE)) {										/*proccessing for the condition, so <c>true</c>, if menu input was handled, <c>false</c> otherwise.*/
			GameController.EndCurrentState();
			return true;
		}

		if (SwinGame.MouseClicked(MouseButton.LeftButton)) {
			int i = 0;
			for (i = 0; i <= _menuStructure[menu].Length - 1; i++) {
																							/*IsMouseOver the i'th button of the menu*/
				if (IsMouseOverMenu(i, level, xOffset)) {
					PerformMenuAction(menu, i);
					return true;
				}
			}

			if (level > 0) {
																								/*none clicked - so end this sub menu*/
				GameController.EndCurrentState();
			}
		}

		return false;
	}

	/// <summary>
	/// Draws the main menu to the screen.
	/// </summary>
	public static void DrawMainMenu()
	{
																								/* Clears the Screen to Black */
		//SwinGame.DrawText("Main Menu", Color.White, GameFont("ArialLarge"), 50, 50)

		DrawButtons(MAIN_MENU);
	}

	/// <summary>
	/// Draws the Game menu to the screen
	/// </summary>
	public static void DrawGameMenu()
	{
																								/* Clears the Screen to Black */
		//SwinGame.DrawText("Paused", Color.White, GameFont("ArialLarge"), 50, 50)

		DrawButtons(GAME_MENU);
	}

	/// <summary>
	/// Draws the settings.
	/// </summary>
	public static void DrawSettings()
	{
		
																											/* Also shows the main menu */
		//SwinGame.DrawText("Settings", Color.White, GameResources.GameFont("ArialLarge"), 50, 50);

		DrawButtons(MAIN_MENU);
		DrawButtons (SETUP_MENU, 1, 1);

		int btnTop = MENU_TOP - (MENU_GAP + BUTTON_HEIGHT) * 1;
		int i =0;
		if (GameController.getDifficutly () == AIOption.Easy)
		{
			i = 0;
		}
		else if (GameController.getDifficutly () == AIOption.Medium)
		{
			i = 1;
		}else if (GameController.getDifficutly () == AIOption.Hard)
		{
			i = 2;
		}

		int btnLeft = MENU_LEFT + BUTTON_SEP * (i + 1);
		SwinGame.DrawRectangle (HIGHLIGHT_COLOR, btnLeft, btnTop, BUTTON_WIDTH, BUTTON_HEIGHT);

	}

	/// <summary>
	/// Draws the buttons.
	/// </summary>
	/// <param name="menu">Menu.</param>
	private static void DrawButtons(int menu)
																					/*drawing associated with a top level menu.*/
	{
		DrawButtons(menu, 0, 0);
	}

	/// <summary>
	/// Draws the buttons.
	/// </summary>
	/// <param name="menu">Menu.</param>
	/// <param name="level">Level.</param>
	/// <param name="xOffset">X offset.</param>
	private static void DrawButtons(int menu, int level, int xOffset)																	
	{
		int btnTop = 0;

		btnTop = MENU_TOP - (MENU_GAP + BUTTON_HEIGHT) * level;
		int i = 0;
		for (i = 0; i <= _menuStructure[menu].Length - 1; i++) {				/* The menu text comes from the _menuStructure field. The level indicates the height
																				of the menu, to enable sub menus. The xOffset repositions the menu horizontally
																				to allow the submenus to be positioned correctly.*/
			int btnLeft = 0;
			btnLeft = MENU_LEFT + BUTTON_SEP * (i + xOffset);
			//SwinGame.FillRectangle (Color.White, btnLeft, btnTop, BUTTON_WIDTH, BUTTON_HEIGHT);
			SwinGame.DrawTextLines(_menuStructure[menu][i], MENU_COLOR, Color.Black, GameResources.GameFont("Menu"), FontAlignment.AlignCenter, btnLeft + TEXT_OFFSET, btnTop + TEXT_OFFSET, BUTTON_WIDTH, BUTTON_HEIGHT);

			if (SwinGame.MouseDown(MouseButton.LeftButton) & IsMouseOverMenu(i, level, xOffset)) {
				SwinGame.DrawRectangle(HIGHLIGHT_COLOR, btnLeft, btnTop, BUTTON_WIDTH, BUTTON_HEIGHT);
			}
		}
	}

	/// <summary>
	/// Determines if is mouse over button the specified button.
	/// </summary>
	/// <returns><c>true</c> if is mouse over button the specified button; otherwise, <c>false</c>.</returns>
	/// <param name="button">Button.</param>
	private static bool IsMouseOverButton(int button)
	{
		return IsMouseOverMenu(button, 0, 0);							/* checking the above condition */
	}

	/// <summary>
	/// Determines if is mouse over menu the specified button level xOffset.
	/// </summary>
	/// <returns><c>true</c> if is mouse over menu the specified button level xOffset; otherwise, <c>false</c>.</returns>
	/// <param name="button">Button.</param>
	/// <param name="level">Level.</param>
	/// <param name="xOffset">X offset.</param>
	private static bool IsMouseOverMenu(int button, int level, int xOffset)
	{
		int btnTop = MENU_TOP - (MENU_GAP + BUTTON_HEIGHT) * level;
		int btnLeft = MENU_LEFT + BUTTON_SEP * (button + xOffset);

		return UtilityFunctions.IsMouseInRectangle(btnLeft, btnTop, BUTTON_WIDTH, BUTTON_HEIGHT); 			/*return the true if the mouse is over , otherwise false */
	}

	/// <summary>
	/// A button has been clicked, perform the associated action.
	/// </summary>
	/// <param name="menu">the menu that has been clicked</param>
	/// <param name="button">the index of the button that was clicked</param>
	private static void PerformMenuAction(int menu, int button)
	{
		switch (menu) {
			case MAIN_MENU:
				PerformMainMenuAction(button);
				break;
			case SETUP_MENU:
				PerformSetupMenuAction(button);
				break;
			case GAME_MENU:
				PerformGameMenuAction(button);
				break;
		}
	}

	/// <summary>
	/// Performs the main menu action.
	/// </summary>
	/// <param name="button">Button.</param>
	private static void PerformMainMenuAction(int button)									/* if he main menu was clicked*/
	{
		switch (button) {
			case MAIN_MENU_PLAY_BUTTON:
				GameController.StartGame();
				break;
			case MAIN_MENU_SETUP_BUTTON:
				GameController.AddNewState(GameState.AlteringSettings);
				break;
			case MAIN_MENU_TOP_SCORES_BUTTON:
				GameController.AddNewState(GameState.ViewingHighScores);
				break;
			case MAIN_MENU_QUIT_BUTTON:
				GameController.EndCurrentState();
				break;
		}
	}

	/// <summary>
	/// Performs the setup menu action.
	/// </summary>
	/// <param name="button">Button.</param>
	private static void PerformSetupMenuAction(int button)										/* The setup menu was clicked */
	{
		switch (button) {
			case SETUP_MENU_EASY_BUTTON:
				GameController.SetDifficulty(AIOption.Easy);
				break;
			case SETUP_MENU_MEDIUM_BUTTON:
				GameController.SetDifficulty(AIOption.Medium);
				break;
			case SETUP_MENU_HARD_BUTTON:
				GameController.SetDifficulty(AIOption.Hard);
				break;
		}
																							/* Always end state - handles exit button as well */
		GameController.EndCurrentState();
	}

	/// <summary>
	/// The game menu was clicked, perform the button's action.
	/// </summary>
	/// <param name="button">the button pressed</param>
	private static void PerformGameMenuAction(int button)
	{
		switch (button) {
			case GAME_MENU_RETURN_BUTTON:
				GameController.EndCurrentState();
				break;
			case GAME_MENU_SURRENDER_BUTTON:
				GameController.EndCurrentState();
																								/* end game menu */
				GameController.EndCurrentState();
																								/* end game */
				break;
			case GAME_MENU_QUIT_BUTTON:
				GameController.AddNewState(GameState.Quitting);
				break;
		}
	}
}


