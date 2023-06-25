using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Главная форма программы
	/// </summary>
	public partial class MainForm: Form
		{
		// Переменные
		private List<string> sides = new List<string> {
			"•", "• •", "•••",
			"• • • •", "•••••", "••••••"
			}; // "Рисунки" на сторонах кубиков
		private uint[] counts = new uint[] { 0, 0, 0, 0, 0, 0 };    // Количества разных сторон кубиков
		private uint currentPlayer = 1;                             // Номер текущего игрока
		private uint[] playerScores = new uint[] { 0, 0, 0 };       // Очки игроков
		private Random rnd = new Random ();                         // ГПСЧ

		// Ссылки на контролы
		private List<Label> cubes;
		private List<TextBox> playerNames;
		private List<Label> playerScoresLabels;
		private List<Label> playerPreScoresLabels;

		private List<Label> preBacks;
		private List<Label> preLines;
		private List<Label> totalLines;

		/// <summary>
		/// Конструктор. Начинает работу
		/// </summary>
		public MainForm ()
			{
			InitializeComponent ();
			}

		// Загрузка формы
		private void MainForm_Load (object sender, EventArgs e)
			{
			// Заголовок
			this.Text = ProgramDescription.AssemblyTitle;
			RDGenerics.LoadWindowDimensions (this);

			LocalizeForm ();

			// Выключение пока ненужных кнопок
			DisableButton (NewCubes);
			DisableButton (SaveScores);

			cubes = new List<Label> { Cube1, Cube2, Cube3, Cube4, Cube5 };
			playerNames = new List<TextBox> { Player1Name, Player2Name, Player3Name };
			playerScoresLabels = new List<Label> { Player1Score, Player2Score, Player3Score };
			playerPreScoresLabels = new List<Label> { Player1PreScore, Player2PreScore, Player3PreScore };

			preBacks = new List<Label> { Pl1PB1Back, Pl2PB1Back, Pl3PB1Back };
			preLines = new List<Label> { Pl1PB2Line, Pl2PB2Line, Pl3PB2Line };
			totalLines = new List<Label> { Pl1PB3Line, Pl2PB3Line, Pl3PB3Line };

			// Настройка ProgressBar
			Pl1PB1Line1.Left = Pl1PB1Back.Left;
			Pl2PB1Line1.Left = Pl2PB1Back.Left;
			Pl3PB1Line1.Left = Pl3PB1Back.Left;
			Pl1PB1Line1.Width = Pl2PB1Line1.Width = Pl3PB1Line1.Width = (int)((float)Pl1PB1Back.Width / 10.0f);

			Pl1PB1Line2.Left = Pl1PB1Back.Left + (int)(3.0f * (float)Pl1PB1Back.Width / 10.0f);
			Pl2PB1Line2.Left = Pl2PB1Back.Left + (int)(3.0f * (float)Pl2PB1Back.Width / 10.0f);
			Pl3PB1Line2.Left = Pl3PB1Back.Left + (int)(3.0f * (float)Pl3PB1Back.Width / 10.0f);
			Pl1PB1Line2.Width = Pl2PB1Line2.Width = Pl3PB1Line2.Width = (int)((float)Pl1PB1Back.Width / 10.0f);

			Pl1PB1Line3.Left = Pl1PB1Back.Left + (int)(7.0f * (float)Pl1PB1Back.Width / 10.0f);
			Pl2PB1Line3.Left = Pl2PB1Back.Left + (int)(7.0f * (float)Pl2PB1Back.Width / 10.0f);
			Pl3PB1Line3.Left = Pl3PB1Back.Left + (int)(7.0f * (float)Pl3PB1Back.Width / 10.0f);
			Pl1PB1Line3.Width = Pl2PB1Line3.Width = Pl3PB1Line3.Width = (int)((float)Pl1PB1Back.Width / 10.0f);

			/*Pl1PB2Line.Width = Pl2PB2Line.Width = Pl3PB2Line.Width = 0;
			Pl1PB3Line.Width = Pl2PB3Line.Width = Pl3PB3Line.Width = 0;*/
			foreach (Label l in preLines)
				l.Width = 0;
			foreach (Label l in totalLines)
				l.Width = 0;

			// Получение имён игроков
			LoadNames ();
			}

		// Метод локализует форму
		private void LocalizeForm ()
			{
			MHelp.Text = Localization.GetText ("GetHelp");
			MLanguage.Text = "&" +
				Localization.GetDefaultText (LzDefaultTextValues.Control_InterfaceLanguage).Replace (":", "");
			MExit.Text = Localization.GetDefaultText (LzDefaultTextValues.Button_Exit);

			BSelectPlayer.Text = Localization.GetText ("WhoIsFirstButton");
			NewCubes.Text = Localization.GetText ("ThrowDiceButton");
			SaveScores.Text = Localization.GetText ("SaveScoresButton");

			if (BSelectPlayer.Enabled)
				StatusLine.Text = Localization.GetText ("FirstStatus");
			}

		// Вспомогательные функции
		private void EnableButton (Button Btn)
			{
			Btn.Enabled = true;
			Btn.BackColor = Color.FromArgb (0, 255, 0);
			}

		private void DisableButton (Button Btn)
			{
			Btn.Enabled = false;
			Btn.BackColor = Color.FromArgb (0, 64, 0);
			}

		// Выбор первого игрока
		private void BSelectPlayer_Click (object sender, EventArgs e)
			{
			DisableButton (BSelectPlayer);
			foreach (TextBox t in playerNames)
				t.Enabled = false;

			SelectPlayerTimer.Interval = rnd.Next (50, 53);
			SelectPlayerTimer.Enabled = true;
			}

		private void SelectPlayerTimer_Tick (object sender, EventArgs e)
			{
			// Выбор нового игрока
			currentPlayer = (currentPlayer % 3) + 1;
			HighlightCurrentPlayer ();

			// Установка нового смещения таймера
			SelectPlayerTimer.Interval++;
			if (SelectPlayerTimer.Interval == 100)
				{
				// Отключение таймера
				SelectPlayerTimer.Enabled = false;

				// Обновление строки состояния
				StatusLine.Text = GetCurrentPlayerName () + Localization.GetText ("ThrowDice");

				// Включение кнопок
				EnableButton (NewCubes);
				}
			}

		// Вспомогательная процедура подсветки текущего игрока
		private void HighlightCurrentPlayer ()
			{
			switch (currentPlayer)
				{
				case 1:
					Player1Name.BackColor = Player1Score.BackColor = Player1PreScore.BackColor =
						Color.FromArgb (255, 255, 0);
					Player2Name.BackColor = Player2Score.BackColor = Player2PreScore.BackColor =
						Player3Name.BackColor = Player3Score.BackColor = Player3PreScore.BackColor =
						Color.FromArgb (255, 255, 255);
					break;

				case 2:
					Player2Name.BackColor = Player2Score.BackColor = Player2PreScore.BackColor =
						Color.FromArgb (255, 255, 0);
					Player1Name.BackColor = Player1Score.BackColor = Player1PreScore.BackColor =
						Player3Name.BackColor = Player3Score.BackColor = Player3PreScore.BackColor =
						Color.FromArgb (255, 255, 255);
					break;

				case 3:
					Player3Name.BackColor = Player3Score.BackColor = Player3PreScore.BackColor =
						Color.FromArgb (255, 255, 0);
					Player2Name.BackColor = Player2Score.BackColor = Player2PreScore.BackColor =
						Player1Name.BackColor = Player1Score.BackColor = Player1PreScore.BackColor =
						Color.FromArgb (255, 255, 255);
					break;
				}
			}

		// Бросок
		private void NewCubes_Click (object sender, EventArgs e)
			{
			DisableButton (NewCubes);
			DisableButton (SaveScores);
			NewBonesTimer.Enabled = true;
			}

		private void NewBonesTimer_Tick (object sender, EventArgs e)
			{
			// Получение новых значений
			foreach (Label l in cubes)
				if (l.Enabled)
					l.Text = sides[rnd.Next (sides.Count)];
			/*if (Cube1.Enabled)
				Cube1.Text = sides[rnd.Next (6)];

			if (Cube2.Enabled)
				Cube2.Text = sides[rnd.Next (6)];

			if (Cube3.Enabled)
				Cube3.Text = sides[rnd.Next (6)];

			if (Cube4.Enabled)
				Cube4.Text = sides[rnd.Next (6)];

			if (Cube5.Enabled)
				Cube5.Text = sides[rnd.Next (6)];*/

			// Ожидаем отпускания клавиши
			if (rotatingBones)
				return;

			// Изменение интервала
			NewBonesTimer.Interval += 20;
			if (NewBonesTimer.Interval == 300)
				{
				// Отключение таймера
				NewBonesTimer.Enabled = false;
				NewBonesTimer.Interval = 100;

				// Подсчёт полученного количества очков
				uint preScores = EvaluateScores ();

				// Возврат всех кубиков, если все заняты
				bool haveActiveCubes = false;
				foreach (Label l in cubes)
					haveActiveCubes |= l.Enabled;

				if (!haveActiveCubes)
					foreach (Label l in cubes)
						l.Enabled = true;
				/*if ((Cube1.Enabled == Cube2.Enabled) && (Cube2.Enabled == Cube3.Enabled) &&
					(Cube3.Enabled == Cube4.Enabled) && (Cube4.Enabled == Cube5.Enabled) && (Cube5.Enabled == false))
					{
					Cube1.Enabled = Cube2.Enabled = Cube3.Enabled = Cube4.Enabled = Cube5.Enabled = true;
					}*/

				// Проверка очков
				if (preScores != 0)
					AddCurrentPlayerPreScores (preScores);

				// Перебор
				if ((preScores == 0) || (playerScores[currentPlayer - 1] + GetCurrentPlayerPreScores () > 1000))
					{
					// Обновление строки состояния
					if (preScores == 0)
						{
						/*if (GetCurrentPlayerPreScores () != 0)
							StatusLine.Text = GetCurrentPlayerName () + ", этот бросок не удался. ";
						else
							StatusLine.Text = "Ну, " + GetCurrentPlayerName () + ", бывает и так. ";*/
						StatusLine.Text = string.Format (Localization.GetText ("FailedThrow" +
							(GetCurrentPlayerPreScores () != 0 ? "1" : "2")), GetCurrentPlayerName ());
						}
					else
						{
						StatusLine.Text = GetCurrentPlayerName () + Localization.GetText ("FailedThrow3");
						}

					// Сброс пре-очков
					ResetCurrentPlayerPreScores ();

					// Выбор нового игрока
					currentPlayer = (currentPlayer % 3) + 1;
					HighlightCurrentPlayer ();

					// Обновление состояния
					StatusLine.Text += (GetCurrentPlayerName () + Localization.GetText ("ThrowDice"));

					// Переключение кнопок
					DisableButton (SaveScores);
					EnableButton (NewCubes);
					/*Cube1.Enabled = Cube2.Enabled = Cube3.Enabled = Cube4.Enabled = Cube5.Enabled = true;*/

					foreach (Label l in cubes)
						l.Enabled = true;
					return;
					}

				// Победа
				if (playerScores[currentPlayer - 1] + GetCurrentPlayerPreScores () == 1000)
					{
					// Сообщения
					StatusLine.Text = string.Format (Localization.GetText ("PlayerWon"), GetCurrentPlayerName ());
					/*MessageBox.Show ("Игрок " + GetCurrentPlayerName () + " победил!",
						ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);*/
					RDGenerics.MessageBox (RDMessageTypes.Success, StatusLine.Text);

					// Сброс настроек и перезапуск
					for (int i = 0; i < playerNames.Count; i++)
						{
						playerScores[i] = 0;
						preLines[i].Width = 0;
						totalLines[i].Width = 0;

						playerPreScoresLabels[i].Text = "0";
						playerScoresLabels[i].Text = "0";
						}

					/*SaveNames ();
					Application.Restart ();*/

					DisableButton (SaveScores);
					EnableButton (NewCubes);
					foreach (Label l in cubes)
						l.Enabled = true;
					return;
					}

				// Включение кнопки
				StatusLine.Text = GetCurrentPlayerName () + Localization.GetText ("SaveOrTryAgain");
				if (GetCurrentPlayerPreScores () >= 50)
					StatusLine.Text += string.Format (Localization.GetText ("SaveOrTryAgainExt"),
						GetCurrentPlayerPreScores ());
				/*StatusLine.Text += ". Но " + GetCurrentPlayerPreScores () + " очков на дороге не валяются!";*/

				EnableButton (NewCubes);

				// Включение при условии, что "бочка покинута"
				EnableButton (SaveScores);
				if ((playerScores[currentPlayer - 1] < 100) && (playerScores[currentPlayer - 1] +
					GetCurrentPlayerPreScores () < 100) ||
					(playerScores[currentPlayer - 1] >= 300) && (playerScores[currentPlayer - 1] < 400) &&
					(playerScores[currentPlayer - 1] + GetCurrentPlayerPreScores () < 400) ||
					(playerScores[currentPlayer - 1] >= 700) && (playerScores[currentPlayer - 1] < 800) &&
					(playerScores[currentPlayer - 1] + GetCurrentPlayerPreScores () < 800))
					{
					StatusLine.Text = GetCurrentPlayerName () + Localization.GetText ("Cask");
					DisableButton (SaveScores);
					}
				}
			}

		// Расчёт количества очков
		private uint EvaluateScores ()
			{
			uint scores = 0;

			// Получение количеств разных сторон кубиков
			/*counts[0] = counts[1] = counts[2] = counts[3] = counts[4] = counts[5] = 0;*/
			for (int i = 0; i < counts.Length; i++)
				counts[i] = 0;

			foreach (Label l in cubes)
				if (l.Enabled)
					counts[sides.IndexOf (l.Text)]++;
			/*if (Cube1.Enabled)
				counts[sides.IndexOf (Cube1.Text)]++;

			if (Cube2.Enabled)
				counts[sides.IndexOf (Cube2.Text)]++;

			if (Cube3.Enabled)
				counts[sides.IndexOf (Cube3.Text)]++;

			if (Cube4.Enabled)
				counts[sides.IndexOf (Cube4.Text)]++;

			if (Cube5.Enabled)
				counts[sides.IndexOf (Cube5.Text)]++;*/

			// Поиск комбинаций
			#region Стриты

			// 1-2-3-4-5
			if ((counts[0] == counts[1]) && (counts[1] == counts[2]) && (counts[2] == counts[3]) &&
				(counts[3] == counts[4]) && (counts[4] == 1))
				{
				scores = 150;
				return scores;
				}

			// 2-3-4-5-6
			if ((counts[5] == counts[1]) && (counts[1] == counts[2]) && (counts[2] == counts[3]) &&
				(counts[3] == counts[4]) && (counts[4] == 1))
				{
				scores = 250;
				return scores;
				}

			#endregion

			#region 5 одинаковых

			// 1-1-1-1-1
			if (counts[0] == 5)
				{
				scores = 1000;
				return scores;
				}

			// 2-2-2-2-2
			if (counts[1] == 5)
				{
				scores = 200;
				return scores;
				}

			// 3-3-3-3-3
			if (counts[2] == 5)
				{
				scores = 300;
				return scores;
				}

			// 4-4-4-4-4
			if (counts[3] == 5)
				{
				scores = 400;
				return scores;
				}

			// 5-5-5-5-5
			if (counts[4] == 5)
				{
				scores = 500;
				return scores;
				}

			// 6-6-6-6-6
			if (counts[5] == 5)
				{
				scores = 600;
				return scores;
				}

			#endregion

			#region 4 одинаковых

			// 1-1-1-1
			if (counts[0] == 4)
				{
				scores += 200;
				DisableCubes (sides[0]);
				}

			// 2-2-2-2
			if (counts[1] == 4)
				{
				scores += 40;
				DisableCubes (sides[1]);
				}

			// 3-3-3-3
			if (counts[2] == 4)
				{
				scores += 60;
				DisableCubes (sides[2]);
				}

			// 4-4-4-4
			if (counts[3] == 4)
				{
				scores += 80;
				DisableCubes (sides[3]);
				}

			// 5-5-5-5
			if (counts[4] == 4)
				{
				scores += 100;
				DisableCubes (sides[4]);
				}

			// 6-6-6-6
			if (counts[5] == 4)
				{
				scores += 120;
				DisableCubes (sides[5]);
				}

			#endregion

			#region 3 одинаковых

			// 1-1-1
			if (counts[0] == 3)
				{
				scores += 100;
				DisableCubes (sides[0]);
				}

			// 2-2-2
			if (counts[1] == 3)
				{
				scores += 20;
				DisableCubes (sides[1]);
				}

			// 3-3-3
			if (counts[2] == 3)
				{
				scores += 30;
				DisableCubes (sides[2]);
				}

			// 4-4-4
			if (counts[3] == 3)
				{
				scores += 40;
				DisableCubes (sides[3]);
				}

			// 5-5-5
			if (counts[4] == 3)
				{
				scores += 50;
				DisableCubes (sides[4]);
				}

			// 6-6-6
			if (counts[5] == 3)
				{
				scores += 60;
				DisableCubes (sides[5]);
				}

			#endregion

			#region Непарные единицы и пятёрки

			// 1-1
			if (counts[0] == 2)
				{
				scores += 20;
				DisableCubes (sides[0]);
				}

			// 5-5
			if (counts[4] == 2)
				{
				scores += 10;
				DisableCubes (sides[4]);
				}

			// 1
			if (counts[0] == 1)
				{
				scores += 10;
				DisableCubes (sides[0]);
				}

			// 5
			if (counts[4] == 1)
				{
				scores += 5;
				DisableCubes (sides[4]);
				}

			#endregion

			// Всё посчитано
			return scores;
			}

		// Вспомогательная функция отключения кубиков
		private void DisableCubes (string CubeSide)
			{
			foreach (Label l in cubes)
				if (l.Text == CubeSide)
					l.Enabled = false;

			/*if (Cube1.Text == CubeSide)
				Cube1.Enabled = false;

			if (Cube2.Text == CubeSide)
				Cube2.Enabled = false;

			if (Cube3.Text == CubeSide)
				Cube3.Enabled = false;

			if (Cube4.Text == CubeSide)
				Cube4.Enabled = false;

			if (Cube5.Text == CubeSide)
				Cube5.Enabled = false;*/
			}

		// Вспомогательные функции назначения текущий очков
		private void AddCurrentPlayerPreScores (uint Value)
			{
			/*switch (currentPlayer)
				{
				case 1:
					Player1PreScore.Text = (uint.Parse (Player1PreScore.Text) + Value).ToString ();
					Player1Score.Text = (uint.Parse (Player1Score.Text) + Value).ToString ();
					Pl1PB2Line.Width = (int)((float)Pl1PB2Back.Width * (float)uint.Parse (Player1Score.Text) / 1000.0f);
					break;

				case 2:
					Player2PreScore.Text = (uint.Parse (Player2PreScore.Text) + Value).ToString ();
					Player2Score.Text = (uint.Parse (Player2Score.Text) + Value).ToString ();
					Pl2PB2Line.Width = (int)((float)Pl2PB2Back.Width * (float)uint.Parse (Player2Score.Text) / 1000.0f);
					break;

				case 3:
					Player3PreScore.Text = (uint.Parse (Player3PreScore.Text) + Value).ToString ();
					Player3Score.Text = (uint.Parse (Player3Score.Text) + Value).ToString ();
					Pl3PB2Line.Width = (int)((float)Pl3PB2Back.Width * (float)uint.Parse (Player3Score.Text) / 1000.0f);
					break;
				}*/

			int i = (int)currentPlayer - 1;
			playerPreScoresLabels[i].Text = (uint.Parse (playerPreScoresLabels[i].Text) + Value).ToString ();

			uint scores = uint.Parse (playerScoresLabels[i].Text) + Value;
			playerScoresLabels[i].Text = scores.ToString ();
			preLines[i].Width = (int)((float)preBacks[i].Width * (float)scores / 1000.0f);
			}

		private void ResetCurrentPlayerPreScores ()
			{
			/*switch (currentPlayer)
				{
				case 1:
					Player1PreScore.Text = "0";
					Player1Score.Text = playerScores[0].ToString ();
					Pl1PB2Line.Width = Pl1PB3Line.Width;
					break;

				case 2:
					Player2PreScore.Text = "0";
					Player2Score.Text = playerScores[1].ToString ();
					Pl2PB2Line.Width = Pl2PB3Line.Width;
					break;

				case 3:
					Player3PreScore.Text = "0";
					Player3Score.Text = playerScores[2].ToString ();
					Pl3PB2Line.Width = Pl3PB3Line.Width;
					break;
				}*/

			int i = (int)currentPlayer - 1;

			playerPreScoresLabels[i].Text = "0";
			playerScoresLabels[i].Text = playerScores[i].ToString ();
			preLines[i].Width = totalLines[i].Width;
			}

		// Сохранить очки
		private void SaveScores_Click (object sender, EventArgs e)
			{
			// Сохранение очков
			/*switch (currentPlayer)
				{
				case 1:
					playerScores[0] = uint.Parse (Player1Score.Text);
					Pl1PB2Line.Width = Pl1PB3Line.Width = (int)((float)Pl1PB2Back.Width *
						(float)(uint.Parse (Player1Score.Text)) / 1000.0f);
					break;

				case 2:
					playerScores[1] = uint.Parse (Player2Score.Text);
					Pl2PB2Line.Width = Pl2PB3Line.Width = (int)((float)Pl2PB2Back.Width *
						(float)(uint.Parse (Player2Score.Text)) / 1000.0f);
					break;

				case 3:
					playerScores[2] = uint.Parse (Player3Score.Text);
					Pl3PB2Line.Width = Pl3PB3Line.Width = (int)((float)Pl3PB2Back.Width *
						(float)(uint.Parse (Player3Score.Text)) / 1000.0f);
					break;
				}*/

			int i = (int)currentPlayer - 1;
			playerScores[i] = uint.Parse (playerScoresLabels[i].Text);
			preLines[i].Width = totalLines[i].Width = (int)((float)preBacks[i].Width *
				(float)playerScores[i] / 1000.0f);

			// Сброс пре-очков
			ResetCurrentPlayerPreScores ();
			StatusLine.Text = GetCurrentPlayerName () + Localization.GetText ("PointsSaved");

			currentPlayer = (currentPlayer % 3) + 1;
			HighlightCurrentPlayer ();
			StatusLine.Text += (GetCurrentPlayerName () + Localization.GetText ("ThrowDice"));

			DisableButton (SaveScores);
			/*Cube1.Enabled = Cube2.Enabled = Cube3.Enabled = Cube4.Enabled = Cube5.Enabled = true;*/
			foreach (Label l in cubes)
				l.Enabled = true;
			}

		// Вспомогательная функция получения пре-очков текущего игрока
		private uint GetCurrentPlayerPreScores ()
			{
			/*switch (currentPlayer)
				{
				case 1:
					return uint.Parse (Player1PreScore.Text);

				case 2:
					return uint.Parse (Player2PreScore.Text);

				case 3:
					return uint.Parse (Player3PreScore.Text);
				}

			return 0;*/
			return uint.Parse (playerPreScoresLabels[(int)currentPlayer - 1].Text);
			}

		// Загрузка и выгрузка имён игроков
		private void LoadNames ()
			{
			/* Попытка открытия файла
			FileStream FS = null;
			try
				{
				FS = new FileStream (Application.StartupPath + "\\Thousand.cfg", FileMode.Open);
				}
			catch
				{
				SaveNames ();
				return;
				}
			StreamReader SR = new StreamReader (FS, System.Text.Encoding.GetEncoding (1251));

			// Загрузка
			try
				{
				Player1Name.Text = SR.ReadLine ();
				Player2Name.Text = SR.ReadLine ();
				Player3Name.Text = SR.ReadLine ();
				}
			catch
				{
				SaveNames ();
				}

			// Завершение
			SR.Close ();
			FS.Close ();*/

			for (int i = 0; i < playerNames.Count; i++)
				{
				playerNames[i].Text = RDGenerics.GetAppSettingsValue ("P" + (i + 1).ToString ());
				if (string.IsNullOrWhiteSpace (playerNames[i].Text))
					playerNames[i].Text = Localization.GetText ("PlayerDefaultName") +
						(i + 1).ToString ();
				}
			}

		private void SaveNames ()
			{
			/* Попытка открытия файла
			FileStream FS = null;
			try
				{
				FS = new FileStream (Application.StartupPath + "\\Thousand.cfg", FileMode.Create);
				}
			catch
				{
				}
			StreamWriter SW = new StreamWriter (FS, System.Text.Encoding.GetEncoding (1251));

			// Загрузка
			SW.WriteLine (Player1Name.Text);
			SW.WriteLine (Player2Name.Text);
			SW.WriteLine (Player3Name.Text);

			// Завершение
			SW.Close ();
			FS.Close ();*/

			RDGenerics.SetAppSettingsValue ("P1", Player1Name.Text);
			RDGenerics.SetAppSettingsValue ("P2", Player2Name.Text);
			RDGenerics.SetAppSettingsValue ("P3", Player3Name.Text);
			}

		/// <summary>
		/// Обработка клавиатурных вызовов
		/// </summary>
		protected override void OnKeyDown (KeyEventArgs e)
			{
			if ((e.KeyCode != Keys.Space) || !NewCubes.Enabled)
				{
				base.OnKeyDown (e);
				return;
				}

			rotatingBones = true;
			NewCubes_Click (null, null);
			}
		private bool rotatingBones = false;

		/// <summary>
		/// Обработка клавиатурных вызовов
		/// </summary>
		protected override void OnKeyUp (KeyEventArgs e)
			{
			if (e.KeyCode != Keys.Space)
				{
				base.OnKeyDown (e);
				return;
				}

			rotatingBones = false;
			}

		/// <summary>
		/// Обработка клавиатурных вызовов
		/// </summary>
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
			{
			switch (keyData)
				{
				case Keys.F5:
					if (BSelectPlayer.Enabled)
						BSelectPlayer_Click (null, null);
					return true;

				/*case Keys.Space:
					if (NewCubes.Enabled)
						NewCubes_Click (null, null);
					return true;*/

				case Keys.Return:
					if (SaveScores.Enabled)
						SaveScores_Click (null, null);
					return true;
				}

			return base.ProcessCmdKey (ref msg, keyData);
			}

		// Вспомогательная функция получения имени текущего игрока
		private string GetCurrentPlayerName ()
			{
			return playerNames[(int)currentPlayer - 1].Text;

			/*switch (currentPlayer)
				{
				case 1:
					return Player1Name.Text;

				case 2:
					return Player2Name.Text;

				case 3:
					return Player3Name.Text;
				}

			return "";*/
			}

		// Выход из игры
		private void MExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Вызов справки
		private void MHelp_Click (object sender, EventArgs e)
			{
			RDGenerics.ShowAbout (false);
			}

		// Изменение языка интерфейса
		private void MLanguage_Click (object sender, EventArgs e)
			{
			if (RDGenerics.MessageBox ())
				LocalizeForm ();
			}

		// Закрытие окна
		private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			/*if (MessageBox.Show ("Завершить игру?", ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)*/
			if (RDGenerics.LocalizedMessageBox (RDMessageTypes.Question, "FinishGame", LzDefaultTextValues.Button_Yes,
				LzDefaultTextValues.Button_No) != RDMessageButtons.ButtonOne)
				{
				e.Cancel = true;
				return;
				}

			SaveNames ();
			RDGenerics.SaveWindowDimensions (this);
			}
		}
	}
