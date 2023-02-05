using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum SymmetryType
{
    None,
    Regular,
    Diagonal
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int _width, _height;
    [SerializeField] private SymmetryType _mapSymmetry;
    [SerializeField] private int _rocksPerPlayer, _waterPerPlayer;
    [SerializeField] private GameObject _cellPrefab, _bondPrefab;
    [SerializeField] private GameObject _wallPrefab, _cornerPrefab;
    [SerializeField] private GameObject _playerPrefab;

    public Cell[,] Cells;
    public Bond[] Bonds;

    private void Awake()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        Cells = new Cell[_width, _height];
        Bonds = new Bond[2 * _width * _height - _width - _height];

        GenerateWalls();
        GenerateDirt();
        GenerateWater();
        GenerateRock();
        GeneratePlayers();
    }

    public void RegenerateLevel()
    {
        foreach (Transform child in this.transform) {
             GameObject.Destroy(child.gameObject);
        }

        GenerateLevel();
    }

    private void GenerateWalls()
    {
        Instantiate(_cornerPrefab, new Vector3(0.5f, 0.5f, 0f), Quaternion.Euler(0f, 0f, 90f));
        Instantiate(_cornerPrefab, new Vector3(_width + 1.5f, 0.5f, 0f), Quaternion.Euler(0f, 0f, 180f));
        Instantiate(_cornerPrefab, new Vector3(0.5f, _height + 1.5f, 0f), Quaternion.identity);
        Instantiate(_cornerPrefab, new Vector3(_width + 1.5f, _height + 1.5f, 0f), Quaternion.Euler(0f, 0f, 270f));

        for (int x = 1; x < _width + 1; x++)
        {
            Instantiate(_wallPrefab, new Vector3(x + 0.5f, 0.5f, 0f), Quaternion.Euler(0f, 0f, 180f));
            Instantiate(_wallPrefab, new Vector3(x + 0.5f, _height + 1.5f, 0f), Quaternion.identity);
        }

        for (int y = 1; y < _height + 1; y++)
        {
            Instantiate(_wallPrefab, new Vector3(0.5f, y + 0.5f, 0f), Quaternion.Euler(0f, 0f, 90f));
            Instantiate(_wallPrefab, new Vector3(_width + 1.5f, y + 0.5f, 0f), Quaternion.Euler(0f, 0f, 270f));
        }
    }

    private void GenerateDirt()
    {
        int bondNumber = 0;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Cell cell = Instantiate(_cellPrefab, new Vector3(x + 1.5f, y + 1.5f, 0f), Quaternion.identity).GetComponent<Cell>();
                cell.transform.parent = this.transform;
                cell.Location = new Vector2Int(x, y);

                Cells[x, y] = cell;

                if (x <= 0)
                {
                    cell.BondLeft = null;
                }
                else
                {
                    cell.BondLeft = Cells[x - 1, y].BondRight;
                    cell.BondLeft.Cell1 = cell;
                }

                if (x >= _width - 1)
                {
                    cell.BondRight = null;
                }
                else
                {
                    cell.BondRight = Instantiate(_bondPrefab, new Vector3(x + 2f, y + 1.5f, 0f), Quaternion.identity).GetComponent<Bond>();
                    cell.BondRight.transform.parent = cell.transform;
                    cell.BondRight.IsVertical = false;
                    cell.BondRight.Cell2 = cell;
                    Bonds[bondNumber] = cell.BondRight;
                    Bonds[bondNumber].index = bondNumber;
                    bondNumber++;
                }

                if (y <= 0)
                {
                    cell.BondDown = null;
                }
                else
                {
                    cell.BondDown = Cells[x, y - 1].BondUp;
                    cell.BondDown.Cell1 = cell;
                }

                if (y >= _height - 1)
                {
                    cell.BondUp = null;
                }
                else
                {
                    cell.BondUp = Instantiate(_bondPrefab, new Vector3(x + 0.5f, y + 1f, 0f), Quaternion.identity).GetComponent<Bond>();
                    cell.BondUp.transform.parent = cell.transform;
                    cell.BondUp.IsVertical = true;
                    cell.BondUp.Cell2 = cell;
                    Bonds[bondNumber] = cell.BondUp;
                    Bonds[bondNumber].index = bondNumber;
                    bondNumber++;
                }
            }
        }
    }

    private void GenerateWater()
    {
        Cells[0, 0].AddWater();
        Cells[_width - 1, 0].AddWater();
        Cells[0, _height - 1].AddWater();
        Cells[_width - 1, _height - 1].AddWater();

        switch (_mapSymmetry)
        {
        case SymmetryType.None:
            break;

        case SymmetryType.Regular:
            for (int i = 0; i < _waterPerPlayer; i++)
            {
                int x;
                int y;

                while (true)
                {
                    x = Random.Range(0, _width / 2 - 1);
                    y = Random.Range(0, _height / 2 - 1);
                    
                    Cell cell = Cells[x, y];

                    if (cell.Type != CellType.Dirt) continue;

                    CellType leftType = x > 0 ? Cells[x - 1, y].Type : CellType.Rock;
                    CellType rightType = Cells[x + 1, y].Type;
                    CellType upType = Cells[x, y + 1].Type;
                    CellType downType = y > 0 ? Cells[x, y - 1].Type : CellType.Rock;

                    if (leftType == CellType.Water) continue;
                    if (rightType == CellType.Water) continue;
                    if (upType == CellType.Water) continue;
                    if (downType == CellType.Water) continue;

                    break;
                }

                Cells[x, y].AddWater();
                Cells[_width - x - 1, y].AddWater();
                Cells[x, _height - y - 1].AddWater();
                Cells[_width - x - 1, _height - y - 1].AddWater();
            }
            break;

        case SymmetryType.Diagonal:
            break;

        default:
            break;
        }
    }

    private void GenerateRock()
    {
        switch (_mapSymmetry)
        {
        case SymmetryType.None:
            break;

        case SymmetryType.Regular:
            for (int i = 0; i < _rocksPerPlayer; i++)
            {
                int x;
                int y;

                while (true)
                {
                    x = Random.Range(0, _width / 2);
                    y = Random.Range(0, _height / 2);
                    
                    Cell cell = Cells[x, y];

                    if (cell.Type != CellType.Dirt) continue;

                    // CellType leftType = x > 0 ? Cells[x - 1, y].Type : CellType.Rock;
                    // CellType rightType = Cells[x + 1, y].Type;
                    // CellType upType = Cells[x, y + 1].Type;
                    // CellType downType = y > 0 ? Cells[x, y - 1].Type : CellType.Rock;

                    break;
                }

                Cells[x, y].AddRock();
                Cells[_width - x - 1, y].AddRock();
                Cells[x, _height - y - 1].AddRock();
                Cells[_width - x - 1, _height - y - 1].AddRock();
            }
            break;

        case SymmetryType.Diagonal:
            break;

        default:
            break;
        }
    }

    private void GeneratePlayers()
    {
        // THIS IS WHERE I WOULD READ THE NUMBER OF PLAYERS IN GAME, BUT AS I DO NOT KNOW IF THAT IS IN THIS BRANCH I'LL JUST LEAVE THIS MEMO HERE
        int numPlayers = 4;
        // ---------------------

        Vector2Int[] startingLocs = { new Vector2Int(0, 0), new Vector2Int(_width - 1, _height - 1), new Vector2Int(_width - 1, 0), new Vector2Int(0, _height - 1) };

        for (int i = 0; i < numPlayers; i++)
        {
            Player newPlayer = Instantiate(_playerPrefab).GetComponent<Player>();
            newPlayer.SetOccupiedCell(Cells[startingLocs[i].x, startingLocs[i].y]);
        }
    }

    private void CullBranches()
    {
        bool[] bondSafe = new bool[Bonds.Length];
        foreach (Bond bond in Bonds)
        {
            CullBranch(bond, bondSafe);
        }
    }

    private bool CullBranch(Bond bond, bool[] bondSafe)
    {
        if (bond == null || bond.Player == 0)
            return false;

        if (bond.HasWater() || bondSafe[bond.index])
            return (bondSafe[bond.index] = true);

        if (CullBranch(bond.Cell1.BondLeft, bondSafe))
            return (bondSafe[bond.index] = true);
        if (CullBranch(bond.Cell1.BondRight, bondSafe))
            return (bondSafe[bond.index] = true);
        if (CullBranch(bond.Cell1.BondUp, bondSafe))
            return (bondSafe[bond.index] = true);
        if (CullBranch(bond.Cell1.BondDown, bondSafe))
            return (bondSafe[bond.index] = true);

        if (CullBranch(bond.Cell2.BondLeft, bondSafe))
            return (bondSafe[bond.index] = true);
        if (CullBranch(bond.Cell2.BondRight, bondSafe))
            return (bondSafe[bond.index] = true);
        if (CullBranch(bond.Cell2.BondUp, bondSafe))
            return (bondSafe[bond.index] = true);
        if (CullBranch(bond.Cell2.BondDown, bondSafe))
            return (bondSafe[bond.index] = true);

        bond.Player = 0;

        return false;
    }
}