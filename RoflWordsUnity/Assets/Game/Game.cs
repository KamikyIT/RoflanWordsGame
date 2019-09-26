using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour {

	[SerializeField]
	TextAsset allRussianWordsTextFile;

	[SerializeField]
	Text _totalScoreText;

	[SerializeField]
	Text _foundOfTotalWordsText;

	[SerializeField]
	Text _currentWordText;

	[SerializeField]
	Text _currentWordScoreText;

	[SerializeField]
	RectTransform _gameFieldRect;

	[SerializeField]
	GameFieldCell _gameFieldCellPrefab;

	[SerializeField]
	Button _reshuffleButton;

	[SerializeField]
	Button _reloadButton;

	GamePlayModel _gamePlayModel;

	GameFieldCell[,] _cellGos;

	int _rows = StaticGameSettings.RowsCount;
	int _columns = StaticGameSettings.ColumnsCount;

	void Awake()
	{
		var russianWordsChecker = new RussianWordsChecker(allRussianWordsTextFile);

		_gamePlayModel = new GamePlayModel(russianWordsChecker);

		_gamePlayModel.OnCurrentWordChanged += (cw) => { _currentWordText.text = cw; };
		_gamePlayModel.OnCurrentWordScoreChanged += (wp) => { _currentWordScoreText.text = wp.HasValue ? wp.ToString() : string.Empty; };
		_gamePlayModel.OnCurrentSessionScoreChanged += (cs) => { _totalScoreText.text = cs.ToString(); };
	}

	void Start()
	{
		SetGameFieldSizes();

		_cellGos = InstanciateGameFieldWithCells();

		FillGameFieldCellsWithModels(_cellGos);

		_reloadButton.onClick.AddListener(() => {
			_gamePlayModel.ReshuffleAndResetSession(_rows, _columns, true);
			FillGameFieldCellsWithModels(_cellGos); });

		_reshuffleButton.onClick.AddListener(() => {
			_gamePlayModel.ReshuffleAndResetSession(_rows, _columns, false);
			FillGameFieldCellsWithModels(_cellGos);
		});

	}

	void Update()
	{
		if (Input.GetMouseButtonUp(0))
			_gamePlayModel.MouseReleased();

	}

	void SetGameFieldSizes()
	{
		var dx = (1f - StaticGameSettings.FieldWidthPart) / 2f;
		var dy = (1f - StaticGameSettings.FieldHeightPart) / 2f;

		_gameFieldRect.anchorMin = new Vector2(dx, dy);
		_gameFieldRect.anchorMax = new Vector2(dx + StaticGameSettings.FieldWidthPart, dy + StaticGameSettings.FieldHeightPart);
	}

	GameFieldCell[,] InstanciateGameFieldWithCells()
	{
		var cellWidth = _gameFieldRect.rect.width / _rows;
		var cellHeight = _gameFieldRect.rect.height / _columns;

		var result = new GameFieldCell[_rows, _columns];

		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _columns; j++)
			{
				var cell = (GameFieldCell) GameObject.Instantiate(_gameFieldCellPrefab, _gameFieldRect);

				var rt = cell.GetComponent<RectTransform>();

				rt.anchorMin = new Vector2(
					((float)i + 0.5f) / (float)_rows,
					((float)j + 0.5f) / (float)_columns
					);

				rt.anchorMax = rt.anchorMin;

				rt.sizeDelta = new Vector2(cellWidth, cellHeight);

				result[i, j] = cell;
			}
		}

		return result;
	}

	void FillGameFieldCellsWithModels(GameFieldCell[,] cellGos)
	{
		_gamePlayModel.ReshuffleAndResetSession(_rows, _columns, true);

		for (int i = 0; i < _rows; i++)
			for (int j = 0; j < _columns; j++)
				cellGos[i, j].InitializeWithModel(_gamePlayModel.GetCellFieldModel(i, j));
	}
}

public class GamePlayModel
{
	public GamePlayModel(ILanguageWordsChecker wordsChecker)
	{
		_wordsChecker = wordsChecker;

		_gameField = new GameFieldModel();

		_gameField.OnMouseEntered += (mc) => TryRegister(mc);
		_gameField.OnMouseLeft += (mc) => { };

		_currentWordFieldCells = new List<FieldCellModel>();
	}

	string _currentWord;
	public string CurrentWord
	{
		get
		{
			return _currentWord;
		}
		set
		{
			_currentWord = value;
			if (OnCurrentWordChanged != null)
				OnCurrentWordChanged(_currentWord);

			if (OnCurrentWordScoreChanged != null)
				OnCurrentWordScoreChanged(CalculateCurrentWordScore());
		}
	}
	public event Action<string> OnCurrentWordChanged;

	int? CalculateCurrentWordScore()
	{
		if (string.IsNullOrEmpty(_currentWord))
			return null;

		var res = 0;

		foreach (var item in _currentWordFieldCells)
			res += 10 * item.Price;

		return res;
	}

	public event Action<int?> OnCurrentWordScoreChanged;

	int _currentSessionScore;
	public int CurrentSessionScore
	{
		get { return _currentSessionScore; }
		set
		{
			_currentSessionScore = value;
			OnCurrentSessionScoreChanged(_currentSessionScore);
		}
	}
	public event Action<int> OnCurrentSessionScoreChanged;

	GameFieldModel _gameField;

	public void ReshuffleAndResetSession(int rows, int columns, bool resetSession)
	{
		_gameField.CreateRandom(rows, columns);

		_currentWordFieldCells.Clear();

		CurrentWord = string.Empty;

		_wordsChecker.ResetSession();

		if (resetSession)
		{
			CurrentSessionScore = 0;
		}
	}

	public FieldCellModel GetCellFieldModel(int i, int j)
	{
		return _gameField.GetCellModel(i, j);
	}

	List<FieldCellModel> _currentWordFieldCells;
	void TryRegister(FieldCellModel mc)
	{
		if (!_currentWordFieldCells.Contains(mc))
		{
			var lastFc = _currentWordFieldCells.LastOrDefault();

			if (lastFc == null)
				AddToCurrentWordNewFieldCell(mc);
			else
			{
				var i = lastFc.I;
				var j = lastFc.J;

				var difI = i - mc.I;
				var difJ = j - mc.J;

				if (-1 <= difI && difI <= 1 && -1 <= difJ && difJ <= 1)
					AddToCurrentWordNewFieldCell(mc);
			}
		}
	}

	public void MouseReleased()
	{
		TryRegisterCurrentWord();
	}

	void TryRegisterCurrentWord()
	{
		if (!string.IsNullOrEmpty(CurrentWord))
		{
			if (_wordsChecker.TryRegisterNewWord(CurrentWord))
			{
				var curWordScore = CalculateCurrentWordScore();

				CurrentSessionScore += curWordScore.HasValue? curWordScore.Value : 0;
			}
		}

		_currentWordFieldCells.Clear();

		CurrentWord = string.Empty;
	}

	void AddToCurrentWordNewFieldCell(FieldCellModel mc)
	{
		_currentWordFieldCells.Add(mc);

		CurrentWord = new string(_currentWordFieldCells.Select(x => x.Char).ToArray());
	}

	ILanguageWordsChecker _wordsChecker;
}


public interface ILanguageWordsChecker
{
	bool WordExistsInLanguage(string word);

	void ResetSession();

	bool TryRegisterNewWord(string word);

	HashSet<string> CurrentSessionRegisteredWords { get; }
}

public class RussianWordsChecker : ILanguageWordsChecker
{
	public RussianWordsChecker(TextAsset allRussianWordsTextFile)
	{
		_allWords = new HashSet<string>(allRussianWordsTextFile.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

		CurrentSessionRegisteredWords = new HashSet<string>();
	}

	HashSet<string> _allWords;

	#region ILanguageWordsChecker
	
	public HashSet<string> CurrentSessionRegisteredWords { get; private set; }

	public bool TryRegisterNewWord(string word)
	{
		if (!WordExistsInLanguage(word))
			return false;

		if (CurrentSessionRegisteredWords.Contains(word))
			return false;

		CurrentSessionRegisteredWords.Add(word);

		return true;
	}

	public void ResetSession()
	{
		CurrentSessionRegisteredWords.Clear();
	}

	public bool WordExistsInLanguage(string word)
	{
		return _allWords.Contains(word);
	}

	#endregion
}
