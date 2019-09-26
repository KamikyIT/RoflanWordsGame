using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameFieldCell : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler
{
	[SerializeField]
	Text _cellText;

	[SerializeField]
	Image _cellBackgroundImage;

	[SerializeField]
	Button _button;

	FieldCellModel _model;

	#region IPointerDownHandler

	public void OnPointerDown(PointerEventData eventData)
	{
		_model.MouseEntered();
	}

	#endregion

	#region IPointerEnterHandler

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Input.GetMouseButton(0))
			_model.MouseEntered();
	}

	#endregion

	#region IPointerExitHandler

	public void OnPointerExit(PointerEventData eventData)
	{
		if (Input.GetMouseButton(0))
			_model.MouseLeft();
	}

	#endregion

	public void InitializeWithModel(FieldCellModel model)
	{
		this._model = model;

		_cellText.text = _model.Char.ToString().ToUpper();

		_cellBackgroundImage.color = GetColorByPrice(_model.Price);
	}

	Color GetColorByPrice(int price)
	{
		switch (price)
		{
			case 1:
				return new Color(23f / 255f, 141f/255f, 92f/255f);

			case 2:
				return new Color(219f / 255f, 152f / 255f, 46f / 255f);

			case 3:
				return new Color(219f / 255f, 83f / 255f, 46f / 255f);
			default:

				return Color.red;
		}
	}
}

public class FieldCellModel
{
	public char Char { get; private set; }

	public int Price { get; private set; }

	public int I { get; private set; }

	public int J { get; private set; }

	public FieldCellModel(char character, int price, int i, int j)
	{
		Char = character;
		Price = price;
		I = i;
		J = j;
	}

	public override string ToString()
	{
		return Char + " " + (Price == 1 ? "" : Price.ToString());
	}

	public void MouseEntered()
	{
		OnMouseEntered(this);
	}

	public event Action<FieldCellModel> OnMouseEntered;

	public void MouseLeft()
	{
		OnMouseLeft(this);
	}

	public event Action<FieldCellModel> OnMouseLeft;
}


public class GameFieldModel
{
	FieldCellModel[,] _allCells;

	void MouseEnter(FieldCellModel mc) { OnMouseEntered(mc); }
	public event Action<FieldCellModel> OnMouseEntered;

	void MouseLeft(FieldCellModel mc) { OnMouseLeft(mc); }
	public event Action<FieldCellModel> OnMouseLeft;

	int _rows;
	int _columns;

	ICharPosibilityCalculator _charPosibilityCalculator;
	IStatisticCharPriceCalculator _priceCalculator;
	IFieldCellsGenerator _fieldCellsGenerator;

	public void CreateRandom(int rows, int columns)
	{
		_rows = rows;
		_columns = columns;

		SubcribtionOnCells(false);

		_allCells = _fieldCellsGenerator.GenerateField(_rows, _columns);

		SubcribtionOnCells(true);
	}

	public FieldCellModel GetCellModel(int i, int j)
	{
		return _allCells[i, j];
	}

	void SubcribtionOnCells(bool trueSubscribe_falseUnsubscribe)
	{
		if (_allCells == null || _allCells.Length == 0)
			return;

		for (int i = 0; i < _rows; i++)
			for (int j = 0; j < _columns; j++)
				if (trueSubscribe_falseUnsubscribe)
				{
					_allCells[i, j].OnMouseEntered += MouseEnter;
					_allCells[i, j].OnMouseLeft += MouseLeft;
				}
				else
				{
					_allCells[i, j].OnMouseEntered -= MouseEnter;
					_allCells[i, j].OnMouseLeft -= MouseLeft;
				}
	}

	public GameFieldModel()
	{
		_charPosibilityCalculator = new StatisticCharPosibilityCalculator();
		_priceCalculator = new StatisticCharPriceCalculator();
		_fieldCellsGenerator = new RandomByStatisticCharPosibilityFieldCellsGenerator(_charPosibilityCalculator, _priceCalculator);
	}
}


public interface IStatisticCharPriceCalculator
{
	int CalculatePrice();
}

public class StatisticCharPriceCalculator : IStatisticCharPriceCalculator
{
	public int CalculatePrice()
	{
		var rand = UnityEngine.Random.Range(1, 100);

		if (rand < 85)
			return 1;

		if (rand < 90)
			return 2;

		if (rand < 95)
			return 3;

		return 4;
	}
}

public interface IFieldCellsGenerator
{
	FieldCellModel[,] GenerateField(int rows, int columns);
}

public class RandomByStatisticCharPosibilityFieldCellsGenerator : IFieldCellsGenerator
{
	ICharPosibilityCalculator _charPosibilityCalculator;
	IStatisticCharPriceCalculator _priceCalculator;


	public RandomByStatisticCharPosibilityFieldCellsGenerator(ICharPosibilityCalculator charPosibilityCalculator, IStatisticCharPriceCalculator priceCalculator)
	{
		_charPosibilityCalculator = charPosibilityCalculator;
		_priceCalculator = priceCalculator;
	}

	public FieldCellModel[,] GenerateField(int rows, int columns)
	{
		var allCells = new FieldCellModel[rows, columns];

		for (int i = 0; i < rows; i++)
			for (int j = 0; j < columns; j++)
				allCells[i, j] = new FieldCellModel(
					character: _charPosibilityCalculator.GenerateRandomChar(),
					price: _priceCalculator.CalculatePrice(), 
					i: i, 
					j: j);

		return allCells;
	}
}

public interface ICharPosibilityCalculator
{
	char GenerateRandomChar();
}

public class StatisticCharPosibilityCalculator : ICharPosibilityCalculator
{
	long totalPoses;
	List<CharPosibility> CharPosibilities;

	public StatisticCharPosibilityCalculator()
	{
		var lines = _charPosibility.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

		totalPoses = 0L;

		CharPosibilities = new List<CharPosibility>();

		foreach (var line in lines)
		{
			var parts = line.Split(new char[] { '\t' });

			var ch = parts[0][0];

			var count = long.Parse(parts[1]);

			CharPosibilities.Add(new CharPosibility()
			{
				start = totalPoses,
				end = totalPoses + count,
				ch = ch,
			});

			totalPoses += count;
		}
	}

	struct CharPosibility
	{
		public long start;
		public long end;

		public char ch;

		public override string ToString()
		{
			return string.Format("{0} {1}-{2}", ch, start, end);
		}
	}

	#region ICharPosibilityCalculator

	public char GenerateRandomChar()
	{
		var index = (long)UnityEngine.Random.Range(0f, totalPoses);

		return CharPosibilities.Single(x => x.start <= index && index <= x.end).ch;
	}

	#endregion

	#region Char posibilities

	const string _charPosibility =
@"
о	55414481
е	42691213
а	40487008
и	37153142
н	33838881
т	31620970
с	27627040
р	23916825
в	22930719
л	22230174
к	17653469
м	16203060
д	15052118
п	14201572
у	13245712
я	10139085
ы	9595941
ь	8784613
г	8564640
з	8329904
б	8051767
ч	7300193
й	6106262
х	4904176
ж	4746916
ш	3678738
ю	3220715
ц	2438807
щ	1822476
э	1610107
ф	1335747
ъ	185452
ё	184928
";

	#endregion
}
