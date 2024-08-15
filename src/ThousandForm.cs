using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Главная форма программы
	/// </summary>
	public partial class ThousandForm: Form
		{
		// "Рисунки" на сторонах кубиков
		private List<string> sides = new List<string> {
			"   " + RDLocale.RN + " ● " + RDLocale.RN + "   ",
			"  ●" + RDLocale.RN + "   " + RDLocale.RN + "●  ",
			"  ●" + RDLocale.RN + " ● " + RDLocale.RN + "●  ",
			"● ●" + RDLocale.RN + "   " + RDLocale.RN + "● ●",
			"● ●" + RDLocale.RN + " ● " + RDLocale.RN + "● ●",
			"● ●" + RDLocale.RN + "● ●" + RDLocale.RN + "● ●",
			};

		// Количества разных сторон кубиков
		private uint[] counts = new uint[] { 0, 0, 0, 0, 0, 0 };

		// Номер текущего игрока
		private uint currentPlayer = 1;

		// Очки игроков
		private uint[] playerScores = new uint[] { 0, 0, 0 };

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
		public ThousandForm ()
			{
			InitializeComponent ();
			}

		// Загрузка формы
		private void ThousandForm_Load (object sender, EventArgs e)
			{
			// Заголовок
			this.Text = ProgramDescription.AssemblyTitle;
			if (!RDGenerics.AppHasAccessRights (false, true))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);

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
			MHelp.Text = RDLocale.GetText ("GetHelp");
			MLanguage.Text = "&" +
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceLanguage).Replace (":", "");
			MExit.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit);

			BSelectPlayer.Text = RDLocale.GetText ("WhoIsFirstButton");
			NewCubes.Text = RDLocale.GetText ("ThrowDiceButton");
			SaveScores.Text = RDLocale.GetText ("SaveScoresButton");

			if (BSelectPlayer.Enabled)
				StatusLine.Text = RDLocale.GetText ("FirstStatus");
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

			SelectPlayerTimer.Interval = RDGenerics.RND.Next (50, 53);
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
				StatusLine.Text = GetCurrentPlayerName () + RDLocale.GetText ("ThrowDice");

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
					l.Text = sides[RDGenerics.RND.Next (sides.Count)];

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

				// Проверка очков
				if (preScores != 0)
					AddCurrentPlayerPreScores (preScores);

				// Перебор
				if ((preScores == 0) || (playerScores[currentPlayer - 1] + GetCurrentPlayerPreScores () > 1000))
					{
					// Обновление строки состояния
					if (preScores == 0)
						{
						StatusLine.Text = string.Format (RDLocale.GetText ("FailedThrow" +
							(GetCurrentPlayerPreScores () != 0 ? "1" : "2")), GetCurrentPlayerName ());
						}
					else
						{
						StatusLine.Text = GetCurrentPlayerName () + RDLocale.GetText ("FailedThrow3");
						}

					// Сброс пре-очков
					ResetCurrentPlayerPreScores ();

					// Выбор нового игрока
					currentPlayer = (currentPlayer % 3) + 1;
					HighlightCurrentPlayer ();

					// Обновление состояния
					StatusLine.Text += (GetCurrentPlayerName () + RDLocale.GetText ("ThrowDice"));

					// Переключение кнопок
					DisableButton (SaveScores);
					EnableButton (NewCubes);

					foreach (Label l in cubes)
						l.Enabled = true;
					return;
					}

				// Победа
				if (playerScores[currentPlayer - 1] + GetCurrentPlayerPreScores () == 1000)
					{
					// Сообщения
					StatusLine.Text = string.Format (RDLocale.GetText ("PlayerWon"), GetCurrentPlayerName ());
					RDGenerics.MessageBox (RDMessageTypes.Success_Center, StatusLine.Text);

					// Сброс настроек и перезапуск
					for (int i = 0; i < playerNames.Count; i++)
						{
						playerScores[i] = 0;
						preLines[i].Width = 0;
						totalLines[i].Width = 0;

						playerPreScoresLabels[i].Text = "0";
						playerScoresLabels[i].Text = "0";
						}

					DisableButton (SaveScores);
					EnableButton (NewCubes);
					foreach (Label l in cubes)
						l.Enabled = true;
					return;
					}

				// Включение кнопки
				StatusLine.Text = GetCurrentPlayerName () + RDLocale.GetText ("SaveOrTryAgain");
				if (GetCurrentPlayerPreScores () >= 50)
					StatusLine.Text += string.Format (RDLocale.GetText ("SaveOrTryAgainExt"),
						GetCurrentPlayerPreScores ());

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
					StatusLine.Text = GetCurrentPlayerName () + RDLocale.GetText ("Cask");
					DisableButton (SaveScores);
					}
				}
			}

		// Расчёт количества очков
		private uint EvaluateScores ()
			{
			uint scores = 0;

			// Получение количеств разных сторон кубиков
			for (int i = 0; i < counts.Length; i++)
				counts[i] = 0;

			foreach (Label l in cubes)
				if (l.Enabled)
					counts[sides.IndexOf (l.Text)]++;

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
			}

		// Вспомогательные функции назначения текущий очков
		private void AddCurrentPlayerPreScores (uint Value)
			{
			int i = (int)currentPlayer - 1;
			playerPreScoresLabels[i].Text = (uint.Parse (playerPreScoresLabels[i].Text) + Value).ToString ();

			uint scores = uint.Parse (playerScoresLabels[i].Text) + Value;
			playerScoresLabels[i].Text = scores.ToString ();
			preLines[i].Width = (int)((float)preBacks[i].Width * (float)scores / 1000.0f);
			}

		private void ResetCurrentPlayerPreScores ()
			{
			int i = (int)currentPlayer - 1;

			playerPreScoresLabels[i].Text = "0";
			playerScoresLabels[i].Text = playerScores[i].ToString ();
			preLines[i].Width = totalLines[i].Width;
			}

		// Сохранить очки
		private void SaveScores_Click (object sender, EventArgs e)
			{
			// Сохранение очков
			int i = (int)currentPlayer - 1;
			playerScores[i] = uint.Parse (playerScoresLabels[i].Text);
			preLines[i].Width = totalLines[i].Width = (int)((float)preBacks[i].Width *
				(float)playerScores[i] / 1000.0f);

			// Сброс пре-очков
			ResetCurrentPlayerPreScores ();
			StatusLine.Text = GetCurrentPlayerName () + RDLocale.GetText ("PointsSaved");

			currentPlayer = (currentPlayer % 3) + 1;
			HighlightCurrentPlayer ();
			StatusLine.Text += (GetCurrentPlayerName () + RDLocale.GetText ("ThrowDice"));

			DisableButton (SaveScores);
			foreach (Label l in cubes)
				l.Enabled = true;
			}

		// Вспомогательная функция получения пре-очков текущего игрока
		private uint GetCurrentPlayerPreScores ()
			{
			return uint.Parse (playerPreScoresLabels[(int)currentPlayer - 1].Text);
			}

		// Загрузка и выгрузка имён игроков
		private void LoadNames ()
			{
			for (int i = 0; i < playerNames.Count; i++)
				{
				playerNames[i].Text = RDGenerics.GetSettings ("P" + (i + 1).ToString (),
					RDLocale.GetText ("PlayerDefaultName" + (i + 1).ToString ()));

				if (string.IsNullOrWhiteSpace (playerNames[i].Text))
					playerNames[i].Text = RDLocale.GetText ("PlayerDefaultName") +
						(i + 1).ToString ();
				}
			}

		private void SaveNames ()
			{
			for (int i = 0; i < playerNames.Count; i++)
				RDGenerics.SetSettings ("P" + (i + 1).ToString (), playerNames[i].Text);
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
		private void ThousandForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			if (RDGenerics.LocalizedMessageBox (RDMessageTypes.Question_Center, "FinishGame",
				RDLDefaultTexts.Button_Yes, RDLDefaultTexts.Button_No) != RDMessageButtons.ButtonOne)
				{
				e.Cancel = true;
				return;
				}

			SaveNames ();
			RDGenerics.SaveWindowDimensions (this);
			}
		}
	}
