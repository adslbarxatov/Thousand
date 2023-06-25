using System;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Главная форма программы
	/// </summary>
	public partial class MainForm: Form
		{
		// Игроки
		private Player compPlayer, player;
		private uint playersWins = uint.MaxValue;
		private uint compWins = uint.MaxValue;

		// Текущая карта
		private Card currentCard;

		// Запрашиваемая игроком масть
		private CardSuits playerRequestedSuit = CardSuits.Clubs;

		// Колода
		private CardsPack pack;

		// Цветовая схема
		private Color backColor = Color.FromArgb (0, 128, 0);
		private Color fieldColor = Color.FromArgb (0, 96, 0);
		private Color cardBackColor = Color.FromArgb (192, 0, 0);
		private Color cardForeColor = Color.FromArgb (255, 255, 255);
		private Color textColor = Color.FromArgb (255, 255, 0);
		private Color scoreColor = Color.FromArgb (0, 128, 255);
		private Color blackColor = Color.FromArgb (0, 0, 0);
		private Color redColor = Color.FromArgb (255, 0, 0);
		private Color[] suitSelectorColors;

		/// <summary>
		/// Конструктор
		/// </summary>
		public MainForm ()
			{
			// Инициализация
			InitializeComponent ();

			this.BackColor = backColor;
			CompPlayer.BackgroundColor = PlayerField.BackgroundColor =
				CompPlayer.GridColor = PlayerField.GridColor = fieldColor;
			CompScoresBar.BackColor = PlayerScoresBar.BackColor = fieldColor;
			CompScoresBar.ForeColor = PlayerScoresBar.ForeColor = scoreColor;
			SkipTurn.ForeColor = TakeCard.ForeColor = PackLabel.ForeColor = textColor;
			PackLabel.BackColor = cardBackColor;
			CurCard.BackColor = cardForeColor;
			CurCard.ForeColor = blackColor;

			suitSelectorColors = new Color[] {
				backColor, Color.FromArgb (128,255,128),
				blackColor, redColor
				};

			// Установка текстовый полей
			this.Text = ProgramDescription.AssemblyTitle;
			LocalizeForm ();
			}

		// Метод локализует форму
		private void LocalizeForm ()
			{
			MStartFirstRound.Text = Localization.GetText ("BeginTheGame");
			MHelp.Text = Localization.GetText ("GetHelp");
			MExit.Text = Localization.GetDefaultText (LzDefaultTextValues.Button_Exit);
			MLanguage.Text =
				Localization.GetDefaultText (LzDefaultTextValues.Control_InterfaceLanguage).Replace (":", "");

			TakeCard.Text = Localization.GetText ("TakeCard");
			SkipTurn.Text = Localization.GetText ("SkipTurn");
			}

		// Начало первого раунда
		private void StartFirstRound_Click (object sender, EventArgs e)
			{
			MStartFirstRound.Enabled = false;
			StartNewRound ();
			}

		// Вспомогательная функция. Выполняет подкотовку элементов управления для нового раунда
		private void StartNewRound ()
			{
			// Инициализация колоды и первой карты
			pack = new CardsPack ();
			currentCard = pack.GetRandomCard2 ();

			// Инициализация или сброс игроков
			if (compPlayer == null)
				compPlayer = new Player (pack, false);  // Компьютер ходит вторым
			else
				compPlayer.ReInitializeSecondPlayer (pack);

			if (player == null)
				player = new Player (pack, true);       // Игрок ходит первым
			else
				player.ReInitializeFirstPlayer (pack);

			// Начальная отрисовка
			DrawPlayersCardsHand (compPlayer.Hand, CompPlayer, false);  // Карты компьютера отрисовываются в закрытую
			DrawPlayersCardsHand (player.Hand, PlayerField, true);      // Игрока, естественно, в открытую
			DrawCurrentCard (currentCard);
			DrawScores ();

			// Обработка первой дамы
			if (currentCard.CardValue == CardValues.Queen)
				{
				// Запрос масти
				pack.ThrowCard (currentCard);
				SuitSelector ss = new SuitSelector (suitSelectorColors);
				currentCard = new Card (ss.SelectedSuit, CardValues.Request);
				ss.Dispose ();

				// Перерисовка
				DrawCurrentCard (currentCard);
				}

			// Если первый ход, сделанный игроком, требует покрытия
			if (GameRules.CoveringNotNeeded (currentCard))
				{
				PlayerField.Enabled = false;    // Состояние поля карт игрока используется как указатель ходящего игрока
				TakeCard.Enabled = false;
				SkipTurn.Enabled = false;

				// Запуск компьютера
				CompPlayerTimer.Enabled = true;

				SetBarState (false, false);
				/*CompScoresBar.BackColor = SkipTurn.ForeColor;
				PlayerScoresBarBackColor = CompPlayer.BackgroundColor;*/
				}

			// В противном случае
			else
				{
				Card card = compPlayer.MakeMove (pack, currentCard);    // По идее, приём карты не требуется
				if (!currentCard.Equals (card))
					throw new Exception ("Critical: player answer for should-cover-card should not return new card, " +
						"point 6");

				DrawPlayersCardsHand (compPlayer.Hand, CompPlayer, false);
				// По результатам ответа карты перерисовываются

				PlayerField.Enabled = true;
				TakeCard.Enabled = true;
				/*CompScoresBar.BackColor = CompPlayer.BackgroundColor;
				PlayerScoresBarBackColor = SkipTurn.ForeColor;*/
				SetBarState (true, false);
				}
			}

		// Установка цветов полей результатов
		private void SetBarState (bool Player, bool DisableAll)
			{
			if (DisableAll)
				{
				CompScoresBar.BackColor = PlayerScoresBar.BackColor = fieldColor;
				return;
				}

			if (Player)
				{
				CompScoresBar.BackColor = fieldColor;
				PlayerScoresBar.BackColor = textColor;
				}
			else
				{
				CompScoresBar.BackColor = textColor;
				PlayerScoresBar.BackColor = fieldColor;
				}
			}

		// Вспомогательная функция. Выполняет отрисовку руки на столе
		private void DrawPlayersCardsHand (CardsHand Hand, DataGridView Field, bool Visible)
			{
			// Сброс
			Field.Columns.Clear ();

			// Добавление строк и столбцов (строка вмещает 10 карт)
			for (int i = 0; i < 10; i++)
				Field.Columns.Add ("C" + i.ToString (), "C" + i.ToString ());

			// Число строк должно быть минимально необходимым
			for (int i = 0; i < (int)Math.Ceiling (Hand.HandSize / 10.0f); i++)
				{
				Field.Rows.Add ();
				Field.Rows[Field.Rows.Count - 1].Height = 90;
				}

			// Сортировка колоды перед отрисовкой
			CardsHand hand = new CardsHand (Hand);
			hand.Sort ();

			// Заполнение
			int p;
			for (p = 0; p < Hand.HandSize; p++)
				{
				Card card = hand.MakeMove (0);

				// Отображение в открытую
				if (Visible)
					{
					// Установка надписи
					Field.Rows[(int)(p / 10)].Cells[p % 10].Value = CardSuitsClass.SuitRepresentation (card.CardSuit) +
						CardValuesClass.ValueRepresentation (card.CardValue);

					// Настройка стиля
					Field.Rows[(int)(p / 10)].Cells[p % 10].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
					Field.Rows[(int)(p / 10)].Cells[p % 10].Style.Font = new Font ("Times New Roman", 20);
					if ((card.CardSuit == CardSuits.Peaks) || (card.CardSuit == CardSuits.Clubs))
						{
						Field.Rows[(int)(p / 10)].Cells[p % 10].Style.ForeColor = blackColor;
						Field.Rows[(int)(p / 10)].Cells[p % 10].Style.SelectionBackColor = blackColor;
						}
					else
						{
						Field.Rows[(int)(p / 10)].Cells[p % 10].Style.ForeColor = redColor;
						Field.Rows[(int)(p / 10)].Cells[p % 10].Style.SelectionBackColor = redColor;
						}
					}

				// Отображение в закрытую
				else
					{
					Field.Rows[(int)(p / 10)].Cells[p % 10].Style.BackColor = cardBackColor;
					Field.Rows[(int)(p / 10)].Cells[p % 10].Style.SelectionBackColor = cardBackColor;
					}
				}

			// Дозаполнение остальных ячеек
			while (p % 10 != 0)
				{
				Field.Rows[(int)(p / 10)].Cells[p % 10].Style.BackColor = fieldColor;
				Field.Rows[(int)(p / 10)].Cells[p % 10].Style.SelectionBackColor = fieldColor;
				p++;
				}
			}

		// Вспомогательная функция. Выполняет отрисовку текущей карты
		private void DrawCurrentCard (Card SomeCard)
			{
			CurCard.Text = CardSuitsClass.SuitRepresentation (SomeCard.CardSuit) +
				CardValuesClass.ValueRepresentation (SomeCard.CardValue);

			if ((SomeCard.CardSuit == CardSuits.Peaks) || (SomeCard.CardSuit == CardSuits.Clubs))
				CurCard.ForeColor = blackColor;
			else
				CurCard.ForeColor = redColor;

			PackLabel.Text = pack.PackSize.ToString () + " / " + pack.RemovedSize.ToString ();
			}

		// Вспомогательная функция. Выполняет отрисовку очков
		private void DrawScores ()
			{
			// Запрос числа побед
			if (compWins == uint.MaxValue)
				{
				try
					{
					playersWins = uint.Parse (RDGenerics.GetAppSettingsValue ("PlWins"));
					compWins = uint.Parse (RDGenerics.GetAppSettingsValue ("PCWins"));
					}
				catch
					{
					playersWins = compWins = 0;
					}
				}

			// Обновление
			CompScoresBar.Text = "(" + compWins.ToString () + ") " +
				Localization.GetText ("PCScore") + compPlayer.Scores.ToString ();
			PlayerScoresBar.Text = "(" + playersWins.ToString () + ") " +
				Localization.GetText ("PlayerScore") + player.Scores.ToString ();
			}

		// Выполнение хода
		private void ExecuteSomeMove ()
			{
			// Ходит игрок
			if (PlayerField.Enabled)
				{
				// Если карта не выбрана (выбрана какая-то левая ячейка)
				if (PlayerField.SelectedCells[0].RowIndex * 10 + PlayerField.SelectedCells[0].ColumnIndex >=
					player.Hand.HandSize)
					{
					/*MessageBox.Show ("Не выбрана карта для выполнения хода", ProgramDescription.AssemblyTitle,
						MessageBoxButtons.OK, MessageBoxIcon.Information);*/
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning, "CardNotSelected");
					return;
					}

				// Получение выбранной карты
				player.Hand.Sort ();    // Сортировка нужна, чтобы видимое совпадало с тем, что есть в объекте CardsHand
				CardsHand hand = new CardsHand (player.Hand);
				Card card = hand.MakeMove ((uint)(PlayerField.SelectedCells[0].RowIndex * 10 +
					PlayerField.SelectedCells[0].ColumnIndex));

				// Проверка на возможность покрытия
				if (!GameRules.CanCover (currentCard, card))
					{
					/*MessageBox.Show ("Выбранная карта не подходит для выполнения хода", ProgramDescription.AssemblyTitle,
						MessageBoxButtons.OK, MessageBoxIcon.Information);*/
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning, "CardIsNotSuitable");
					return;
					}

				// Выбор масти, если требуется
				if (card.CardValue == CardValues.Queen)
					{
					if (player.Hand.HandSize != 1)  // Если дама - последняя в руке, выбирается её собственная масть
						{
						SuitSelector ss = new SuitSelector (suitSelectorColors);
						playerRequestedSuit = ss.SelectedSuit;
						ss.Dispose ();
						}
					else
						{
						playerRequestedSuit = card.CardSuit;    // Это упрощает подсчёт очков
						}
					}

				// Если ход допустим (выполнение дошло доселе)
				currentCard = player.MakeManualMove (pack, currentCard, card, playerRequestedSuit);

				// Если карту не потребуется покрыть
				if (GameRules.CoveringNotNeeded (card))
					{
					PlayerField.Enabled = false;
					TakeCard.Enabled = false;

					CompPlayerTimer.Enabled = true;

					SetBarState (false, false);
					/*CompScoresBar.BackColor = SkipTurn.ForeColor;
					PlayerScoresBarBackColor = CompPlayer.BackgroundColor;*/
					}

				// В противном случае
				else
					{
					card = compPlayer.MakeMove (pack, currentCard);
					if (!currentCard.Equals (card))
						throw new Exception ("Critical: player answer for should-cover-card should" +
							" not return new card, point 7");

					DrawPlayersCardsHand (compPlayer.Hand, CompPlayer, false);

					TakeCard.Enabled = true;
					}
				SkipTurn.Enabled = false;

				// Перерисовка
				DrawPlayersCardsHand (player.Hand, PlayerField, true);
				DrawCurrentCard (currentCard);
				}

			// Ходит компьютер
			else
				{
				// Выполнение собственно хода
				currentCard = compPlayer.MakeMove (pack, currentCard);

				// Если карту не потребуется покрыть
				if (GameRules.CoveringNotNeeded (currentCard) || currentCard.Answered)
					{
					PlayerField.Enabled = true;
					TakeCard.Enabled = true;

					CompPlayerTimer.Enabled = false;

					SetBarState (true, false);
					/*CompScoresBar.BackColor = CompPlayer.BackgroundColor;
					PlayerScoresBarBackColor = SkipTurn.ForeColor;*/
					}
				// В противном случае
				else
					{
					Card card = player.MakeManualMove (pack, currentCard, null, playerRequestedSuit);
					if (!currentCard.Equals (card))
						throw new Exception ("Critical: player answer for should-cover-card should not" +
							" return new card, point 8");

					DrawPlayersCardsHand (player.Hand, PlayerField, true);
					}

				// Перерисовка
				DrawPlayersCardsHand (compPlayer.Hand, CompPlayer, false);
				DrawCurrentCard (currentCard);
				}

			// Проверка на выигрыш
			TestWins ();
			}

		// Метод проверяет наличие выигрышей
		private void TestWins ()
			{
			if ((compPlayer.Hand.HandSize * player.Hand.HandSize == 0) && (currentCard.CardValue != CardValues.Eight))
				{
				// Остановка автоматики
				CompPlayerTimer.Enabled = false;
				SetBarState (true, true);
				/*CompScoresBar.BackColor =
					PlayerScoresBarBackColor = CompPlayer.BackgroundColor;*/

				// Победил игрок
				if (player.Hand.HandSize == 0)
					{
					// Отрисовка карт противника в открытую
					DrawPlayersCardsHand (compPlayer.Hand, CompPlayer, true);
					}

				// Обновление очков
				compPlayer.UpdateScores (currentCard);
				player.UpdateScores (currentCard);
				DrawScores ();

				// Дополнительное условие
				if (compPlayer.Scores == 101)
					compPlayer.ZeroScores ();
				if (player.Scores == 101)
					player.ZeroScores ();

				// Если вся игра завершена
				if ((compPlayer.Scores > 101) || (player.Scores > 101))
					{
					if (compPlayer.Scores > 101)
						{
						RDGenerics.LocalizedMessageBox (RDMessageTypes.Success, "WinMessage");
						playersWins++;
						}
					else
						{
						RDGenerics.LocalizedMessageBox (RDMessageTypes.Error, "LoseMessage");
						compWins++;
						}

					/*	{
						MessageBox.Show ("ВЫ ПОБЕДИЛИ В ЭТОЙ ИГРЕ!!!", ProgramDescription.AssemblyTitle,
							MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					else
						{
						MessageBox.Show ("Игра проиграна", ProgramDescription.AssemblyTitle,
							MessageBoxButtons.OK, MessageBoxIcon.Information);
						}*/

					RestartGame ();
					return;
					}

				// Сообщение
				if (player.Hand.HandSize == 0)
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Success, "WinRoundMessage");
				else
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Error, "LoseRoundMessage");

				/*{
				MessageBox.Show ("Вы победили в этом раунде!", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
				MessageBox.Show ("Вы проиграли в этом раунде", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				}*/

				// Сброс состояния
				StartNewRound ();
				}
			}

		// Добор карт
		private void GetCard_Click (object sender, EventArgs e)
			{
			if (!PlayerField.Enabled)
				return;

			TakeCard.Enabled = false;

			currentCard.AnswerCard ();
			player.MakeManualMove (pack, currentCard, null, playerRequestedSuit);

			DrawPlayersCardsHand (player.Hand, PlayerField, true);

			SkipTurn.Enabled = true;
			}

		// Пропуск хода
		private void LeftNextMove_Click (object sender, EventArgs e)
			{
			if (!PlayerField.Enabled)
				return;

			if (currentCard.CardValue == CardValues.Eight)
				{
				/*MessageBox.Show ("Восьмёрку нужно покрыть", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Information);*/
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning, "EightShouldBeCovered");
				return;
				}

			PlayerField.Enabled = false;
			SkipTurn.Enabled = false;

			CompPlayerTimer.Enabled = true;

			SetBarState (false, false);
			/*CompScoresBar.BackColor = SkipTurn.ForeColor;
			PlayerScoresBarBackColor = CompPlayer.BackgroundColor;*/
			}

		// Выполняет ходы за компьютер
		private void CompPlayerTimer_Tick (object sender, EventArgs e)
			{
			ExecuteSomeMove ();
			}

		// Выбор карты и попытка хода
		private void PlayerField_CellDoubleClick (object sender, DataGridViewCellEventArgs e)
			{
			ExecuteSomeMove ();
			}

		// Выход из программы
		private void MExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			/*if (MessageBox.Show ("Завершить игру?", ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo,
				 MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{
				this.Close ();
				}*/

			if (RDGenerics.LocalizedMessageBox (RDMessageTypes.Question, "FinishGame", LzDefaultTextValues.Button_Yes,
				LzDefaultTextValues.Button_No) == RDMessageButtons.ButtonTwo)
				e.Cancel = true;
			}

		// Отображение справки по программе
		private void MHelp_Click (object sender, EventArgs e)
			{
			/*HelpForm hf = new HelpForm ();*/
			RDGenerics.ShowAbout (false);
			}

		// Перезапуск игры
		private void RestartGame ()
			{
			// Сброс состояний
			compPlayer = null;
			player = null;
			pack = null;

			// Сохранение побед
			RDGenerics.SetAppSettingsValue ("PlWins", playersWins.ToString ());
			RDGenerics.SetAppSettingsValue ("PCWins", compWins.ToString ());

			// Запуск
			StartNewRound ();
			}

		// Выбор языка
		private void MLanguage_Click (object sender, EventArgs e)
			{
			if (RDGenerics.MessageBox ())
				LocalizeForm ();
			}
		}
	}
