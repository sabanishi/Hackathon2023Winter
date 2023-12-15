using System;
using System.Collections.Generic;
using Hackathon2023Winter.Entity;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// TilemapProviderに含まれるEntityを管理するクラス
    /// </summary>
    public class LevelEntityManager : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera;
        [SerializeField] private EntityShaderBridge entityShaderBridge;

        private List<BaseEntity> _entities;
        private bool _hasLevelData;
        private TilemapProvider _provider;
        private bool _isOnline;
        private TilemapProviderDict _tilemapProviderDict;

        private Subject<(GameObject,GoalEntity)> _clearSubject;
        public IObservable<(GameObject clearObj,GoalEntity goalEntity)> OnClearObservable => _clearSubject;
        
        private Subject<(int,bool)> _onEnterSubject;
        private Subject<(int,bool)> _onExitSubject;
        public IObservable<(int stageId, bool isCircle)> OnEnterObservable => _onEnterSubject;
        public IObservable<(int stageId, bool isCircle)> OnExitObservable => _onExitSubject;

        public void Setup(bool isOnline, bool isHost)
        {
            _isOnline = isOnline;
            _entities = new List<BaseEntity>();
            entityShaderBridge.Setup(isOnline, isHost);
            _clearSubject = new Subject<(GameObject,GoalEntity)>();
            _onEnterSubject = new Subject<(int,bool)>();
            _onExitSubject = new Subject<(int,bool)>();
            _tilemapProviderDict = Resources.Load<TilemapProviderDict>("TilemapProviderDict");
        }

        private void OnDestroy()
        {
            _clearSubject?.Dispose();
            _onEnterSubject?.Dispose();
            _onExitSubject?.Dispose();
        }

        public void Cleanup()
        {
            _entities.Clear();
            entityShaderBridge.Cleanup();
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
        public void CreateLevel(bool isOnline,int stageId)
        {
            if (!_tilemapProviderDict.TryGetPrefab(stageId, out var tilemapProviderPrefab))
            {
                Debug.LogError($"LevelEntityManager#CreateLevel: {stageId}が見つかりませんでした");
                return;
            }
            
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
                if(entity==null)continue;
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
                        goalEntity.OnClearObservable.Subscribe(x => _clearSubject.OnNext((x,goalEntity))).AddTo(gameObject);
                        break;
                    case GateEntity gateEntity:
                        gateEntity.OnEnterObservable.Subscribe(x => _onEnterSubject.OnNext(x)).AddTo(gameObject);
                        gateEntity.OnExitObservable.Subscribe(x =>
                        {
                            _onExitSubject?.OnNext(x);
                        }).AddTo(gameObject);
                        break;
                }
            }

            //PlayerのScaleをEntityShaderBridgeに渡す
            SendPlayerScaleInfo();
            SendEntityInfo();
        }

        public void SendPlayerScaleInfo()
        {
            Vector2 circleScale = Vector2.zero;
            Vector2 rectScale = Vector2.zero;
            foreach (var entity in _entities)
            {
                switch (entity)
                {
                    case PlayerEntity playerEntity:
                        var scale = playerEntity.GetSize() / 2;
                        var cameraScale = renderCamera.ViewportToWorldPoint(Vector2.one)
                                          - renderCamera.ViewportToWorldPoint(Vector2.zero);
                        var width = scale / cameraScale.x;
                        var height = scale / cameraScale.y;

                        if (playerEntity.IsCircle)
                        {
                            circleScale = new Vector2(width, height);
                        }
                        else
                        {
                            rectScale = new Vector2(width, height);
                        }
                        break;
                }
            }
            entityShaderBridge.SetPlayerScale(circleScale, rectScale);
        }

        private void SendEntityInfo()
        {
            GameObject playerCircle = null;
            GameObject playerRect = null;
            //GameObject,isGoal
            var goalObjects = new List<(GameObject,bool,Vector2)>();

            foreach (var entity in _entities)
            {
                switch (entity)
                {
                    case PlayerEntity playerEntity:
                        if (playerEntity.IsCircle)
                        {
                            playerCircle = playerEntity.gameObject;
                        }
                        else
                        {
                            playerRect = playerEntity.gameObject;
                        }
                        break;
                    case GoalEntity goalEntity:
                        var goalSeed = new Vector2(Random.value * 100,Random.value * 100);
                        goalObjects.Add((goalEntity.gameObject,true,goalSeed));
                        break;
                    case GateEntity gateEntity:
                        var gateSeed = new Vector2(Random.value * 100,Random.value * 100);
                        goalObjects.Add((gateEntity.gameObject,false,gateSeed));
                        break;
                }
            }
            
            entityShaderBridge.SetEntityObject(playerCircle, playerRect,goalObjects);
        }

        private void Update()
        {
            entityShaderBridge.CalcShaderInfo();
        }

        /// <summary>
        /// ユーザーの操作を可能にする
        /// </summary>
        public void SetCanInput(bool canInput)
        {
            foreach (var entity in _entities)
            {
                if (entity is PlayerEntity playerEntity)
                {
                    playerEntity.SetCanInput(canInput);
                }
            }
        }

        public void SetIsSimulateActive(bool isActive)
        {
            foreach (var entity in _entities)
            {
                entity.SetIsSimulate(isActive);
            }
        }

        public List<BaseEntity> GetEntities()
        {
            return _entities;
        }
    }
}