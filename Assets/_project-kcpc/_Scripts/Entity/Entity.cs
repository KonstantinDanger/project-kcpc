using AYellowpaper;
using FishNet.Object;
using ProjectKCPC.Scripts.Config;
using UnityEngine;

namespace ProjectKCPC.Scripts.Entity
{
    public class Entity : NetworkBehaviour
    {
        [field: Header("Base Configs")]
        [field: SerializeField] public CameraConfig CameraConfig { get; private set; }
        [field: SerializeField] public MovementConfig MovementConfig { get; private set; }
        [field: SerializeField] public GravityConfig GravityConfig { get; private set; }
        [field: SerializeField] public ExternalForceConfig ExternalForceConfig { get; private set; }
        [field: SerializeField] public JumpConfig JumpConfig { get; private set; }
        [field: SerializeField] public CrouchConfig CrouchConfig { get; private set; }

        [Header("Base Components")]
        [SerializeField] private InterfaceReference<IMovable> _movementRef;
        [SerializeField] private InterfaceReference<IApplyGravity> _gravityRef;
        [SerializeField] private InterfaceReference<IExternalForceHandler> _externalForceRef;
        [SerializeField] private InterfaceReference<IJumpable> _jumpRef;
        [SerializeField] private InterfaceReference<ICrouchable> _crouchRef;

        public IMovable Movement => _movementRef.Value;
        public IApplyGravity Gravity => _gravityRef.Value;
        public IExternalForceHandler ExternalForce => _externalForceRef.Value;
        public IJumpable Jump => _jumpRef.Value;
        public ICrouchable Crouch => _crouchRef.Value;

        protected void Update()
        {
            UpdateExternalForce();
            UpdateGravity();
            OnUpdate();
        }

        protected virtual void OnUpdate() {}

        private void UpdateExternalForce() 
            => ExternalForce.UpdateExternalForce(ExternalForceConfig.Drag);
        
        private void UpdateGravity() 
            => Gravity.ApplyGravity(GravityConfig.Gravity, GravityConfig.MaxFallSpeed);
    }
}
