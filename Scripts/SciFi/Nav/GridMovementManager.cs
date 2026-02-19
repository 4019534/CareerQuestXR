using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class GridMovementManager : MonoBehaviour
{
    public static GridMovementManager Instance;
    public Canvas gameboard;
    public GameObject levelIntroCanvas;
    public TMP_Text instructionTextUI;
    public TMP_Text instructionsText;
    public TMP_Text instructionsText2;
    private int numberOfSteps = 0;
    public int wrongSteps = 0;
    public GameObject tilePrefab;
    public GameObject buttonPrefab;
    public Transform player;
    public Vector3 gridStartPosition = new Vector3(-25.8f, 0.25f, -32.2f);
    public Vector3 buttonStartPosition = new Vector3(-66f, 313f, 0f);
    public float rowSpacing = 1f;
    public float colSpacing = 1f;
    public Transform gridParent;
    public Transform buttonsParent;
    private GridTile[,] grid;
    private ButtonGridTile[,] buttonGrid;
    private int currentRow, currentCol;
    public int currentLevelIndex = -1;
    public List<LevelRule> levels = new List<LevelRule>();
    private LevelRule currentLevel;
    private string currentUsername;
    private ButtonGridTile currentButtonTile;

    void Awake() => Instance = this;

    void Start()
    {
        currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        currentLevelIndex = PlayerPrefs.GetInt($"{currentUsername}_LevelCompleted", -1);
        InitializeLevels(); 
        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= 10)
        {
            MainSceneLogic.Instance.ShowSurvey();
            return;
        }

        currentLevel = levels[currentLevelIndex];
        instructionTextUI.text = currentLevel.instructionText;
        instructionsText.text = $"Level:{currentLevelIndex + 1}";
        instructionsText2.text = instructionTextUI.text;
        MainSceneLogic.Instance.gameStarted = false;
        levelIntroCanvas.gameObject.SetActive(true);

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (Transform child in buttonsParent)
            Destroy(child.gameObject);
    }
    public void OnStartLevelButton()
    {
        levelIntroCanvas.gameObject.SetActive(false);

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (Transform child in buttonsParent)
            Destroy(child.gameObject);

        BuildGrid(currentLevel.tileValues);
        PlacePlayer(currentLevel.startTile);
        HighlightValidTiles();

        
        MainSceneLogic.Instance.attempts = 1;
        MainSceneLogic.Instance.attemptsText.text = $"Attempts: {MainSceneLogic.Instance.attempts}";

        MainSceneLogic.Instance.timer = 0f;
        MainSceneLogic.Instance.timerText.text = "Time: 0.0s";
        MainSceneLogic.Instance.gameStarted = true;

    }

    void BuildGrid(int[,] values)
    {
        int rows = values.GetLength(0);
        int cols = values.GetLength(1);
        grid = new GridTile[rows, cols];
        buttonGrid = new ButtonGridTile[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = gridStartPosition + new Vector3(c * colSpacing, 0, r * rowSpacing);
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity);
                GridTile tile = tileObj.GetComponent<GridTile>();
                tile.transform.SetParent(gridParent);
                tile.Init(r, c, this);
                tile.tileValue = values[r, c];
                tile.UpdateLabel();
                grid[r, c] = tile;


                GameObject newButton = Instantiate(buttonPrefab);
                ButtonGridTile buttonTile = newButton.GetComponent<ButtonGridTile>();
                buttonTile.transform.SetParent(buttonsParent, false);
                buttonTile.Init(r, c, this, tile);
                buttonTile.tileValue = values[r, c];
                buttonTile.UpdateLabel();
                buttonGrid[r, c] = buttonTile;
            }
        }
        HighlightValidTiles();
    }

    public ButtonGridTile GetButtonTile(int row, int col)
    {
        if (buttonGrid != null && row >= 0 && row < buttonGrid.GetLength(0) && col >= 0 && col < buttonGrid.GetLength(1))
        {
            return buttonGrid[row, col];
        }
        return null;
    }

    public void TryMoveToTile(int row, int col)
    {
        int dr = Mathf.Abs(row - currentRow);
        int dc = Mathf.Abs(col - currentCol);

        if (dr + dc == 1 && currentLevel.ruleFunc(grid[row, col]))
        {
            PlacePlayer(new Vector2Int(row, col));
            numberOfSteps++;
            
            if (row == currentLevel.targetTile.x && col == currentLevel.targetTile.y)
            {        
                MainSceneLogic.Instance.RecordLevelResults(currentLevelIndex, MainSceneLogic.Instance.timer, MainSceneLogic.Instance.attempts, numberOfSteps, wrongSteps);
                numberOfSteps = 0;
                wrongSteps = 0;
                LoadNextLevel();
            }
        }
        else
        {
            grid[row, col].FlashError();
        }

    }

    void PlacePlayer(Vector2Int gridPos)
    {
        if (currentButtonTile != null)
        {
            currentButtonTile.SetCurrentPosition(false);
        }

        currentRow = gridPos.x;
        currentCol = gridPos.y;

        Vector3 pos = grid[currentRow, currentCol].transform.position;
        player.position = new Vector3(pos.x + 6.5f, pos.y + 5.5f, pos.z + 6.5f);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        currentButtonTile = buttonGrid[currentRow, currentCol];
        if (currentButtonTile != null)
        {
            currentButtonTile.SetCurrentPosition(true);
        }

        HighlightValidTiles();
    }

    public void ResetPlayerPosition()
    {
        if (currentLevel == null)
        {
            return;
        }

        PlacePlayer(currentLevel.startTile);
        numberOfSteps = 0;
        wrongSteps = 0;
    }


    public void HighlightValidTiles()
    {
        int[,] dirs = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };


        foreach (var tile in grid)
            tile?.SetHighlight(false);

        for (int r = 0; r < buttonGrid.GetLength(0); r++)
        {
            for (int c = 0; c < buttonGrid.GetLength(1); c++)
            {
                buttonGrid[r, c]?.SetHighlight(false);
            }
        }

        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int r = currentRow + dirs[i, 0];
            int c = currentCol + dirs[i, 1];

            if (r >= 0 && r < buttonGrid.GetLength(0) && c >= 0 && c < buttonGrid.GetLength(1))
            {
                ButtonGridTile buttonTile = buttonGrid[r, c];

                if (currentLevel.buttonRuleFunc(buttonTile))
                {
                    buttonTile.SetHighlight(true);

                }
            }


            if (r >= 0 && r < grid.GetLength(0) && c >= 0 && c < grid.GetLength(1))
            {
                GridTile tile = grid[r, c];
                ButtonGridTile buttonTile = buttonGrid[r, c];

                if (currentLevel.ruleFunc(tile))
                {
                    tile.SetHighlight(true);
                    buttonTile.SetHighlight(true);
                }
            }
        }

        HighlightTargetTile();
    }

    private void HighlightTargetTile()
    {
        Vector2Int target = currentLevel.targetTile;

        if (target.x < 0 || target.x >= grid.GetLength(0)) return;
        if (target.y < 0 || target.y >= grid.GetLength(1)) return;

        GridTile tile = grid[target.x, target.y];
        ButtonGridTile buttonTile = buttonGrid[target.x, target.y];

        tile.SetTargetHighlight(true);
        buttonTile.SetTargetHighlight(true);
    }

    void InitializeLevels()
    {
        levels = new List<LevelRule>();

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are multiples of 4.",
            startTile = new Vector2Int(0, 0),
            targetTile = new Vector2Int(0, 1),
            tileValues = new int[6, 6] {
                { 4, 8, 3, 9, 5, 6 },
                { 12, 4, 7, 3, 1, 5 },
                { 3, 8, 1, 2, 3, 9 },
                { 7, 5, 3, 5, 6, 7 },
                { 1, 6, 3, 2, 5, 9 },
                { 1, 6, 3, 2, 5, 9 }
            },
            ruleFunc = tile => tile.tileValue % 4 == 0,
            buttonRuleFunc = buttonTile => buttonTile.tileValue % 4 == 0
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are multiples of 3.",
            startTile = new Vector2Int(0, 0),
            targetTile = new Vector2Int(5, 5),
            tileValues = new int[6, 6] {
                { 9, 6, 12, 3, 2, 1 },
                { 12, 3, 9, 6, 5, 7 },
                { 3, 9, 6, 12, 15, 2 },
                { 5, 18, 3, 9, 6, 18 },
                { 1, 9, 6, 12, 3, 9 },
                { 3, 6, 9, 15, 12, 15 }

            },
            ruleFunc = tile => tile.tileValue % 3 == 0,
            buttonRuleFunc = buttonTile => buttonTile.tileValue % 3 == 0
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are prime numbers.",
            startTile = new Vector2Int(0, 2),
            targetTile = new Vector2Int(5, 3),
            tileValues = new int[6, 6] {
                { 6, 4, 2, 3, 5, 10 },
                { 1, 5, 3, 7, 11, 4 },
                { 7, 13, 2, 11, 17, 8 },
                { 19, 3, 5, 13, 5, 14 },
                { 4, 2, 7, 3, 2, 6 },
                { 10, 2, 1, 2, 3, 11 }
            },
            ruleFunc = tile => IsPrime(tile.tileValue),
            buttonRuleFunc = buttonTile => IsPrime(buttonTile.tileValue)
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are even numbers.",
            startTile = new Vector2Int(1, 1),
            targetTile = new Vector2Int(4, 4),
            tileValues = new int[6, 6] {
                { 3, 2, 5, 9, 8, 11 },
                { 4, 6, 8, 2, 10, 7 },
                { 1, 10, 2, 6, 4, 3 },
                { 5, 8, 4, 10, 6, 9 },
                { 3, 2, 6, 8, 14, 13 },
                { 7, 3, 4, 6, 2, 10 }
            },
            ruleFunc = tile => tile.tileValue % 2 == 0,
            buttonRuleFunc = buttonTile => Math.Sqrt(buttonTile.tileValue) % 2 == 0
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are multiples of 5.",
            startTile = new Vector2Int(0, 0),
            targetTile = new Vector2Int(5, 2),
            tileValues = new int[6, 6] {
                { 5, 10, 15, 20, 25, 12 },
                { 20, 25, 30, 5, 6, 5 },
                { 15, 5, 10, 5, 20, 7 },
                { 6, 5, 15, 10, 25, 9 },
                { 2, 20, 10, 5, 35, 4 },
                { 1, 5, 25, 30, 15, 30 }
            },
            ruleFunc = tile => tile.tileValue % 5 == 0,
            buttonRuleFunc = buttonTile => Math.Sqrt(buttonTile.tileValue) % 5 == 0
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are perfect squares.",
            startTile = new Vector2Int(2, 0),
            targetTile = new Vector2Int(0, 5),
            tileValues = new int[6, 6] {
                { 1, 4, 9, 4, 16, 9 },
                { 4, 1, 9, 16, 25, 4 },
                { 25, 36, 9, 16, 1, 4 },
                { 49, 4, 64, 1, 9, 16 },
                { 9, 4, 1, 16, 25, 36 },
                { 16, 9, 4, 1, 49, 1 }
            },
            ruleFunc = tile => Math.Sqrt(tile.tileValue) % 1 == 0,
            buttonRuleFunc = buttonTile => Math.Sqrt(buttonTile.tileValue) % 1 == 0
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles that are odd numbers.",
            startTile = new Vector2Int(0, 0),
            targetTile = new Vector2Int(5, 5),
            tileValues = new int[6, 6] {
                { 1, 3, 5, 7, 4, 9 },
                { 5, 7, 9, 11, 6, 13 },
                { 2, 9, 11, 15, 17, 8 },
                { 4, 11, 13, 17, 19, 21 },
                { 6, 15, 17, 19, 21, 23 },
                { 8, 3, 5, 21, 23, 25 }
            },
            ruleFunc = tile => tile.tileValue % 2 == 1,
            buttonRuleFunc = buttonTile => buttonTile.tileValue % 2 == 1
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on tiles greater than 10.",
            startTile = new Vector2Int(3, 0),
            targetTile = new Vector2Int(2, 5),
            tileValues = new int[6, 6] {
                { 5, 8, 12, 15, 18, 20 },
                { 9, 11, 14, 16, 20, 22 },
                { 13, 15, 17, 19, 21, 24 },
                { 11, 15, 18, 22, 23, 9 },
                { 6, 12, 14, 16, 25, 27 },
                { 4, 9, 11, 13, 15, 7 }
            },
            ruleFunc = tile => tile.tileValue > 10,
            buttonRuleFunc = buttonTile => buttonTile.tileValue > 10
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on multiples of 7.",
            startTile = new Vector2Int(0, 5),
            targetTile = new Vector2Int(5, 0),
            tileValues = new int[6, 6] {
                { 3, 5, 14, 21, 28, 7 },
                { 8, 7, 14, 35, 42, 14 },
                { 2, 14, 21, 28, 49, 21 },
                { 5, 28, 35, 42, 56, 28 },
                { 9, 42, 49, 56, 63, 35 },
                { 7, 14, 21, 28, 35, 14 }
            },
            ruleFunc = tile => tile.tileValue % 7 == 0,
            buttonRuleFunc = buttonTile => buttonTile.tileValue % 7 == 0
        });

        levels.Add(new LevelRule
        {
            instructionText = "Step only on powers of 2.",
            startTile = new Vector2Int(2, 2),
            targetTile = new Vector2Int(5, 4),
            tileValues = new int[6, 6] {
                { 3, 5, 7, 9, 11, 13 },
                { 6, 8, 16, 32, 12, 15 },
                { 10, 4, 8, 16, 64, 17 },
                { 14, 2, 4, 8, 32, 19 },
                { 18, 16, 32, 64, 128, 21 },
                { 22, 24, 26, 28, 16, 23 }
            },
            ruleFunc = tile => IsPowerOfTwo(tile.tileValue),
            buttonRuleFunc = buttonTile => IsPowerOfTwo(buttonTile.tileValue)
        });
    }

    static bool IsPrime(int number)
    {
        if (number < 2) return false;
        for (int i = 2; i <= Mathf.Sqrt(number); i++)
        {
            if (number % i == 0) return false;
        }
        return true;
    }

    static bool IsFibonacci(int n)
    {
        int[] fibs = { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };
        return System.Array.IndexOf(fibs, n) >= 0;
    }

    static bool IsPowerOfTwo(int n)
    {
        return n > 0 && (n & (n - 1)) == 0;
    }
}

[System.Serializable]
public class LevelRule
{
    public string instructionText;
    public Vector2Int startTile;
    public Vector2Int targetTile;
    public int[,] tileValues;
    public System.Func<GridTile, bool> ruleFunc;
    public System.Func<ButtonGridTile, bool> buttonRuleFunc;
}


