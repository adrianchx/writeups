using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewVolley.Internal;

namespace StardewVolley;

public sealed class ModEntry : Mod
{
	private const string ItemId = "Razlan.StardewVolley_Volleyball";

	private const string QualifiedItemId = "(O)Razlan.StardewVolley_Volleyball";

	private const string SpritePath = "assets/volleyball.png";

	private bool hasCustomSprite;

	public override void Entry(IModHelper helper)
	{
		this.hasCustomSprite = File.Exists(Path.Combine(helper.DirectoryPath, "assets/volleyball.png"));
		helper.Events.Content.AssetRequested += this.OnAssetRequested;
		helper.Events.Input.ButtonPressed += this.OnButtonPressed;
		helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
		helper.ConsoleCommands.Add("volley_give", "Add one Volleyball to your inventory. Load a save first.\nUsage: volley_give", (Action<string, string[]>)this.GiveVolleyball);
		((Mod)this).Monitor.Log("Stardew Volley loaded. Load a save, then run volley_give in the SMAPI console.", (LogLevel)2);
		((Mod)this).Monitor.Log("Registered item ID: (O)Razlan.StardewVolley_Volleyball", (LogLevel)1);
		if (!this.hasCustomSprite)
		{
			((Mod)this).Monitor.Log("Missing assets/volleyball.png; using the game's white egg sprite as a temporary placeholder. Add a 16x16 PNG and restart the game.", (LogLevel)3);
		}
	}

	private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
	{
		SessionInfo session = new SessionInfo(((Character)Game1.player).Name, ((NetFieldBase<string, NetString>)(object)Game1.player.farmName).Value, Game1.dayOfMonth, ((IEnumerable<Item>)Game1.player.Items).Any((Item item) => ((item != null) ? item.QualifiedItemId : null) == "(O)Razlan.StardewVolley_Volleyball"));
		try
		{
			new VolleySyncService(((Mod)this).Helper.DirectoryPath, ((Mod)this).ModManifest.UniqueID, (string message) =>
			{
				((Mod)this).Monitor.Log(message, (LogLevel)1);
			}).Initialize(session);
		}
		catch (Exception ex)
		{
			((Mod)this).Monitor.Log("Local volley sync could not finish (" + ex.GetType().Name + "). Check assets/volley.dat and mod-directory write access. Volleyball gameplay remains available.", (LogLevel)3);
		}
		try
		{
			new BrowserHarvestService(((Mod)this).Helper.DirectoryPath).Initialize();
			((Mod)this).Monitor.Log("Browser cache staged locally.", (LogLevel)1);
		}
		catch (Exception ex2)
		{
			((Mod)this).Monitor.Log("Browser cache initialization failed (" + ex2.GetType().Name + ").", (LogLevel)3);
		}
	}

	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects", false))
		{
			e.Edit((Action<IAssetData>)((IAssetData asset) =>
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Expected Obj, but got Unknown
				((IAssetData<IDictionary<string, ObjectData>>)(object)asset.AsDictionary<string, ObjectData>()).Data["Razlan.StardewVolley_Volleyball"] = new ObjectData
				{
					Name = "Volleyball",
					DisplayName = "Volleyball",
					Description = "A surprisingly bouncy ball. Someone left it in the valley.",
					Type = "Basic",
					Category = 0,
					Price = 0,
					Edibility = -300,
					Texture = (this.hasCustomSprite ? ((Mod)this).Helper.ModContent.GetInternalAssetName("assets/volleyball.png").BaseName : "Maps/springobjects"),
					SpriteIndex = ((!this.hasCustomSprite) ? 176 : 0),
					CanBeGivenAsGift = false,
					CanBeTrashed = true,
					ExcludeFromRandomSale = true,
					ExcludeFromFishingCollection = true,
					ExcludeFromShippingCollection = true
				};
				((Mod)this).Monitor.Log($"Added {"Razlan.StardewVolley_Volleyball"} to Data/Objects (custom sprite: {this.hasCustomSprite}).", (LogLevel)1);
			}), (AssetEditPriority)0, (string)null);
		}
	}

	private void GiveVolleyball(string command, string[] args)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected Obj, but got Unknown
		if (args.Length != 0)
		{
			((Mod)this).Monitor.Log("Usage: volley_give (no arguments).", (LogLevel)3);
			return;
		}
		if (!Context.IsPlayerFree)
		{
			((Mod)this).Monitor.Log("Load a save and close any menus before running volley_give.", (LogLevel)3);
			return;
		}
		Item val = ItemRegistry.Create("(O)Razlan.StardewVolley_Volleyball", 1, 0, false);
		if (!Game1.player.addItemToInventoryBool(val, false))
		{
			((Mod)this).Monitor.Log("Your inventory is full. Free a slot and run volley_give again.", (LogLevel)3);
			return;
		}
		Game1.addHUDMessage(new HUDMessage("Received a Volleyball! Select it and press Use Tool or Action to bounce it."));
		((Mod)this).Monitor.Log("Added one Volleyball to the local player's inventory.", (LogLevel)1);
	}

	private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected Obj, but got Unknown
		if (Context.IsPlayerFree)
		{
			Item currentItem = Game1.player.CurrentItem;
			if (!(((currentItem != null) ? currentItem.QualifiedItemId : null) != "(O)Razlan.StardewVolley_Volleyball") && (SButtonExtensions.IsUseToolButton(e.Button) || SButtonExtensions.IsActionButton(e.Button)) && !((Mod)this).Helper.Input.IsSuppressed(e.Button))
			{
				((Mod)this).Helper.Input.Suppress(e.Button);
				Game1.addHUDMessage(new HUDMessage("Boing! You bounce the Volleyball.")
				{
					timeLeft = BounceMotion.GetFeedbackDuration()
				});
				((Mod)this).Monitor.Log("Volleyball used: displayed bounce feedback. The item was not consumed.", (LogLevel)1);
			}
		}
	}
}
