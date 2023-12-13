using System;
using System.Collections.Generic;
using Hackathon2023Winter.Entity;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// TilemapProviderに含まれるEntityを管理するクラス
    /// </summary>
    public class LevelEntityManager : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera;
        [SerializeField] private TilemapProvider tilemapProviderPrefab;
        [SerializeField] private EntityShaderBridge entityShaderBridge;

        private List<BaseEntity> _entities;
        private bool _hasLevelData;
        private TilemapProvider _provider;
        private bool _isOnline;

        private Subject<GameObject> _clearSubject;
        public IObservable<GameObject> OnClearObservable => _clearSubject;
        private Subject<int> _goToSubject;
        public IObservable<int> OnGoToObservable => _goToSubject;

        public void Setup(bool isOnline, bool isHost)
        {
            _isOnline = isOnline;
            _entities = new List<BaseEntity>();
            entityShaderBridge.Setup(isOnline, isHost);
            _clearSubject = new Subject<GameObject>();
            _goToSubject = new Subject<int>();
        }

        public void Cleanup()
        {
            _entities.Clear();
            entityShaderBridge.Cleanup();
            _clearSubject?.Dispose();
            _goToSubject?.Dispose();
            if (_provider != null)
            {
                if (_isOnline)
                {
                    PhotonNetwork.Destroy(_provider.gameObject);
                }
                else
                {
                    Destroy(_provider.gameObject);
                }
            }
        }

        /// <summary>
        /// オフラインモードまたはオンラインモードのHostの時、Levelを生成する
        /// </summary>
        /// <param name="isOnline"></param>
        public void CreateLevel(bool isOnline)
        {
            _hasLevelData = true;
            var tilemap = tilemapProviderPrefab.TerrainTilemap;
            tilemap.CompressBounds();
            var size = tilemap.size;
            float sX = (float)size.x / 2;
            float sY = (float)size.y / 2;

            //Tileを生成する
            if (isOnline)
            {
                _provider = PhotonNetwork.Instantiate(tilemapProviderPrefab.name, Vector3.zero, Quaternion.identity)
                    .GetComponent<TilemapProvider>();
            }
            else
            {
                _provider = Instantiate(tilemapProviderPrefab);
                _provider.SwitchToOffline();
            }

            var transform1 = _provider.transform;
            transform1.parent = transform;
            transform1.localPosition = new Vector3(sX, sY, 0) * (-1.0f);
            transform1.localScale = Vector3.one;

            //TilemapProvider内のEntityを全て取得する
            _entities = _provider.GetEntities();

            //初期化処理
            foreach (var entity in _entities)
            {
                entity.SetIsOwner(true);
                switch (entity)
                {
                    case PlayerEntity playerEntity:
                        bool isCircle = playerEntity.IsCircle;
                        if (isOnline)
                        {
                            playerEntity.Setup(true, isCircle);
                        }
                        else
                        {
                            playerEntity.Setup(isCircle, true);
                        }

                        break;
                    case JumpRampEntity jumpRamp:
                        jumpRamp.Setup();
                        break;
                    case ShiftBlock shiftBlock:
                        shiftBlock.Setup();
                        break;
                    case GoalEntity goalEntity:
                        goalEntity.OnClearObservable.Subscribe(x => _clearSubject.OnNext(x)).AddTo(gameObject);
                        break;
                    case GateEntity gateEntity:
                        gateEntity.OnGoToObservable.Subscribe(x=>_goToSubject.OnNext(x)).AddTo(gameObject);
                        break;
                }
            }

            //PlayerのScaleをEntityShaderBridgeに渡す
            SendPlayerScaleInfo();
        }

        private void SendPlayerScaleInfo()
        {
            Vector2 circleScale = Vector2.zero;
            Vector2 rectScale = Vector2.zero;
            GameObject playerCircle = null;
            GameObject playerRect = null;

            foreach (var entity in _entities)
            {
                if (entity is PlayerEntity playerEntity)
                {
                    var scale = playerEntity.GetSize() / 2;
                    var cameraScale = renderCamera.ViewportToWorldPoint(Vector2.one)
                                      - renderCamera.ViewportToWorldPoint(Vector2.zero);
                    var width = scale / cameraScale.x;
                    var height = scale / cameraScale.y;

                    if (playerEntity.IsCircle)
                    {
                        circleScale = new Vector2(width, height);
                        playerCircle = playerEntity.gameObject;
                    }
                    else
                    {
                        rectScale = new Vector2(width, height);
                        playerRect = playerEntity.gameObject;
                    }
                }
            }

            entityShaderBridge.SetPlayerScale(circleScale, rectScale);
            entityShaderBridge.SetPlayerObject(playerCircle, playerRect);
        }

        private void Update()
        {
            entityShaderBridge.CalcShaderInfo();
        }
    }
}