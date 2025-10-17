using System.Collections.Generic;
using Game.Entities;
using Models;
using UnityEngine;

namespace Game
{
    public class MainCameraManager : MonoBehaviour
    {
        [SerializeField]
        private float Angle = 180f;

        [SerializeField]
        private float ZScale = -1.15f;

        [SerializeField]
        private float ZOffset = -10f;

        public Camera Camera { get; private set; }

        private GameObject      _focus;
        private bool            _offset;
        private HashSet<Entity> _rotatingEntities;

        private void Awake()
        {
            Camera = Camera.main;
            if (Camera == null)
            {
                Debug.LogError("MainCameraManager: Keine Hauptkamera gefunden!");
                return;
            }

            Camera.orthographicSize = Mathf.Max(Settings.MapScale, 0.001f);
            Camera.transparencySortMode = TransparencySortMode.CustomAxis;
            _offset = Settings.CameraOffset;
            _rotatingEntities = new HashSet<Entity>();
        }

        private void Update()
        {
            if (Camera == null)
                return;

            CheckForInputs();

            // Drehung der Kamera
            transform.rotation = Quaternion.Euler(0, 0, Settings.CameraAngle * Mathf.Rad2Deg);

            // Fokus-Position setzen
            if (_focus != null)
            {
                var yOffset = (_offset ?
                    2.5f :
                    0) * Camera.orthographicSize / 6f;
                transform.position = new Vector3(
                    _focus.transform.position.x,
                    _focus.transform.position.y,
                    ZOffset
                );
                transform.Translate(0, 0.5f + yOffset, 0, Space.Self);
            }

            // Sichere Projektionsmatrix berechnen
            ApplyCustomProjectionMatrix();

            // Transparenzsortierung aktualisieren
            Camera.transparencySortAxis = transform.up;

            // Rotierende Entities aktualisieren
            foreach (var entity in _rotatingEntities)
            {
                entity.Rotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            }
        }

        private void ApplyCustomProjectionMatrix()
        {
            float orthoHeight = Mathf.Max(Camera.orthographicSize, 0.0001f);
            float orthoWidth = Camera.aspect * orthoHeight;

            // Basis-Orthoprojektion
            Matrix4x4 m = Matrix4x4.Ortho(
                -orthoWidth, orthoWidth,
                -orthoHeight, orthoHeight,
                Camera.nearClipPlane,
                Camera.farClipPlane
            );

            // Sicherheitsprüfung für Angle
            if (!float.IsFinite(Angle))
                Angle = 0f;

            float s = ZScale / orthoHeight;
            float sinA = Mathf.Sin(Mathf.Deg2Rad * -Angle);
            float cosA = Mathf.Cos(Mathf.Deg2Rad * -Angle);

            m[0, 2] = s * sinA;
            m[1, 2] = -s * cosA;
            m[0, 3] = -ZOffset * m[0, 2];
            m[1, 3] = -ZOffset * m[1, 2];

            // Matrix validieren, bevor sie gesetzt wird
            if (IsMatrixValid(m))
            {
                Camera.projectionMatrix = m;
            }
            else
            {
                Debug.LogWarning("MainCameraManager: Ungültige Projektionsmatrix erkannt – übersprungen.");
            }
        }

        private static bool IsMatrixValid(Matrix4x4 m)
        {
            for (int i = 0; i < 16; i++)
            {
                if (!float.IsFinite(m[i]))
                    return false;
            }

            return true;
        }

        private void CheckForInputs()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.mouseScrollDelta != Vector2.zero)
                {
                    float newSize = Camera.orthographicSize - Input.mouseScrollDelta.y * 0.5f;
                    Camera.orthographicSize = Mathf.Clamp(newSize, 3f, 15f);
                    Settings.MapScale = Camera.orthographicSize;
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                _offset = !_offset;
                Settings.CameraOffset = _offset;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                Settings.CameraAngle = 180f;
            }
        }

        public void SetFocus(GameObject focus)
        {
            _focus = focus;
        }

        public void AddRotatingEntity(Entity entity)
        {
            _rotatingEntities.Add(entity);
        }

        public void RemoveRotatingEntity(Entity entity)
        {
            _rotatingEntities.Remove(entity);
        }

        public void Clear()
        {
            _rotatingEntities.Clear();
        }
    }
}