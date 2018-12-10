using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using JigLibX.Collision;
using System;
using Microsoft.Xna.Framework.Audio;

namespace GDApp
{
    public class HeroAnimatedPlayerObject : AnimatedPlayerObject
    {
        private float moveSpeed, rotationSpeed;
        private bool stunned;
        private readonly float DefaultMinimumMoveVelocity = 1;
        bool shovelEquipped = false;
        private bool bOnce;
        private bool firstTime;

        public float MoveSpeed
        {
            get
            {
                return this.moveSpeed;
            }
            set
            {
                this.moveSpeed = value >= 0 ? value : 0;
            }
        }

        public HeroAnimatedPlayerObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Keys[] moveKeys, float radius, float height,
            float accelerationRate, float decelerationRate,
            float moveSpeed, float rotationSpeed,
            float jumpHeight, Vector3 translationOffset,
            KeyboardManager keyboardManager,EventDispatcher eventDispatcher)
            : base(id, actorType, transform, effectParameters, moveKeys, radius, height,
                  accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //add extra constructor parameters like health, inventory etc...
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;


            this.stunned = false;
            this.bOnce = true;
            this.firstTime = true;
            //register for callback on CDCR
            this.CharacterBody.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
            eventDispatcher.ObstacleEvent += EventDispatcher_ObstacleEvent;
            eventDispatcher.ItemEquipped += EventDispatcher_ItemEquipped;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnSnowDrift)
            {
                this.bOnce = true;

            }
        }

        private void EventDispatcher_ItemEquipped(EventData eventData)
        {
            string additionalParameters = eventData.AdditionalParameters[0] as string;
            if (eventData.EventType == EventActionType.OnItem && additionalParameters == "shovel")
            {
                this.shovelEquipped = true;

            }
        }

        private void EventDispatcher_ObstacleEvent(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnSlip)
            {
                this.stunned = true;
            }
            else if (eventData.EventType == EventActionType.SlipOver)
            {
                this.stunned = false;
            }
        }


        //this methods defines how your player interacts with ALL collidable objects in the world - its really the players complete behaviour
        private bool CollisionSkin_callbackFn(CollisionSkin collider, CollisionSkin collidee)
        {
            HandleCollisions(collider.Owner.ExternalData as CollidableObject, collidee.Owner.ExternalData as CollidableObject);
            return true;
        }

        //want do we want to do now that we have collided with an object?
        private void HandleCollisions(CollidableObject collidableObjectCollider, CollidableObject collidableObjectCollidee)
        {
            AudioEmitter audioEmitter = new AudioEmitter();
            audioEmitter.Position = collidableObjectCollider.Transform.Translation;
            //did the "as" typecast return a valid object?
            if (collidableObjectCollidee != null)
            {
                if (collidableObjectCollidee.ActorType == ActorType.Ice)
                {
                    this.moveSpeed = 0.7f;
                    //Console.WriteLine("Ice");
                    object[] additionalParameter = { true };
                    EventDispatcher.Publish(new EventData(EventActionType.OnIce, EventCategoryType.Obstacle, additionalParameter));
                    this.bOnce = true;
                }
                else if (collidableObjectCollidee.ActorType == ActorType.Snow)
                {

                    
                    if (this.shovelEquipped)
                    {
                        
                        object[] additionalParameters = { "Oof",audioEmitter };
                        EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound3D, additionalParameters));
                        EventDispatcher.Publish(new EventData(collidableObjectCollidee, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                        this.shovelEquipped = false;
                        this.bOnce = true;
                    }

                }
                else if (collidableObjectCollidee.ActorType == ActorType.Goal)
                {

                    EventDispatcher.Publish(new EventData("You WIN!", this, EventActionType.OnGameWin, EventCategoryType.GameWon));
                }
                else if (collidableObjectCollidee.ActorType == ActorType.CollidableGround)
                {
                    this.stunned = false;
                    //Console.WriteLine("mov");
                    this.moveSpeed = 0.7f;
                    if (this.bOnce)
                    {
                        Console.WriteLine("ground");
                        EventDispatcher.Publish(new EventData(EventActionType.OnGround, EventCategoryType.Obstacle));
                        this.bOnce = false;
                    }
                }

                //add else if statements here for all the responses that you want your player to have
                //else if (collidableObjectCollidee.ActorType == ActorType.CollidableAmmo)
                //{

                //}
            }
        }

        protected override void HandleKeyboardInput(GameTime gameTime)
        {

            //jump
            if (!this.stunned)
            {
                //forward/backward
                if (this.KeyboardManager.IsKeyDown(this.MoveKeys[0]))
                {
                    this.CharacterBody.Velocity += this.Transform.Look * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    this.AnimationState = AnimationStateType.Running;
                }
                else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[1]))
                {
                    this.CharacterBody.Velocity -= this.Transform.Look * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    this.AnimationState = AnimationStateType.Running;
                }

                //strafe left/right
                if (this.KeyboardManager.IsKeyDown(this.MoveKeys[2]))
                {
                    this.CharacterBody.Velocity -= this.Transform.Right * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    this.AnimationState = AnimationStateType.Running;
                }
                else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[3]))
                {
                    this.CharacterBody.Velocity += this.Transform.Right * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    this.AnimationState = AnimationStateType.Running;
                }

                if (!this.KeyboardManager.IsAnyKeyPressed())
                    this.CharacterBody.DesiredVelocity = Vector3.Zero;

                if (this.CharacterBody.Velocity.Length() < DefaultMinimumMoveVelocity)
                    this.AnimationState = AnimationStateType.Idle;

                //update the camera position to reflect the collision skin position
                this.Transform.Translation = this.CharacterBody.Position;


            }
            SetAnimationByInput();

        }

        protected override void SetAnimationByInput()
        {
            switch (this.AnimationState)
            {
                case AnimationStateType.Running:
                    SetAnimation("Take 001", "char_walk");
                    break;

                case AnimationStateType.Idle:
                    SetAnimation("Take 001", "char_idle");
                    break;
            }

        }
    }
}
