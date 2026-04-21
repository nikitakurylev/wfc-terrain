using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TerrainGeneration
{
    public class TerrainEditor : MonoBehaviour
    {
        [SerializeField] private TerrainManager _terrainManager;
        [SerializeField] private Button _generateButton;
        [SerializeField] private TMP_Dropdown _dropdown;

        private Vector2Int _position1;
        private Vector2Int _position2;

        private bool _isSelecting;

        private void Start()
        {
            _dropdown.options.Add(new TMP_Dropdown.OptionData("Regenerate"));
            _dropdown.options.Add(new TMP_Dropdown.OptionData("Clear"));
            _dropdown.options.AddRange(_terrainManager.PossibleBiomes.Select(b => new TMP_Dropdown.OptionData(b.name)));
        }

        private void Update()
        {
            if (_isSelecting)
            {
                _position2 = GetBiomePositionAtCursor();

                var bottomLeft = new Vector2Int(Mathf.Min(_position1.x, _position2.x),
                    Mathf.Min(_position1.y, _position2.y));

                var topRight = new Vector2Int(Mathf.Max(_position1.x, _position2.x) + 1,
                    Mathf.Max(_position1.y, _position2.y) + 1);

                transform.position =
                    new Vector3((_position1.x + _position2.x) / 2f, 0, (_position1.y + _position2.y) / 2f) *
                    _terrainManager.ScaledBiomeSize;

                transform.localScale = new Vector3(topRight.x - bottomLeft.x, 10, topRight.y - bottomLeft.y) *
                    _terrainManager.ScaledBiomeSize + Vector3.one * _terrainManager.ScaledBiomeSize;

                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    _isSelecting = false;
                    transform.localScale = Vector3.zero;
                    if(_dropdown.value == 0)
                        _terrainManager.RegenerateTerrain(bottomLeft, topRight);
                    else if(_dropdown.value == 1)
                        _terrainManager.ClearTerrain(bottomLeft, topRight);
                    else
                        _terrainManager.PaintTerrain(_terrainManager.PossibleBiomes[_dropdown.value - 2], bottomLeft, topRight);
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    _position1 = GetBiomePositionAtCursor();
                    _isSelecting = true;
                }
            }
        }

        private Vector2Int GetBiomePositionAtCursor()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var raycastHit))
            {
                return new Vector2Int(Mathf.RoundToInt(raycastHit.point.x / _terrainManager.ScaledBiomeSize),
                    Mathf.RoundToInt(raycastHit.point.z / _terrainManager.ScaledBiomeSize));
            }

            return new Vector2Int();
        }
    }
}