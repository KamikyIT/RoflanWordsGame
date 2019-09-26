using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevelopeSettingsMenu : MonoBehaviour, IDevelopeGameSettings
{
	[SerializeField]
	Button _saveButton;

	[SerializeField]
	Button _loadButton;

	[SerializeField]
	Button _runButton;

	[SerializeField]
	Slider _rowsSlider;
	[SerializeField]
	Slider _columnsSlider;

	[SerializeField]
	Slider _fieldWidthSlider;
	[SerializeField]
	Slider _fieldHeightSlider;

	[SerializeField]
	RectTransform _testFieldDisplayPanel;


	void Start()
	{
		Load();
		
		_saveButton.onClick.AddListener(Save);
		_loadButton.onClick.AddListener(Load);
		_runButton.onClick.AddListener(Run);

		_rowsSlider.onValueChanged.AddListener((v) => RowsCount = (int)v);
		_columnsSlider.onValueChanged.AddListener((v) => ColumnsCount = (int)v);

		_fieldWidthSlider.onValueChanged.AddListener((v) =>
		{
			FieldWidthPart = v;

			DisplayMiniFieldPartsImage();
		});
		_fieldHeightSlider.onValueChanged.AddListener((v) => {
			FieldHeightPart = v;

			DisplayMiniFieldPartsImage();
		});


		DisplayCurrentSettings();
	}

	void DisplayMiniFieldPartsImage()
	{
		var dx = (1f - FieldWidthPart) / 2f;
		var dy = (1f - FieldHeightPart) / 2f;
		_testFieldDisplayPanel.anchorMin = new Vector2(dx, dy);
		_testFieldDisplayPanel.anchorMax = new Vector2(dx + FieldWidthPart, dy + FieldHeightPart);
	}

	void DisplayCurrentSettings()
	{
		_rowsSlider.value = RowsCount;
		_columnsSlider.value = ColumnsCount;

		_fieldWidthSlider.value = FieldWidthPart;
		_fieldHeightSlider.value = FieldHeightPart;

		DisplayMiniFieldPartsImage();
	}

	#region IDevelopeGameSettings

	public int RowsCount { get; set; }
	public int ColumnsCount { get; set; }

	public float FadeTime { get; set; }

	public float FieldWidthPart { get; set; }
	public float FieldHeightPart { get; set; }

	public void Save()
	{
		PlayerPrefs.SetInt("RowsCount", RowsCount);
		StaticGameSettings.RowsCount = RowsCount;
		PlayerPrefs.SetInt("ColumnsCount", ColumnsCount);
		StaticGameSettings.ColumnsCount = ColumnsCount;

		PlayerPrefs.SetFloat("FadeTime", FadeTime);
		StaticGameSettings.FadeTime = FadeTime;

		PlayerPrefs.SetFloat("FieldWidthPart", FieldWidthPart);
		StaticGameSettings.FieldWidthPart = FieldWidthPart;
		PlayerPrefs.SetFloat("FieldHeightPart", FieldHeightPart);
		StaticGameSettings.FieldHeightPart = FieldHeightPart;

		PlayerPrefs.Save();
	}

	public void Load()
	{
		RowsCount = StaticGameSettings.RowsCount;
		ColumnsCount = StaticGameSettings.ColumnsCount;

		FadeTime = PlayerPrefs.GetFloat("FadeTime", 1f);

		FieldWidthPart = StaticGameSettings.FieldWidthPart;
		FieldHeightPart = StaticGameSettings.FieldHeightPart;

		DisplayCurrentSettings();
	}

	public void Run()
	{
		Save();

		SceneManager.LoadSceneAsync("game_scene");
	}

	#endregion
}

public static class StaticGameSettings
{
	static StaticGameSettings()
	{
		ReadFromPlayerPrefs();
	}

	public static int RowsCount { get; set; }
	public static int ColumnsCount { get; set; }

	public static float FieldWidthPart { get; set; }
	public static float FieldHeightPart { get; set; }

	public static float FadeTime { get; set; }


	public static void ReadFromPlayerPrefs()
	{
		RowsCount = PlayerPrefs.GetInt("RowsCount", 5);
		ColumnsCount = PlayerPrefs.GetInt("ColumnsCount", 5);

		FadeTime = PlayerPrefs.GetFloat("FadeTime", 1f);

		FieldWidthPart = PlayerPrefs.GetFloat("FieldWidthPart", 0.70f);
		FieldHeightPart = PlayerPrefs.GetFloat("FieldHeightPart", 0.70f);
	}
}

public interface IDevelopeGameSettings
{
	int RowsCount { get; set; }
	int ColumnsCount { get; set; }

	float FadeTime { get; set; }

	float FieldWidthPart { get; set; }
	float FieldHeightPart { get; set; }

	void Save();

	void Load();

	void Run();
}
