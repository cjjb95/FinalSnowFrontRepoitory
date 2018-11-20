/*
Function: 		Stores common hard-coded variable values used within the game e.g. key mappings, mouse sensitivity
Author: 		NMCG
Version:		1.0
Date Updated:	5/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace GDLibrary
{
    public sealed class LerpSpeed
    {
        private static readonly float SpeedMultiplier = 2;
        public static readonly float VerySlow = 0.008f; 
        public static readonly float Slow = SpeedMultiplier * VerySlow;
        public static readonly float Medium = SpeedMultiplier * Slow;
        public static readonly float Fast = SpeedMultiplier * Medium;
        public static readonly float VeryFast = SpeedMultiplier * Fast;
    }

    public sealed class AppData
    {
        #region Common
        public static int IndexMoveForward = 0;
        public static int IndexMoveBackward = 1;
        public static int IndexRotateLeft = 2;
        public static int IndexRotateRight = 3;
        public static int IndexMoveJump = 4;
        public static int IndexMoveCrouch = 5;
        public static int IndexStrafeLeft = 6;
        public static int IndexStrafeRight = 7;
        #endregion

        #region Car
        public static readonly float CarRotationSpeed = 0.1f;
        public static readonly float CarMoveSpeed = 0.09f;
        public static readonly float CarStrafeSpeed = 0.7f * CarMoveSpeed;
        #endregion

        #region Camera
        public static readonly float CameraRotationSpeed = 0.01f;
        public static readonly float CameraMoveSpeed = 0.075f;
        public static readonly float CameraStrafeSpeed = 0.7f * CameraMoveSpeed;
    
        public static readonly Keys[] CameraMoveKeys = { Keys.W, Keys.S, Keys.A, Keys.D, 
                                         Keys.Space, Keys.C, Keys.LeftShift, Keys.RightShift};
        public static readonly Keys[] CameraMoveKeys_Alt1 = { Keys.T, Keys.G, Keys.F, Keys.H };

        //3rd person specific
        public static readonly float CameraThirdPersonScrollSpeedDistanceMultiplier = 0.00125f;
        public static readonly float CameraThirdPersonScrollSpeedElevationMultiplier = 0.001f;
        public static readonly float CameraThirdPersonDistance = 60;
        public static readonly float CameraThirdPersonElevationAngleInDegrees = 30;

        //security camera
        public static readonly Vector3 SecurityCameraRotationAxisYaw = Vector3.UnitX;
        public static readonly Vector3 SecurityCameraRotationAxisPitch = Vector3.UnitY;
        public static readonly Vector3 SecurityCameraRotationAxisRoll = Vector3.UnitZ;

        #endregion

        public static readonly Keys[] ObjectMoveKeys = {
            Keys.NumPad8, Keys.NumPad5,  //forward, backward
            Keys.NumPad4, Keys.NumPad6,  //rotate left, rotate right
            Keys.NumPad1, Keys.NumPad3   //strafe left, strafe right
        };

        #region Menu
        public static readonly string MenuMainID = "main";
        public static readonly Keys MenuShowHideKey = Keys.Escape;
        #endregion

        #region JigLibX
        public static readonly Vector3 Gravity = -10 * Vector3.UnitY;
        public static readonly Vector3 BigGravity = 5 * Gravity;
        #endregion

        #region Effect parameter ids used by the effect dictionary
        public static readonly string LitModelsEffectID = "lit basic";
        public static readonly string UnlitModelsEffectID = "unlit basic";
        public static readonly string UnlitModelDualEffectID = "unlit dual";
        #endregion

        #region Player
        public static readonly string PlayerOneID = "player1";
        public static readonly string PlayerTwoID = "player2";
        public static readonly Keys[] PlayerOneMoveKeys = { Keys.U, Keys.J, Keys.H, Keys.K, Keys.Y, Keys.I, Keys.N, Keys.M };
        public static readonly Keys[] PlayerTwoMoveKeys = { Keys.NumPad8, Keys.NumPad5, Keys.NumPad4, Keys.NumPad6, Keys.NumPad7, Keys.NumPad9, Keys.NumPad2, Keys.NumPad3 };
        public static readonly float PlayerMoveSpeed = 0.1f;
        public static readonly float PlayerStrafeSpeed = 0.7f * PlayerMoveSpeed;
        public static readonly float PlayerRotationSpeed = 0.08f;
        public static readonly float PlayerRadius = 1.5f;
        public static readonly float PlayerHeight = 4.5f;
        public static readonly float PlayerMass = 25;
        public static readonly float PlayerJumpHeight = 25;
        #endregion

        #region Mouse
        //defines how much the mouse has to move in pixels before a movement is registered - see MouseManager::HasMoved()
        public static readonly float MouseSensitivity = 1;

        //always ensure that we start picking OUTSIDE the collidable first person camera radius - otherwise we will always pick ourself!
        public static readonly float CollidableCameraCapsuleRadius = 2;
        public static readonly float PickStartDistance = CollidableCameraCapsuleRadius * 2f;
        public static readonly float PickEndDistance = 1000; //can be related to camera far clip plane radius but should be limited to typical level max diameter
        public static readonly bool EnablePickAndPlace = true;

        #endregion
    }
}
