using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonGridTile : MonoBehaviour
{
    public int row;
    public int col;
    public TMP_Text valueLabel;
    private GridMovementManager manager;
    public GridTile linkedTile;
    private Image image;
    public Color errorColor = Color.red;
    public Color originalColor = Color.black;
    public Color highlightColor = Color.yellow;
    public Color highlightTargetColor = Color.green;
    public Color currentPositionColor = Color.blue;
    public int tileValue;
    private bool isHighlighted = false;
    private bool isCurrentPosition = false;

    public void CallGridMouseDown()
    {
        GridTile.Instance.CallingMouseDown(row, col, isHighlighted);
    }

    public void Init(int row, int col, GridMovementManager manager, GridTile linkedTile)
    {
        this.row = row;
        this.col = col;
        this.manager = manager;
        this.linkedTile = linkedTile;

        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
            originalColor = image.color;
    }

    public void CallMouseDown(bool highlight)
    {
        if (isCurrentPosition)
        {
            return; 
        }
        if (highlight)
        {
            return;
        }
        else
        {
            FlashError();           
        }
    }
    public void UpdateLabel()
    {
        if (valueLabel != null)
            valueLabel.text = tileValue.ToString();
    }


    public void FlashError()
    {
        if (image == null) return;        
        StopAllCoroutines();
        StartCoroutine(FlashErrorRoutine());
    }

    private IEnumerator FlashErrorRoutine()
    {
        image.color = errorColor;
        yield return new WaitForSeconds(0.25f);
        image.color = originalColor;
    }

    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        if (image == null)
        {
            return;
        }
    }

    public void SetTargetHighlight(bool enabled)
    {
        isHighlighted = enabled;
        if (image == null)
        {
            return;
        }
        if (enabled)
        {
            image.color = highlightTargetColor;
        }
        else
        {
            image.color = originalColor;
        }
    }

    public void SetCurrentPosition(bool isCurrent)
    {
        isCurrentPosition = isCurrent;
        
        if (image == null)
        {
            return;
        }

        if (isCurrent)
        {
            image.color = currentPositionColor;
        }
        else
        {
            if (!isHighlighted) 
            {
                image.color = originalColor;
            }
        }
    }
}
