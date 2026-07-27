using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FishMatch.Match3
{
    public class Match3GridController : MonoBehaviour
    {
        [SerializeField] private int width = 7;
        [SerializeField] private int height = 7;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private GemDefinition[] gemDefinitions;
        [SerializeField] private float dragThresholdPixels = 40f;
        [SerializeField] private float swapAnimationDelay = 0.15f;
        [SerializeField] private float clearAnimationDelay = 0.1f;
        [SerializeField] private float fallAnimationDelay = 0.2f;

        // Fired once per player swap that actually produces a match (i.e. "costs a move").
        public event Action OnValidSwapPerformed;

        // Fired once per cascade step within a swap's resolution, with the number of gems cleared in that step.
        public event Action<int> OnGemsCleared;

        public bool InputEnabled { get; set; } = true;

        private Match3Grid grid;
        private Gem[,] gemViews;
        private Dictionary<GemType, GemDefinition> definitionLookup;
        private Camera targetCamera;

        private bool isBusy;
        private bool isDragging;
        private Vector2Int dragStartCell;
        private Vector2 dragStartScreenPos;

        private void Start()
        {
            targetCamera = Camera.main;
            BuildDefinitionLookup();

            grid = new Match3Grid(width, height, RandomGemType);
            grid.FillRandom();

            gemViews = new Gem[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    gemViews[x, y] = SpawnGem(grid.GetCell(cell), cell, spawnAboveGrid: false);
                }
            }
        }

        private void BuildDefinitionLookup()
        {
            definitionLookup = new Dictionary<GemType, GemDefinition>();
            foreach (var definition in gemDefinitions)
            {
                definitionLookup[definition.type] = definition;
            }
        }

        private GemType RandomGemType()
        {
            return gemDefinitions[UnityEngine.Random.Range(0, gemDefinitions.Length)].type;
        }

        private void Update()
        {
            if (isBusy || !InputEnabled)
            {
                return;
            }

            var pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                if (TryGetCellAtScreenPosition(pointer.position.ReadValue(), out var cell))
                {
                    dragStartCell = cell;
                    dragStartScreenPos = pointer.position.ReadValue();
                    isDragging = true;
                }
            }
            else if (isDragging && pointer.press.isPressed)
            {
                Vector2 delta = pointer.position.ReadValue() - dragStartScreenPos;
                if (delta.magnitude >= dragThresholdPixels)
                {
                    isDragging = false;

                    var direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                        ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
                        : new Vector2Int(0, (int)Mathf.Sign(delta.y));

                    var targetCell = dragStartCell + direction;
                    if (grid.InBounds(targetCell))
                    {
                        StartCoroutine(SwapRoutine(dragStartCell, targetCell));
                    }
                }
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                isDragging = false;
            }
        }

        private bool TryGetCellAtScreenPosition(Vector2 screenPos, out Vector2Int cell)
        {
            var screenPoint = new Vector3(screenPos.x, screenPos.y, -targetCamera.transform.position.z);
            var world = targetCamera.ScreenToWorldPoint(screenPoint);
            var local = transform.InverseTransformPoint(world);

            cell = new Vector2Int(Mathf.RoundToInt(local.x / cellSize), Mathf.RoundToInt(local.y / cellSize));
            return grid.InBounds(cell);
        }

        private IEnumerator SwapRoutine(Vector2Int a, Vector2Int b)
        {
            isBusy = true;

            SwapVisualAndData(a, b);
            yield return new WaitForSeconds(swapAnimationDelay);

            var matches = grid.FindMatches();
            if (matches.Count == 0)
            {
                // No match produced - swap back.
                SwapVisualAndData(a, b);
                yield return new WaitForSeconds(swapAnimationDelay);
                isBusy = false;
                yield break;
            }

            OnValidSwapPerformed?.Invoke();

            yield return ResolveMatchesRoutine();
            isBusy = false;
        }

        private void SwapVisualAndData(Vector2Int a, Vector2Int b)
        {
            grid.Swap(a, b);
            (gemViews[a.x, a.y], gemViews[b.x, b.y]) = (gemViews[b.x, b.y], gemViews[a.x, a.y]);
            gemViews[a.x, a.y].SetGridPosition(a, CellToLocalPosition(a));
            gemViews[b.x, b.y].SetGridPosition(b, CellToLocalPosition(b));
        }

        private IEnumerator ResolveMatchesRoutine()
        {
            while (true)
            {
                var matches = grid.FindMatches();
                if (matches.Count == 0)
                {
                    yield break;
                }

                foreach (var cell in matches)
                {
                    var gem = gemViews[cell.x, cell.y];
                    gemViews[cell.x, cell.y] = null;
                    Destroy(gem.gameObject);
                }
                grid.ClearCells(matches);
                OnGemsCleared?.Invoke(matches.Count);

                yield return new WaitForSeconds(clearAnimationDelay);

                var collapseResults = grid.CollapseColumns();
                ApplyCollapseResults(collapseResults);

                yield return new WaitForSeconds(fallAnimationDelay);
            }
        }

        private void ApplyCollapseResults(List<ColumnCollapseResult> results)
        {
            foreach (var column in results)
            {
                foreach (var fall in column.Falls)
                {
                    var gem = gemViews[fall.From.x, fall.From.y];
                    gemViews[fall.From.x, fall.From.y] = null;
                    gemViews[fall.To.x, fall.To.y] = gem;
                    gem.SetGridPosition(fall.To, CellToLocalPosition(fall.To));
                }

                foreach (var spawn in column.Spawns)
                {
                    gemViews[spawn.Position.x, spawn.Position.y] = SpawnGem(spawn.Type, spawn.Position, spawnAboveGrid: true);
                }
            }
        }

        private Gem SpawnGem(GemType type, Vector2Int cellPosition, bool spawnAboveGrid)
        {
            var targetLocalPos = CellToLocalPosition(cellPosition);
            var spawnLocalPos = spawnAboveGrid
                ? targetLocalPos + new Vector3(0f, cellSize * height, 0f)
                : targetLocalPos;

            var gemObject = Instantiate(gemPrefab, transform);
            gemObject.transform.localPosition = spawnLocalPos;

            var gem = gemObject.GetComponent<Gem>();
            gem.Initialize(definitionLookup[type], cellPosition);
            gem.SetGridPosition(cellPosition, targetLocalPos, snap: !spawnAboveGrid);
            return gem;
        }

        private Vector3 CellToLocalPosition(Vector2Int cell)
        {
            return new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
        }
    }
}
