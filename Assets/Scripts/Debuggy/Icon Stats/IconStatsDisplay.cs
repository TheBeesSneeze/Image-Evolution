using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class IconStatsDisplay : MonoBehaviour
{
	[Header("Most used")] 
	[SerializeField] private Button mostUsedButton;
    [SerializeField] private Image mostUsedImage;
    [SerializeField] private TMP_Text mostUsedText;

    [Header("Least used")]
    [SerializeField] private Button leastUsedButton;
	[SerializeField] private Image leastUsedImage;
    [SerializeField] private TMP_Text leastUsedText;

	private Dictionary<Sprite, int> icon_uses;

    // Start is called before the first frame update
    void Start()
    {
		icon_uses = new Dictionary<Sprite, int>();
	    foreach (Sprite sprite in ShapeManager.Instance.shapeSprites)
		    icon_uses.Add(sprite,0);

	    ShapeManager.OnShapeCreated += OnShapeCreated;

		mostUsedButton.onClick.AddListener(MostUsedButtonPressed);
		leastUsedButton.onClick.AddListener(LeastUsedButtonPressed);
    }

    void OnShapeCreated(Shape newShape)
    {
	    if (newShape == null)
		    return;

	    var icon = newShape.sprite.sprite;
		if(icon_uses.ContainsKey(icon))
		    icon_uses[icon] += 1;

		UpdateDisplay();
    }

    public void UpdateDisplay()
    {
	    var tup = GetMostUsed_LeastUsed();
	    var mostUsed = tup.Item1;
	    var leastUsed = tup.Item2;

	    mostUsedImage.sprite = mostUsed.Key;
	    mostUsedText.text = "Uses: "+mostUsed.Value;

	    leastUsedImage.sprite = leastUsed.Key;
	    leastUsedText.text = "Uses: " + leastUsed.Value;
	}

    public (KeyValuePair<Sprite,int>, KeyValuePair<Sprite, int>) GetMostUsed_LeastUsed()
    {
	    var mostUsed = icon_uses.FirstOrDefault();
	    var leastUsed = icon_uses.FirstOrDefault();


		foreach (var icon_use_pair in icon_uses)
	    {
		    if (icon_use_pair.Value > mostUsed.Value)
			    mostUsed = icon_use_pair;

			if (icon_use_pair.Value < leastUsed.Value)
			    leastUsed = icon_use_pair;
	    }

		return (mostUsed,leastUsed);
    }

	#region Hide Icon

	public void MostUsedButtonPressed()
	{
		var mostUsed = GetMostUsed_LeastUsed().Item1.Key;
		ShapeManager.Instance.shapeSprites.Remove(mostUsed);
		RemoveIcon(mostUsed);
	}

	public void LeastUsedButtonPressed()
	{
		var leastUsed = GetMostUsed_LeastUsed().Item2.Key;
		ShapeManager.Instance.shapeSprites.Remove(leastUsed);
		RemoveIcon(leastUsed);
	}

	private void RemoveIcon(Sprite icon)
	{
		icon_uses.Remove(icon);
		UpdateDisplay();
	}

	#endregion

	[Button]
    public void PrintIconStats()
    {
	    foreach (var icon_use_pair in icon_uses)
	    {
		    Debug.Log(icon_use_pair.Value + " : " + icon_use_pair.Key);
	    }
	}
}
