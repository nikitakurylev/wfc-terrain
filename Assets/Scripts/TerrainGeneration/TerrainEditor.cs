using UnityEngine;
using UnityEngine.UI;

namespace TerrainGeneration
{
    public class TerrainEditor : MonoBehaviour
    {
        [SerializeField] private TerrainManager _terrainManager;
        [SerializeField] private Button _confirmButton;

        private Vector2Int _position1;
        private Vector2Int _position2;

        private bool _isSelecting;

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
                    new Vector3((_position1.x + _position2.x) / 2f, 0, (_position1.y + _position2.y) / 2f) * _terrainManager.ScaledBiomeSize;

                transform.localScale = new Vector3(topRight.x - bottomLeft.x, 10, topRight.y - bottomLeft.y) *
                                       _terrainManager.ScaledBiomeSize + Vector3.one * _terrainManager.ScaledBiomeSize;

                if (Input.GetMouseButtonUp(0))
                {
                    _isSelecting = false;
                    transform.localScale = Vector3.zero;
                    _terrainManager.RegenerateTerrain(bottomLeft, topRight);
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
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