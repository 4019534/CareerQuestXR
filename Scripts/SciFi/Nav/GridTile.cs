using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;


public class GridTile : MonoBehaviour
{
    public static GridTile Instance;
    public int row;
    public int col;
    public TMP_Text valueLabel;
    private GridMovementManager manager;
    private Renderer rend;
    public Material errorMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material highlightTargetMaterial;
    public int tileValue;
    private bool isHighlighted = false;

    void Awake() => Instance = this;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && originalMaterial == null)
        {
            originalMaterial = rend.material;
        }
    }

    public void Init(int row, int col, GridMovementManager manager)
    {
        this.row = row;
        this.col = col;
        this.manager = manager;

        if (rend == null)
            rend = GetComponent<Renderer>();

        if (rend != null && originalMaterial == null)
        {
            originalMaterial = rend.material;
        }
    }

    public void CallingMouseDown(int row, int col, bool highlight)
    {
        OnMouseDown(row, col, highlight);
        ButtonGridTile specificButton = manager.GetButtonTile(row, col);
        if (specificButton != null)
        {
            specificButton.CallMouseDown(highlight);
        }
    }

    void OnMouseDown(int row, int col, bool highlight)
    {
        if (highlight)
        {
            manager.TryMoveToTile(row, col);
        }
        else
        {
            FlashError();
            manager.wrongSteps++;
        }
    }
    public void UpdateLabel()
    {
        if (valueLabel != null)
            valueLabel.text = tileValue.ToString();
    }

    public void FlashError()
    {
        if (rend == null || errorMaterial == null || originalMaterial == null) return;
        StartCoroutine(FlashErrorRoutine());

    }

    private IEnumerator FlashErrorRoutine()
    {
        rend.material = errorMaterial;
        yield return new WaitForSeconds(0.25f);
        rend.material = originalMaterial;
    }

    public void SetHighlight(bool highlight)
    {
        if (rend == null)
        {
            return;
        }
            
        isHighlighted = highlight;
    }

    public void SetTargetHighlight(bool enabled)
    {
        if (rend == null)
        {
            return;
        }
            
        isHighlighted = enabled;

        if (enabled && highlightTargetMaterial != null)
        {
            rend.material = highlightTargetMaterial;
        }
        else if (originalMaterial != null)
        {
            rend.material = originalMaterial;
        }
    }
}

