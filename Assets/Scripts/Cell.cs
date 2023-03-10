using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Dirt,
    Rock,
    Water
}

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject _waterPrefab;
    [SerializeField] private GameObject _rockPrefab;
    [SerializeField] private GameObject _rootPrefab;
    
    public Vector2Int Location;

    public CellType Type
    {
        get
        {
            if (HasRock())
                return CellType.Rock;
            if (HasWater())
                return CellType.Water;
            return CellType.Dirt;
        }
    }
    
    public Bond BondLeft = null;
    public Bond BondRight = null;
    public Bond BondUp = null;
    public Bond BondDown = null;

    private Water _water = null;
    private Rock _rock = null;

    private SpriteRenderer _renderer;
    private RootRenderer _branch;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _branch = Instantiate(_rootPrefab, this.transform.position, Quaternion.identity).GetComponent<RootRenderer>();
    }

    public void AddWater()
    {
        _water = Instantiate(_waterPrefab, this.transform).GetComponent<Water>();
        _water.transform.SetParent(this.transform);
    }

    public bool HasWater()
    {
        return _water != null;
    }

    public Water GetWater()
    {
        return _water;
    }

    public void AddRock()
    {
        _rock = Instantiate(_rockPrefab, this.transform).GetComponent<Rock>();
        _rock.transform.SetParent(this.transform);
    }

    public bool HasRock()
    {
        return _rock != null;
    }

    public void UpdateRoot()
    {
        int left = BondLeft == null ? 0 : BondLeft.Player;
        int right = BondRight == null ? 0 : BondRight.Player;
        int up = BondUp == null ? 0 : BondUp.Player;
        int down = BondDown == null ? 0 : BondDown.Player;
        
        _branch.UpdateSprite(left, right, up, down);
	}

	// Checks all bonds. If any of them belong to a player, returns that player's ID.
    // We assume that a tile is occupied by a root if a player owns a bond that leads to it.
    // Returns 0 if this tile is deemed unoccupied.
    public int Occupancy()
    {
        Bond[] bonds = { BondLeft, BondRight, BondUp, BondDown };

        for (int i = 0; i < bonds.Length; i++)
        {
            if (bonds[i] != null && bonds[i].Player != 0)
            {
                return bonds[i].Player;
            }
        }

        return 0;
    }

    public bool IsConnected(int player)
    {
        if (HasWater())
            return true;
            
        if (BondLeft != null && BondLeft.Player == player)
            return true;
        if (BondRight != null && BondRight.Player == player)
            return true;
        if (BondUp != null && BondUp.Player == player)
            return true;
        if (BondDown != null && BondDown.Player == player)
            return true;

        return false;
    }
}
