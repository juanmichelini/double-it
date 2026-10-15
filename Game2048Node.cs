using Godot;
using System;
using Game2048;

public class Game2048Node : Node2D
{
    private Game2048.Board gameBoard;
    private Label scoreLabel;
    private Label resultLabel;
    private GridContainer gridContainer;
    private int score = 0;
    private bool gameOver = false;
    private bool gameWon = false;
    private ColorRect[,] tileRects = new ColorRect[4,4];
    private Label[,] tileLabels = new Label[4,4];

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("GridContainer");
        scoreLabel = GetNode<Label>("UI/ScoreLabel");
        resultLabel = GetNode<Label>("UI/ResultLabel");

        gameBoard = Game.emptyBoard();
        InitializeGrid();
        gameBoard = Game.sproutTiles(gameBoard);
        UpdateUI();
    }

    private void InitializeGrid()
    {
        foreach (Node child in gridContainer.GetChildren())
        {
            child.QueueFree();
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var tileContainer = new MarginContainer();
                tileContainer.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
                tileContainer.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
                tileContainer.RectMinSize = new Vector2(100, 100);

                var tileRect = new ColorRect();
                tileRect.Color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                tileContainer.AddChild(tileRect);

                var tileLabel = new Label();
                tileLabel.Align = Label.AlignEnum.Center;
                tileLabel.Valign = Label.VAlign.Center;
                tileLabel.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
                tileLabel.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
                tileLabel.AddColorOverride("font_color", Colors.Black);
                tileContainer.AddChild(tileLabel);

                gridContainer.AddChild(tileContainer);

                tileRects[i, j] = tileRect;
                tileLabels[i, j] = tileLabel;
            }
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int value = gameBoard[i, j];
                tileLabels[i, j].Text = value > 0 ? value.ToString() : "";
                tileRects[i, j].Color = GetColorForValue(value);
            }
        }

        scoreLabel.Text = $"Score: {score}";

        if (gameWon)
        {
            resultLabel.Text = "You Win! Press R to restart";
            resultLabel.Visible = true;
        }
        else if (gameOver)
        {
            resultLabel.Text = "Game Over! Press R to restart";
            resultLabel.Visible = true;
        }
        else
        {
            resultLabel.Visible = false;
        }
    }

    private Color GetColorForValue(int value)
    {
        switch (value)
        {
            case 0: return new Color(0.8f, 0.8f, 0.8f, 1.0f);
            case 1: return new Color(0.9f, 0.9f, 0.8f, 1.0f);
            case 2: return new Color(0.93f, 0.89f, 0.78f, 1.0f);
            case 4: return new Color(0.93f, 0.87f, 0.67f, 1.0f);
            case 8: return new Color(0.95f, 0.69f, 0.47f, 1.0f);
            case 16: return new Color(0.96f, 0.58f, 0.39f, 1.0f);
            case 32: return new Color(0.96f, 0.49f, 0.37f, 1.0f);
            case 64: return new Color(0.96f, 0.37f, 0.23f, 1.0f);
            case 128: return new Color(0.93f, 0.81f, 0.45f, 1.0f);
            case 256: return new Color(0.93f, 0.8f, 0.38f, 1.0f);
            case 512: return new Color(0.93f, 0.78f, 0.31f, 1.0f);
            case 1024: return new Color(0.93f, 0.77f, 0.25f, 1.0f);
            case 2048: return new Color(0.93f, 0.76f, 0.18f, 1.0f);
            default: return new Color(0.6f, 0.2f, 0.8f, 1.0f);
        }
    }

    private void CheckGameState()
    {
        if (Game.hasWon(gameBoard))
        {
            gameWon = true;
        }
        else if (!Game.hasValidMove(gameBoard))
        {
            gameOver = true;
        }
    }

    private void RestartGame()
    {
        gameBoard = Game.emptyBoard();
        gameBoard = Game.sproutTiles(gameBoard);
        score = 0;
        gameOver = false;
        gameWon = false;
        UpdateUI();
    }

    public override void _Input(InputEvent @event)
    {
        if (gameOver || gameWon)
        {
            if (@event is InputEventKey restartEvent && restartEvent.Pressed && restartEvent.Scancode == (uint)KeyList.R)
            {
                RestartGame();
            }
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            Direction direction;
            bool validKey = true;

            switch (keyEvent.Scancode)
            {
                case (uint)KeyList.Up:
                    direction = Direction.Up;
                    break;
                case (uint)KeyList.Down:
                    direction = Direction.Down;
                    break;
                case (uint)KeyList.Left:
                    direction = Direction.Left;
                    break;
                case (uint)KeyList.Right:
                    direction = Direction.Right;
                    break;
                default:
                    validKey = false;
                    direction = Direction.Left;
                    break;
            }

            if (validKey)
            {
                Board oldBoard = Game.emptyBoard();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        oldBoard[i, j] = gameBoard[i, j];
                    }
                }

                gameBoard = Game.move(direction, gameBoard);

                if (!Game.boardsEqual(oldBoard, gameBoard))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (gameBoard[i, j] > oldBoard[i, j] && oldBoard[i, j] != 0)
                            {
                                score += gameBoard[i, j];
                            }
                        }
                    }

                    gameBoard = Game.sproutTiles(gameBoard);
                    UpdateUI();
                    CheckGameState();
                }
            }
        }
    }
}
