using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    public class HeroPlayerObject : PlayerObject
    {
        int health;
        private bool stunned;

        public HeroPlayerObject(string id, ActorType actorType,
            Transform3D transform, EffectParameters effectParameters,
            Model model, Keys[] moveKeys, float radius, float height,
            float accelerationRate, float decelerationRate, float jumpHeight,
            Vector3 translationOffset, KeyboardManager keyboardManager)
            : base(id, actorType, transform, effectParameters, model, moveKeys, radius, height, accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            this.stunned = false;
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        private bool CollisionSkin_callbackFn(JigLibX.Collision.CollisionSkin collider, JigLibX.Collision.CollisionSkin collidee)
        {
            CollidableObject thingHit = collidee.Owner.ExternalData as CollidableObject;

            if (thingHit.ActorType == ActorType.Ice)
            {
                object[] additionalParameter = { true };
                EventDispatcher.Publish(new EventData(EventActionType.OnIce, EventCategoryType.Obstacle, additionalParameter));
            }
            else
            {
                object[] additionalParameter = { false };
                EventDispatcher.Publish(new EventData(EventActionType.OnIce, EventCategoryType.Obstacle, additionalParameter));
            }

            if (thingHit.ActorType == ActorType.Snow)
            {
                int x = 0;//put break point to know if player inside snow
            }

            return true;
        }

        protected override void HandleKeyboardInput(GameTime gameTime)
        {
            //if (this.stunned)
            //{
            /* Notice in the code below that we are NO LONGER simply changing the camera translation value. 
         * Since this is now a collidable camera we, instead, modify the camera position by calling the PlayerObject move methods.
         * 
         * Q. Why do we still use the rotation methods of Transform3D? 
         * A. Rotating the camera doesnt affect CD/CR since the camera is modelled by a player object which has a capsule shape.
         *    A capsule's collision response won't alter as a result of any rotation since its cross-section is spherical across the XZ-plane.
         */

            //forward/backward
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[0]))
            {
                this.CharacterBody.Velocity += this.Transform.Look * 2 * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[1]))
            {
                this.CharacterBody.Velocity -= this.Transform.Look * 2 * gameTime.ElapsedGameTime.Milliseconds;
            }

            //strafe left/right
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[2]))
            {

                this.CharacterBody.Velocity -= this.Transform.Right * 2 * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[3]))
            {

                this.CharacterBody.Velocity += this.Transform.Right * 2 * gameTime.ElapsedGameTime.Milliseconds;
            }


            //}


            //to do - clone, dispose

        }


        protected override void HandleMouseInput(GameTime gameTime)
        {
            base.HandleMouseInput(gameTime);
        }
    }
}
