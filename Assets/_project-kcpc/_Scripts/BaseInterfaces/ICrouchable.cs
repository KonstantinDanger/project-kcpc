namespace ProjectKCPC.Scripts
{
    public interface ICrouchable 
    {
        public float InitialHeight { get; }

        /// <summary>
        /// Returns current height
        /// </summary>
        /// <param name="isCrouching"></param>
        /// <param name="crouchHeight"></param>
        /// <param name="crouchAcceleration"></param>
        /// <returns></returns>
        float SetCrouch(bool isCrouching, float crouchHeight, float crouchAcceleration);
    }
}
