﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

partial class Shotgun : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int ClipSize => 8;
	public override float ReloadTime => 0.5f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" ); 

		AmmoClip = 6;
	}

	public override void AttackPrimary( Player owner )
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		Owner.SetAnimParam( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();

		//
		// Shoot the bullets
		//
		for ( int i = 0; i < 10; i++ )
		{
			ShootBullet( 0.25f, 0.3f, 7.0f, 3.0f );
		}
	}

	public override void AttackSecondary( Player owner )
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		if ( !TakeAmmo( 2 ) )
		{
			DryFire();
			return;
		}

		Owner.SetAnimParam( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();

		//
		// Shoot the bullets
		//
		for ( int i = 0; i < 20; i++ )
		{
			ShootBullet( 0.5f, 0.3f, 5.0f, 3.0f );
		}
	}

	[Client]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		PlaySound( "rust_pumpshotgun.shoot" );
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimParam( "fire", true );
	}

	[Client]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		PlaySound( "rust_pumpshotgun.shootdouble" );
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParam( "fire_double", true );
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is DeathmatchPlayer player )
		{
			var ammo = player.TakeAmmo( AmmoType, 1 );
			if ( ammo == 0 )
				return;

			AmmoClip += ammo;

			if ( AmmoClip < ClipSize )
			{
				Reload( Owner );
			}
			else
			{
				FinishReload();
			}
		}
	}

	[Client]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimParam( "reload_finished", true );
	}

	public override void TickPlayerAnimator( PlayerAnimator anim )
	{
		anim.SetParam( "holdtype", 2 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
